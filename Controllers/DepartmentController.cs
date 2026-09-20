using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Students)
                .Include(d => d.DriveDepartments)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Department());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            var codeExists = await _context.Departments
                .AnyAsync(d => d.DepartmentCode.ToLower() == department.DepartmentCode.Trim().ToLower());

            if (codeExists)
            {
                ModelState.AddModelError(nameof(department.DepartmentCode), "A department with this code already exists.");
                return View(department);
            }

            department.DepartmentCode = department.DepartmentCode.Trim().ToUpper();
            department.DepartmentName = department.DepartmentName.Trim();
            department.CreatedAt = DateTime.UtcNow;

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            var existing = await _context.Departments.FindAsync(id);
            if (existing == null) return NotFound();

            var codeExists = await _context.Departments
                .AnyAsync(d => d.DepartmentCode.ToLower() == department.DepartmentCode.Trim().ToLower() && d.DepartmentId != id);

            if (codeExists)
            {
                ModelState.AddModelError(nameof(department.DepartmentCode), "A department with this code already exists.");
                return View(department);
            }

            existing.DepartmentName = department.DepartmentName.Trim();
            existing.DepartmentCode = department.DepartmentCode.Trim().ToUpper();

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Department details updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Students)
                .Include(d => d.DriveDepartments)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null) return NotFound();

            if (department.Students.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{department.DepartmentName}' because it has {department.Students.Count} registered student(s).";
                return RedirectToAction(nameof(Index));
            }

            if (department.DriveDepartments.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{department.DepartmentName}' because it is associated with placement drives.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Department deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
