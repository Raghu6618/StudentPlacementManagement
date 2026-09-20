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
    public class PlacementDriveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEligibilityService _eligibilityService;
        private readonly IStudentService _studentService;

        public PlacementDriveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEligibilityService eligibilityService,
            IStudentService studentService)
        {
            _context = context;
            _userManager = userManager;
            _eligibilityService = eligibilityService;
            _studentService = studentService;
        }

        public async Task<IActionResult> Index(string? status, int? departmentId, string? search, int page = 1)
        {
            int pageSize = 9;
            var query = _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .Include(d => d.DriveDepartments)
                    .ThenInclude(dd => dd.Department)
                .Include(d => d.Applications)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(d => d.Status == status);
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(d => d.DriveDepartments.Any(dd => dd.DepartmentId == departmentId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(d => d.JobRole.ToLower().Contains(term)
                    || (d.Company != null && d.Company.CompanyName.ToLower().Contains(term))
                    || (d.JobLocation != null && d.JobLocation.ToLower().Contains(term))
                    || (d.RequiredSkills != null && d.RequiredSkills.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var drives = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            // If user is a student, check their eligibility status for each drive in memory (0 extra queries)
            if (User.IsInRole("Student"))
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var student = await _studentService.GetStudentByUserIdAsync(userId);
                    if (student != null)
                    {
                        var studentEligibilityMap = new Dictionary<int, EligibilityResult>();
                        var appliedDriveIds = await _context.Applications
                            .AsNoTracking()
                            .Where(a => a.StudentId == student.StudentId)
                            .Select(a => a.DriveId)
                            .ToListAsync();

                        foreach (var d in drives)
                        {
                            var res = _eligibilityService.EvaluateEligibility(student, d, appliedDriveIds.Contains(d.DriveId));
                            studentEligibilityMap[d.DriveId] = res;
                        }

                        ViewBag.StudentEligibilityMap = studentEligibilityMap;
                        ViewBag.AppliedDriveIds = appliedDriveIds;
                    }
                }
            }

            return View(drives);
        }

        public async Task<IActionResult> Details(int id)
        {
            var drive = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .Include(d => d.DriveDepartments)
                    .ThenInclude(dd => dd.Department)
                .Include(d => d.InterviewRounds)
                .Include(d => d.Applications)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s!.Department)
                .FirstOrDefaultAsync(d => d.DriveId == id);

            if (drive == null) return NotFound();

            var vm = new PlacementDriveDetailsViewModel
            {
                Drive = drive,
                TotalApplicantsCount = drive.Applications.Count,
                InterviewRounds = drive.InterviewRounds.OrderBy(r => r.RoundNumber).ToList()
            };

            // If logged-in user is Student
            if (User.IsInRole("Student"))
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var student = await _studentService.GetStudentByUserIdAsync(userId);
                    if (student != null)
                    {
                        vm.ExistingApplication = drive.Applications.FirstOrDefault(a => a.StudentId == student.StudentId);
                        vm.HasAlreadyApplied = vm.ExistingApplication != null;
                        vm.UserEligibility = _eligibilityService.EvaluateEligibility(student, drive, vm.HasAlreadyApplied);
                    }
                }
            }
            else if (User.IsInRole("Admin"))
            {
                var eligibleStudentIds = await _eligibilityService.GetEligibleStudentIdsAsync(drive.DriveId);
                vm.EligibleStudentsCount = eligibleStudentIds.Count;
            }

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new PlacementDriveCreateViewModel
            {
                DriveDate = DateTime.UtcNow.AddDays(14),
                ApplicationStartDate = DateTime.UtcNow,
                ApplicationEndDate = DateTime.UtcNow.AddDays(7),
                PassingYear = DateTime.UtcNow.Year,
                CompanyList = await GetCompanySelectListAsync(),
                DepartmentList = await GetDepartmentMultiSelectListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlacementDriveCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CompanyList = await GetCompanySelectListAsync();
                model.DepartmentList = await GetDepartmentMultiSelectListAsync();
                return View(model);
            }

            if (model.ApplicationEndDate <= model.ApplicationStartDate)
            {
                ModelState.AddModelError(nameof(model.ApplicationEndDate), "Application End Date must be after Application Start Date.");
                model.CompanyList = await GetCompanySelectListAsync();
                model.DepartmentList = await GetDepartmentMultiSelectListAsync();
                return View(model);
            }

            var drive = new PlacementDrive
            {
                CompanyId = model.CompanyId,
                JobRole = model.JobRole.Trim(),
                JobDescription = model.JobDescription.Trim(),
                JobLocation = model.JobLocation?.Trim(),
                EmploymentType = model.EmploymentType,
                SalaryPackage = model.SalaryPackage,
                MinCGPA = model.MinCGPA,
                MaxBacklogs = model.MaxBacklogs,
                PassingYear = model.PassingYear,
                DriveDate = model.DriveDate,
                ApplicationStartDate = model.ApplicationStartDate,
                ApplicationEndDate = model.ApplicationEndDate,
                RequiredSkills = model.RequiredSkills?.Trim(),
                EligibilityDescription = model.EligibilityDescription?.Trim(),
                Status = model.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.PlacementDrives.Add(drive);
            await _context.SaveChangesAsync();

            // Link eligible departments
            if (model.SelectedDepartmentIds != null && model.SelectedDepartmentIds.Any())
            {
                foreach (var deptId in model.SelectedDepartmentIds)
                {
                    _context.DriveDepartments.Add(new DriveDepartment
                    {
                        DriveId = drive.DriveId,
                        DepartmentId = deptId
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Placement Drive for '{drive.JobRole}' created successfully!";
            return RedirectToAction(nameof(Details), new { id = drive.DriveId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var drive = await _context.PlacementDrives
                .Include(d => d.DriveDepartments)
                .FirstOrDefaultAsync(d => d.DriveId == id);

            if (drive == null) return NotFound();

            var vm = new PlacementDriveCreateViewModel
            {
                DriveId = drive.DriveId,
                CompanyId = drive.CompanyId,
                JobRole = drive.JobRole,
                JobDescription = drive.JobDescription,
                JobLocation = drive.JobLocation,
                EmploymentType = drive.EmploymentType,
                SalaryPackage = drive.SalaryPackage,
                MinCGPA = drive.MinCGPA,
                MaxBacklogs = drive.MaxBacklogs,
                PassingYear = drive.PassingYear,
                DriveDate = drive.DriveDate,
                ApplicationStartDate = drive.ApplicationStartDate,
                ApplicationEndDate = drive.ApplicationEndDate,
                RequiredSkills = drive.RequiredSkills,
                EligibilityDescription = drive.EligibilityDescription,
                Status = drive.Status,
                SelectedDepartmentIds = drive.DriveDepartments.Select(dd => dd.DepartmentId).ToList(),
                CompanyList = await GetCompanySelectListAsync(),
                DepartmentList = await GetDepartmentMultiSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlacementDriveCreateViewModel model)
        {
            if (id != model.DriveId) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.CompanyList = await GetCompanySelectListAsync();
                model.DepartmentList = await GetDepartmentMultiSelectListAsync();
                return View(model);
            }

            var drive = await _context.PlacementDrives
                .Include(d => d.DriveDepartments)
                .FirstOrDefaultAsync(d => d.DriveId == id);

            if (drive == null) return NotFound();

            drive.CompanyId = model.CompanyId;
            drive.JobRole = model.JobRole.Trim();
            drive.JobDescription = model.JobDescription.Trim();
            drive.JobLocation = model.JobLocation?.Trim();
            drive.EmploymentType = model.EmploymentType;
            drive.SalaryPackage = model.SalaryPackage;
            drive.MinCGPA = model.MinCGPA;
            drive.MaxBacklogs = model.MaxBacklogs;
            drive.PassingYear = model.PassingYear;
            drive.DriveDate = model.DriveDate;
            drive.ApplicationStartDate = model.ApplicationStartDate;
            drive.ApplicationEndDate = model.ApplicationEndDate;
            drive.RequiredSkills = model.RequiredSkills?.Trim();
            drive.EligibilityDescription = model.EligibilityDescription?.Trim();
            drive.Status = model.Status;
            drive.UpdatedAt = DateTime.UtcNow;

            // Sync departments
            _context.DriveDepartments.RemoveRange(drive.DriveDepartments);
            if (model.SelectedDepartmentIds != null && model.SelectedDepartmentIds.Any())
            {
                foreach (var deptId in model.SelectedDepartmentIds)
                {
                    _context.DriveDepartments.Add(new DriveDepartment
                    {
                        DriveId = drive.DriveId,
                        DepartmentId = deptId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Placement Drive updated successfully!";
            return RedirectToAction(nameof(Details), new { id = drive.DriveId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EligibleStudents(int id)
        {
            var drive = await _context.PlacementDrives
                .Include(d => d.Company)
                .Include(d => d.DriveDepartments)
                    .ThenInclude(dd => dd.Department)
                .FirstOrDefaultAsync(d => d.DriveId == id);

            if (drive == null) return NotFound();

            var eligibleIds = await _eligibilityService.GetEligibleStudentIdsAsync(id);

            var students = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Applications)
                .Where(s => eligibleIds.Contains(s.StudentId))
                .OrderByDescending(s => s.CGPA)
                .ToListAsync();

            ViewBag.Drive = drive;
            ViewBag.AppliedStudentIds = await _context.Applications
                .Where(a => a.DriveId == id)
                .Select(a => a.StudentId)
                .ToListAsync();

            return View(students);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var drive = await _context.PlacementDrives
                .Include(d => d.Applications)
                .Include(d => d.Placements)
                .FirstOrDefaultAsync(d => d.DriveId == id);

            if (drive == null) return NotFound();

            if (drive.Applications.Any() || drive.Placements.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete drive with active applications or placements. You can change its status to 'Cancelled' instead.";
                return RedirectToAction(nameof(Details), new { id = drive.DriveId });
            }

            _context.PlacementDrives.Remove(drive);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Placement Drive deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetCompanySelectListAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetDepartmentMultiSelectListAsync()
        {
            return await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = $"{d.DepartmentName} ({d.DepartmentCode})" })
                .ToListAsync();
        }
    }
}
