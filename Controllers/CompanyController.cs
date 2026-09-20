using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CompanyController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? search, string? industry, bool? activeOnly, int page = 1)
        {
            int pageSize = 9;
            var query = _context.Companies
                .AsNoTracking()
                .Include(c => c.PlacementDrives)
                .Include(c => c.Placements)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(c => c.IsActive);
            }
            else if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(c => c.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.CompanyName.ToLower().Contains(term)
                    || (c.Location != null && c.Location.ToLower().Contains(term))
                    || (c.Industry != null && c.Industry.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(industry))
            {
                query = query.Where(c => c.Industry == industry);
            }

            var totalCount = await query.CountAsync();
            var companies = await query
                .OrderBy(c => c.CompanyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Industry = industry;
            ViewBag.ActiveOnly = activeOnly;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Industries = await _context.Companies
                .AsNoTracking()
                .Where(c => !string.IsNullOrEmpty(c.Industry))
                .Select(c => c.Industry!)
                .Distinct()
                .ToListAsync();

            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .Include(c => c.PlacementDrives)
                    .ThenInclude(d => d.DriveDepartments)
                        .ThenInclude(dd => dd.Department)
                .Include(c => c.Placements)
                    .ThenInclude(p => p.Student)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null) return NotFound();

            return View(company);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Company());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company, IFormFile? logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(company);
            }

            var nameExists = await _context.Companies
                .AnyAsync(c => c.CompanyName.ToLower() == company.CompanyName.Trim().ToLower());

            if (nameExists)
            {
                ModelState.AddModelError(nameof(company.CompanyName), "A company with this name already exists.");
                return View(company);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
                var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
                if (allowedExts.Contains(ext))
                {
                    var logoDir = Path.Combine(_environment.WebRootPath, "uploads", "company-logos");
                    if (!Directory.Exists(logoDir)) Directory.CreateDirectory(logoDir);

                    var logoFileName = $"{Guid.NewGuid()}{ext}";
                    var savePath = Path.Combine(logoDir, logoFileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }
                    company.LogoPath = $"/uploads/company-logos/{logoFileName}";
                }
            }

            company.CompanyName = company.CompanyName.Trim();
            company.CreatedAt = DateTime.UtcNow;
            company.IsActive = true;

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Company '{company.CompanyName}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            return View(company);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company model, IFormFile? logoFile)
        {
            if (id != model.CompanyId) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var nameExists = await _context.Companies
                .AnyAsync(c => c.CompanyName.ToLower() == model.CompanyName.Trim().ToLower() && c.CompanyId != id);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.CompanyName), "Another company with this name already exists.");
                return View(model);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                var allowedExts = new[] { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
                var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
                if (allowedExts.Contains(ext))
                {
                    var logoDir = Path.Combine(_environment.WebRootPath, "uploads", "company-logos");
                    if (!Directory.Exists(logoDir)) Directory.CreateDirectory(logoDir);

                    var logoFileName = $"{Guid.NewGuid()}{ext}";
                    var savePath = Path.Combine(logoDir, logoFileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }
                    company.LogoPath = $"/uploads/company-logos/{logoFileName}";
                }
            }

            company.CompanyName = model.CompanyName.Trim();
            company.Website = model.Website;
            company.Industry = model.Industry;
            company.Description = model.Description;
            company.Location = model.Location;
            company.HRName = model.HRName;
            company.HREmail = model.HREmail;
            company.HRPhone = model.HRPhone;
            company.IsActive = model.IsActive;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company details updated successfully!";
            return RedirectToAction(nameof(Details), new { id = company.CompanyId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.IsActive = !company.IsActive;
            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Company '{company.CompanyName}' status changed to {(company.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.Companies
                .Include(c => c.PlacementDrives)
                .Include(c => c.Placements)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null) return NotFound();

            if (company.PlacementDrives.Any() || company.Placements.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete company '{company.CompanyName}' because it has existing drives or placements. You can deactivate it instead.";
                return RedirectToAction(nameof(Index));
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
