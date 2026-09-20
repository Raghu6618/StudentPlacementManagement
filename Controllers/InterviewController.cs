using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;
using StudentPlacementManagement.ViewModels;

namespace StudentPlacementManagement.Controllers
{
    [Authorize]
    public class InterviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IStudentService _studentService;

        public InterviewController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            IStudentService studentService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _studentService = studentService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Rounds(int driveId)
        {
            var drive = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .Include(d => d.InterviewRounds)
                    .ThenInclude(r => r.RoundResults)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);

            if (drive == null) return NotFound();

            return View(drive);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRound(int driveId)
        {
            var drive = await _context.PlacementDrives
                .Include(d => d.Company)
                .Include(d => d.InterviewRounds)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);

            if (drive == null) return NotFound();

            var nextRoundNum = drive.InterviewRounds.Any()
                ? drive.InterviewRounds.Max(r => r.RoundNumber) + 1
                : 1;

            var vm = new InterviewRoundCreateViewModel
            {
                DriveId = drive.DriveId,
                DriveRole = drive.JobRole,
                CompanyName = drive.Company?.CompanyName,
                RoundNumber = nextRoundNum,
                RoundDate = DateTime.UtcNow.AddDays(2)
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRound(InterviewRoundCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roundExists = await _context.InterviewRounds
                .AnyAsync(r => r.DriveId == model.DriveId && r.RoundNumber == model.RoundNumber);

            if (roundExists)
            {
                ModelState.AddModelError(nameof(model.RoundNumber), $"Round #{model.RoundNumber} already exists for this drive.");
                return View(model);
            }

            var round = new InterviewRound
            {
                DriveId = model.DriveId,
                RoundName = model.RoundName.Trim(),
                RoundNumber = model.RoundNumber,
                RoundDate = model.RoundDate,
                Description = model.Description?.Trim(),
                IsRequired = model.IsRequired,
                CreatedAt = DateTime.UtcNow
            };

            _context.InterviewRounds.Add(round);
            await _context.SaveChangesAsync();

            // Populate pending results for all shortlisted or applied candidates of this drive
            var candidateStudentIds = await _context.Applications
                .Where(a => a.DriveId == model.DriveId && (a.Status == "Shortlisted" || a.Status == "Applied"))
                .Select(a => a.StudentId)
                .ToListAsync();

            foreach (var studentId in candidateStudentIds)
            {
                _context.RoundResults.Add(new RoundResult
                {
                    RoundId = round.RoundId,
                    StudentId = studentId,
                    Status = "Pending",
                    UpdatedAt = DateTime.UtcNow
                });

                // Notify candidate
                var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student?.User != null)
                {
                    await _notificationService.SendNotificationAsync(
                        student.User.Id,
                        $"📅 Interview Scheduled: {round.RoundName}",
                        $"Round {round.RoundNumber} ({round.RoundName}) has been scheduled on {round.RoundDate:dd MMM yyyy, hh:mm tt} UTC. {round.Description}",
                        "Interview"
                    );
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Interview Round '{round.RoundName}' scheduled successfully!";
            return RedirectToAction(nameof(Rounds), new { driveId = model.DriveId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Grading(int roundId)
        {
            var round = await _context.InterviewRounds
                .Include(r => r.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Include(r => r.RoundResults)
                    .ThenInclude(rr => rr.Student)
                        .ThenInclude(s => s!.Department)
                .FirstOrDefaultAsync(r => r.RoundId == roundId);

            if (round == null) return NotFound();

            // Make sure any newly shortlisted students also have a result record
            var candidates = await _context.Applications
                .Include(a => a.Student)
                    .ThenInclude(s => s!.Department)
                .Where(a => a.DriveId == round.DriveId && (a.Status == "Shortlisted" || a.Status == "Applied" || a.Status == "Selected"))
                .ToListAsync();

            var existingStudentIds = round.RoundResults.Select(rr => rr.StudentId).ToList();

            foreach (var app in candidates)
            {
                if (!existingStudentIds.Contains(app.StudentId))
                {
                    var newResult = new RoundResult
                    {
                        RoundId = round.RoundId,
                        StudentId = app.StudentId,
                        Status = "Pending",
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.RoundResults.Add(newResult);
                    round.RoundResults.Add(newResult);
                }
            }
            await _context.SaveChangesAsync();

            var candidateVms = round.RoundResults.Select(rr => new CandidateRoundEvaluationViewModel
            {
                RoundResultId = rr.RoundResultId,
                RoundId = rr.RoundId,
                StudentId = rr.StudentId,
                StudentName = rr.Student?.FullName ?? "Unknown",
                EnrollmentNo = rr.Student?.EnrollmentNo ?? "N/A",
                DepartmentName = rr.Student?.Department?.DepartmentName ?? "N/A",
                CGPA = rr.Student?.CGPA ?? 0m,
                ResumePath = rr.Student?.ResumePath,
                Status = rr.Status,
                Score = rr.Score,
                Remarks = rr.Remarks
            }).OrderBy(c => c.StudentName).ToList();

            var vm = new RoundGradingViewModel
            {
                Round = round,
                Candidates = candidateVms
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateResult(int roundResultId, string status, decimal? score, string? remarks)
        {
            var result = await _context.RoundResults
                .Include(r => r.InterviewRound)
                    .ThenInclude(ir => ir!.PlacementDrive)
                        .ThenInclude(d => d!.Company)
                .Include(r => r.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(r => r.RoundResultId == roundResultId);

            if (result == null) return NotFound();

            result.Status = status;
            result.Score = score;
            result.Remarks = remarks;
            result.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification to student
            if (result.Student?.User != null)
            {
                await _notificationService.SendNotificationAsync(
                    result.Student.User.Id,
                    $"Interview Result: {result.InterviewRound?.RoundName}",
                    $"Your status for {result.InterviewRound?.RoundName} ({result.InterviewRound?.PlacementDrive?.Company?.CompanyName}) is now: {status}. Score: {(score.HasValue ? score.Value.ToString("F1") : "N/A")}. Remarks: {remarks ?? "None"}",
                    "Result"
                );
            }

            TempData["SuccessMessage"] = $"Result for {result.Student?.FullName} updated to '{status}'.";
            return RedirectToAction(nameof(Grading), new { roundId = result.RoundId });
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyInterviews()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null) return NotFound();

            var appliedDriveIds = await _context.Applications
                .AsNoTracking()
                .Where(a => a.StudentId == student.StudentId)
                .Select(a => a.DriveId)
                .ToListAsync();

            var rounds = await _context.InterviewRounds
                .AsNoTracking()
                .Include(r => r.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Include(r => r.RoundResults.Where(rr => rr.StudentId == student.StudentId))
                .Where(r => appliedDriveIds.Contains(r.DriveId))
                .OrderBy(r => r.RoundDate)
                .ToListAsync();

            return View(rounds);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRound(int id)
        {
            var round = await _context.InterviewRounds
                .Include(r => r.RoundResults)
                .FirstOrDefaultAsync(r => r.RoundId == id);

            if (round == null) return NotFound();

            int driveId = round.DriveId;
            _context.InterviewRounds.Remove(round);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Interview Round deleted successfully.";
            return RedirectToAction(nameof(Rounds), new { driveId });
        }
    }
}
