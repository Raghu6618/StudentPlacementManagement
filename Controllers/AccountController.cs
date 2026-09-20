using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.ViewModels;

namespace StudentPlacementManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", user.Email);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return await RedirectBasedOnRoleAsync(user);
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials. Please try again.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard();
            }

            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = $"{d.DepartmentName} ({d.DepartmentCode})" })
                .ToListAsync();

            ViewBag.Departments = departments;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Re-populate departments for view rendering if needed
            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = $"{d.DepartmentName} ({d.DepartmentCode})" })
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if enrollment number is already registered
            var enrollmentExists = await _context.Students.AnyAsync(s => s.EnrollmentNo == model.EnrollmentNo);
            if (enrollmentExists)
            {
                ModelState.AddModelError(nameof(model.EnrollmentNo), "This enrollment number is already registered.");
                return View(model);
            }

            // Check if email already exists
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                UserRole = "Student",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");

                var student = new Student
                {
                    UserId = user.Id,
                    EnrollmentNo = model.EnrollmentNo.Trim().ToUpper(),
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    Phone = model.Phone,
                    DepartmentId = model.DepartmentId,
                    PassingYear = model.PassingYear,
                    PlacementStatus = "NotPlaced",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Sign the student in immediately
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Registration successful! Welcome to the Placement Management Portal. Please complete your academic and resume details.";
                return RedirectToAction("Dashboard", "Student");
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IActionResult> RedirectBasedOnRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (roles.Contains("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            if (roles.Contains("Recruiter"))
            {
                return RedirectToAction("Index", "PlacementDrive");
            }

            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
