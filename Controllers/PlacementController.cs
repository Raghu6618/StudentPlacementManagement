using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.Controllers
{
    [Authorize]
    public class PlacementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentService _studentService;
        private readonly INotificationService _notificationService;

        public PlacementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStudentService studentService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _studentService = studentService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int? departmentId, int? companyId, string? status, string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
                query = query.Where(p => p.Student != null && p.Student.DepartmentId == departmentId.Value);

            if (companyId.HasValue && companyId.Value > 0)
                query = query.Where(p => p.CompanyId == companyId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => (p.Student != null && (p.Student.FirstName.ToLower().Contains(term)
                    || p.Student.LastName.ToLower().Contains(term)
                    || p.Student.EnrollmentNo.ToLower().Contains(term)))
                    || (p.Company != null && p.Company.CompanyName.ToLower().Contains(term))
                    || p.JobRole.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();
            var placements = await query
                .OrderByDescending(p => p.PlacementDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.DepartmentId = departmentId;
            ViewBag.CompanyId = companyId;
            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            ViewBag.Departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            ViewBag.Companies = await _context.Companies
                .AsNoTracking()
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName })
                .ToListAsync();

            return View(placements);
        }

        public async Task<IActionResult> Details(int id)
        {
            var placement = await _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .FirstOrDefaultAsync(p => p.PlacementId == id);

            if (placement == null) return NotFound();

            // Security check
            if (User.IsInRole("Student") && !User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null || placement.Student?.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(placement);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyOffer()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var student = await _studentService.GetStudentByUserIdAsync(userId);
            if (student == null) return NotFound();

            var placements = await _context.Placements
                .AsNoTracking()
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .Where(p => p.StudentId == student.StudentId)
                .OrderByDescending(p => p.PlacementDate)
                .ToListAsync();

            return View(placements);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, DateTime? joiningDate, string? remarks)
        {
            var placement = await _context.Placements
                .Include(p => p.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(p => p.PlacementId == id);

            if (placement == null) return NotFound();

            placement.Status = status;
            if (joiningDate.HasValue) placement.JoiningDate = joiningDate.Value;
            if (!string.IsNullOrEmpty(remarks)) placement.Remarks = remarks;

            await _context.SaveChangesAsync();

            if (placement.Student?.User != null)
            {
                await _notificationService.SendNotificationAsync(
                    placement.Student.User.Id,
                    "Placement Offer Status Update",
                    $"Your placement status with {placement.Company?.CompanyName ?? "the company"} has been updated to: {status}.",
                    "Placement"
                );
            }

            TempData["SuccessMessage"] = "Placement offer details updated successfully!";
            return RedirectToAction(nameof(Details), new { id = placement.PlacementId });
        }
    }
}
