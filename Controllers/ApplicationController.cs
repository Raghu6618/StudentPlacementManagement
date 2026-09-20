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
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEligibilityService _eligibilityService;
        private readonly IStudentService _studentService;
        private readonly IPlacementService _placementService;
        private readonly INotificationService _notificationService;

        public ApplicationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEligibilityService eligibilityService,
            IStudentService studentService,
            IPlacementService placementService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _eligibilityService = eligibilityService;
            _studentService = studentService;
            _placementService = placementService;
            _notificationService = notificationService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? driveId, int? companyId, int? departmentId, string? status, string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Applications
                .AsNoTracking()
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s!.Department)
                .AsQueryable();

            if (driveId.HasValue && driveId.Value > 0)
                query = query.Where(a => a.DriveId == driveId.Value);

            if (companyId.HasValue && companyId.Value > 0)
                query = query.Where(a => a.PlacementDrive != null && a.PlacementDrive.CompanyId == companyId.Value);

            if (departmentId.HasValue && departmentId.Value > 0)
                query = query.Where(a => a.Student != null && a.Student.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => (a.Student != null && (a.Student.FirstName.ToLower().Contains(term)
                    || a.Student.LastName.ToLower().Contains(term)
                    || a.Student.EnrollmentNo.ToLower().Contains(term)))
                    || (a.PlacementDrive != null && a.PlacementDrive.JobRole.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var applications = await query
                .OrderByDescending(a => a.AppliedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ApplicationViewModel
                {
                    ApplicationId = a.ApplicationId,
                    DriveId = a.DriveId,
                    CompanyName = a.PlacementDrive!.Company!.CompanyName,
                    CompanyLogoPath = a.PlacementDrive.Company.LogoPath,
                    JobRole = a.PlacementDrive.JobRole,
                    SalaryPackage = a.PlacementDrive.SalaryPackage,
                    DriveDate = a.PlacementDrive.DriveDate,
                    StudentId = a.StudentId,
                    StudentName = a.Student!.FullName,
                    EnrollmentNo = a.Student.EnrollmentNo,
                    DepartmentName = a.Student.Department!.DepartmentName,
                    StudentCGPA = a.Student.CGPA,
                    StudentBacklogs = a.Student.Backlogs,
                    ResumePath = a.Student.ResumePath,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            var vm = new ApplicationFilterViewModel
            {
                DriveId = driveId,
                CompanyId = companyId,
                DepartmentId = departmentId,
                Status = status,
                SearchTerm = search,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                Applications = applications
            };

            ViewBag.Companies = await _context.Companies
                .AsNoTracking()
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName })
                .ToListAsync();

            ViewBag.Drives = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new SelectListItem { Value = d.DriveId.ToString(), Text = $"{d.Company!.CompanyName} - {d.JobRole}" })
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            return View(vm);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null) return NotFound("Student record not found.");

            var applications = await _context.Applications
                .AsNoTracking()
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s!.Department)
                .Where(a => a.StudentId == student.StudentId)
                .OrderByDescending(a => a.AppliedDate)
                .Select(a => new ApplicationViewModel
                {
                    ApplicationId = a.ApplicationId,
                    DriveId = a.DriveId,
                    CompanyName = a.PlacementDrive!.Company!.CompanyName,
                    CompanyLogoPath = a.PlacementDrive.Company.LogoPath,
                    JobRole = a.PlacementDrive.JobRole,
                    SalaryPackage = a.PlacementDrive.SalaryPackage,
                    DriveDate = a.PlacementDrive.DriveDate,
                    StudentId = a.StudentId,
                    StudentName = a.Student!.FullName,
                    EnrollmentNo = a.Student.EnrollmentNo,
                    DepartmentName = a.Student.Department!.DepartmentName,
                    StudentCGPA = a.Student.CGPA,
                    StudentBacklogs = a.Student.Backlogs,
                    ResumePath = a.Student.ResumePath,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int driveId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null) return NotFound("Student profile record not found.");

            // 1. Verify profile completeness (Resume must be uploaded)
            if (string.IsNullOrEmpty(student.ResumePath))
            {
                TempData["ErrorMessage"] = "Please upload your resume in your Profile before applying to placement drives.";
                return RedirectToAction("Profile", "Student", new { id = student.StudentId });
            }

            // 2. Run Eligibility Check
            var eligibility = await _eligibilityService.CheckEligibilityAsync(student.StudentId, driveId);
            if (!eligibility.IsEligible)
            {
                TempData["ErrorMessage"] = $"Cannot submit application: {eligibility.Reason}";
                return RedirectToAction("Details", "PlacementDrive", new { id = driveId });
            }

            // 3. Prevent duplicate application
            var existingApp = await _context.Applications
                .AnyAsync(a => a.DriveId == driveId && a.StudentId == student.StudentId);

            if (existingApp)
            {
                TempData["WarningMessage"] = "You have already applied to this placement drive.";
                return RedirectToAction("Details", "PlacementDrive", new { id = driveId });
            }

            var application = new Application
            {
                DriveId = driveId,
                StudentId = student.StudentId,
                AppliedDate = DateTime.UtcNow,
                Status = "Applied",
                Remarks = "Submitted by student via portal"
            };

            _context.Applications.Add(application);

            var drive = await _context.PlacementDrives
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);

            // Send confirmation notification
            await _notificationService.SendNotificationAsync(
                userId,
                "Application Submitted Successfully",
                $"Your application for {drive?.JobRole} at {drive?.Company?.CompanyName} has been submitted successfully.",
                "Application"
            );

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your application for '{drive?.JobRole}' at {drive?.Company?.CompanyName} has been submitted successfully!";
            return RedirectToAction(nameof(MyApplications));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, string newStatus, string? remarks, decimal? packageOffered, DateTime? joiningDate)
        {
            var user = await _userManager.GetUserAsync(User);
            var adminId = user?.Id;

            if (newStatus == "Selected")
            {
                var (success, error) = await _placementService.SelectStudentAsync(applicationId, packageOffered, joiningDate, adminId);
                if (!success)
                {
                    TempData["ErrorMessage"] = error;
                }
                else
                {
                    TempData["SuccessMessage"] = "Student marked as Selected and Placement record created successfully!";
                }
            }
            else if (newStatus == "Shortlisted")
            {
                var (success, error) = await _placementService.ShortlistStudentAsync(applicationId, remarks, adminId);
                if (!success) TempData["ErrorMessage"] = error;
                else TempData["SuccessMessage"] = "Application shortlisted successfully.";
            }
            else if (newStatus == "Rejected")
            {
                var (success, error) = await _placementService.RejectStudentAsync(applicationId, remarks, adminId);
                if (!success) TempData["ErrorMessage"] = error;
                else TempData["SuccessMessage"] = "Application updated to Rejected.";
            }
            else
            {
                var app = await _context.Applications.FindAsync(applicationId);
                if (app != null)
                {
                    app.Status = newStatus;
                    app.Remarks = remarks;
                    app.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Application status updated to {newStatus}.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
