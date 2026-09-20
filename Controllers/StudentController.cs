using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;
using StudentPlacementManagement.ViewModels;

namespace StudentPlacementManagement.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentService _studentService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStudentService studentService,
            INotificationService notificationService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _studentService = studentService;
            _notificationService = notificationService;
            _environment = environment;
        }

        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return NotFound("Student profile record not found.");
            }

            var completionPct = _studentService.CalculateProfileCompletion(student);

            // Student statistics
            var myApplications = await _context.Applications
                .AsNoTracking()
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Where(a => a.StudentId == student.StudentId)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            var appliedDriveIds = myApplications.Select(a => a.DriveId).ToList();

            var availableDrives = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .Include(d => d.DriveDepartments)
                .Where(d => d.Status == "Open" && d.ApplicationEndDate >= DateTime.UtcNow)
                .Where(d => !appliedDriveIds.Contains(d.DriveId))
                .Where(d => d.DriveDepartments.Any(dd => dd.DepartmentId == student.DepartmentId))
                .OrderBy(d => d.ApplicationEndDate)
                .Take(5)
                .ToListAsync();

            var appliedCount = myApplications.Count;
            var shortlistedCount = myApplications.Count(a => a.Status == "Shortlisted");
            var selectedCount = myApplications.Count(a => a.Status == "Selected");

            // Upcoming interviews for drives where student is Shortlisted or Applied
            var activeDriveIds = myApplications
                .Where(a => a.Status == "Shortlisted" || a.Status == "Applied")
                .Select(a => a.DriveId)
                .ToList();

            var upcomingInterviews = await _context.InterviewRounds
                .AsNoTracking()
                .Include(r => r.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Where(r => activeDriveIds.Contains(r.DriveId) && r.RoundDate >= DateTime.UtcNow.AddHours(-12))
                .OrderBy(r => r.RoundDate)
                .Take(5)
                .ToListAsync();

            var recentNotifications = await _notificationService.GetUserNotificationsAsync(userId, 5);

            var vm = new StudentDashboardViewModel
            {
                Student = student,
                ProfileCompletionPercentage = completionPct,
                AvailableDrivesCount = availableDrives.Count,
                AppliedDrivesCount = appliedCount,
                ShortlistedDrivesCount = shortlistedCount,
                SelectedDrivesCount = selectedCount,
                UpcomingDrives = availableDrives,
                RecentApplications = myApplications.Take(5).ToList(),
                UpcomingInterviews = upcomingInterviews,
                RecentNotifications = recentNotifications
            };

            return View(vm);
        }

        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> Profile(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            Student? student;
            if (id.HasValue && User.IsInRole("Admin"))
            {
                // Admin can view any student profile
                student = await _studentService.GetStudentByIdAsync(id.Value);
            }
            else
            {
                // Student can only access their own profile
                student = await _studentService.GetStudentByUserIdAsync(user.Id);
            }

            if (student == null) return NotFound("Student profile not found.");

            var vm = MapToViewModel(student);
            vm.ProfileCompletionPercentage = _studentService.CalculateProfileCompletion(student);

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> EditProfile(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            Student? student;
            if (id.HasValue && User.IsInRole("Admin"))
            {
                student = await _studentService.GetStudentByIdAsync(id.Value);
            }
            else
            {
                student = await _studentService.GetStudentByUserIdAsync(user.Id);
            }

            if (student == null) return NotFound("Student record not found.");

            var vm = MapToViewModel(student);
            vm.DepartmentList = await GetDepartmentSelectListAsync();
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Student,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Security check: verify ownership
            if (!User.IsInRole("Admin"))
            {
                var ownStudent = await _studentService.GetStudentByUserIdAsync(user.Id);
                if (ownStudent == null || ownStudent.StudentId != model.StudentId)
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                model.DepartmentList = await GetDepartmentSelectListAsync();
                return View(model);
            }

            var studentToUpdate = new Student
            {
                StudentId = model.StudentId,
                EnrollmentNo = model.EnrollmentNo.Trim().ToUpper(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                State = model.State,
                DepartmentId = model.DepartmentId,
                PassingYear = model.PassingYear,
                CGPA = model.CGPA,
                Backlogs = model.Backlogs
            };

            var (success, errorMessage) = await _studentService.UpdateProfileAsync(studentToUpdate);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to update profile.");
                model.DepartmentList = await GetDepartmentSelectListAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile details updated successfully!";
            return RedirectToAction(nameof(Profile), new { id = model.StudentId });
        }

        [HttpPost]
        [Authorize(Roles = "Student,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResume(int studentId, IFormFile resumeFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Security ownership validation
            if (!User.IsInRole("Admin"))
            {
                var currentStudent = await _studentService.GetStudentByUserIdAsync(user.Id);
                if (currentStudent == null || currentStudent.StudentId != studentId)
                {
                    return Forbid();
                }
            }

            var (success, fileName, filePath, errorMessage) = await _studentService.UploadResumeAsync(studentId, resumeFile);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = $"Resume '{fileName}' uploaded successfully!";
            }

            return RedirectToAction(nameof(Profile), new { id = studentId });
        }

        [HttpGet]
        [Authorize(Roles = "Student,Admin,Recruiter")]
        public async Task<IActionResult> DownloadResume(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null || string.IsNullOrEmpty(student.ResumePath))
            {
                return NotFound("Resume file not found.");
            }

            // Security check: non-admin student can only download their own resume
            if (User.IsInRole("Student") && !User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || student.UserId != user.Id)
                {
                    return Forbid();
                }
            }

            var cleanPath = student.ResumePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_environment.WebRootPath, cleanPath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File does not exist on disk.");
            }

            var contentType = "application/pdf";
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".doc") contentType = "application/msword";
            else if (ext == ".docx") contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            return PhysicalFile(filePath, contentType, student.ResumeFileName ?? "Student_Resume.pdf");
        }

        // Admin: Student directory
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? search, int? departmentId, string? placementStatus, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.FirstName.ToLower().Contains(term)
                    || s.LastName.ToLower().Contains(term)
                    || s.EnrollmentNo.ToLower().Contains(term)
                    || s.Email.ToLower().Contains(term));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(placementStatus))
            {
                query = query.Where(s => s.PlacementStatus == placementStatus);
            }

            var totalCount = await query.CountAsync();
            var students = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.PlacementStatus = placementStatus;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Departments = await GetDepartmentSelectListAsync();

            return View(students);
        }

        private StudentProfileViewModel MapToViewModel(Student s)
        {
            return new StudentProfileViewModel
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                EnrollmentNo = s.EnrollmentNo,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Gender = s.Gender,
                DateOfBirth = s.DateOfBirth,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                City = s.City,
                State = s.State,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department?.DepartmentName,
                PassingYear = s.PassingYear,
                CGPA = s.CGPA,
                Backlogs = s.Backlogs,
                ResumeFileName = s.ResumeFileName,
                ResumePath = s.ResumePath,
                PlacementStatus = s.PlacementStatus
            };
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentSelectListAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = $"{d.DepartmentName} ({d.DepartmentCode})"
                })
                .ToListAsync();
        }
    }
}
