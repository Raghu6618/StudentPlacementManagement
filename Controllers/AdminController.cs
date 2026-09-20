using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.ViewModels;

namespace StudentPlacementManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalStudents = await _context.Students.CountAsync();
            var totalCompanies = await _context.Companies.CountAsync(c => c.IsActive);
            var activeDrives = await _context.PlacementDrives.CountAsync(d => d.Status == "Open");
            var totalApplications = await _context.Applications.CountAsync();
            var shortlistedStudents = await _context.Applications.CountAsync(a => a.Status == "Shortlisted");
            var placedStudents = await _context.Students.CountAsync(s => s.PlacementStatus == "Placed");
            var unplacedStudents = totalStudents - placedStudents;

            var placementPct = totalStudents > 0
                ? Math.Round(((decimal)placedStudents / totalStudents) * 100, 1)
                : 0m;

            var placements = await _context.Placements.AsNoTracking().ToListAsync();
            var highestPkg = placements.Any() ? placements.Max(p => p.SalaryPackage) : 0m;
            var avgPkg = placements.Any() ? Math.Round(placements.Average(p => p.SalaryPackage), 2) : 0m;

            // Recent activity lists
            var recentDrives = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.Company)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentApplications = await _context.Applications
                .AsNoTracking()
                .Include(a => a.Student)
                    .ThenInclude(s => s!.Department)
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .OrderByDescending(a => a.AppliedDate)
                .Take(6)
                .ToListAsync();

            var recentlyPlaced = await _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .OrderByDescending(p => p.PlacementDate)
                .Take(5)
                .ToListAsync();

            var upcomingRounds = await _context.InterviewRounds
                .AsNoTracking()
                .Include(r => r.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .Where(r => r.RoundDate >= DateTime.UtcNow.AddDays(-1))
                .OrderBy(r => r.RoundDate)
                .Take(5)
                .ToListAsync();

            var recentAudits = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Chart 1: Department-wise placement stats
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Students)
                .ToListAsync();

            var deptNames = new List<string>();
            var deptPlaced = new List<int>();
            var deptTotal = new List<int>();

            foreach (var dept in departments)
            {
                deptNames.Add(dept.DepartmentCode);
                deptTotal.Add(dept.Students.Count);
                deptPlaced.Add(dept.Students.Count(s => s.PlacementStatus == "Placed"));
            }

            // Chart 2: Top companies by selections
            var companyStats = await _context.Companies
                .AsNoTracking()
                .Include(c => c.Placements)
                .Where(c => c.Placements.Any())
                .OrderByDescending(c => c.Placements.Count)
                .Take(6)
                .ToListAsync();

            var compNames = companyStats.Select(c => c.CompanyName).ToList();
            var compOffers = companyStats.Select(c => c.Placements.Count).ToList();

            // Chart 3: Monthly trend (past 6 months)
            var trendMonths = new List<string>();
            var trendCounts = new List<int>();
            var currentMonth = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = currentMonth.AddMonths(-i);
                trendMonths.Add(targetMonth.ToString("MMM yyyy"));
                var count = placements.Count(p => p.PlacementDate.Year == targetMonth.Year && p.PlacementDate.Month == targetMonth.Month);
                trendCounts.Add(count);
            }

            var vm = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalCompanies = totalCompanies,
                ActiveDrives = activeDrives,
                TotalApplications = totalApplications,
                ShortlistedStudents = shortlistedStudents,
                PlacedStudents = placedStudents,
                UnplacedStudents = unplacedStudents,
                PlacementPercentage = placementPct,
                HighestPackage = highestPkg,
                AveragePackage = avgPkg,

                RecentDrives = recentDrives,
                RecentApplications = recentApplications,
                RecentlyPlacedStudents = recentlyPlaced,
                UpcomingRounds = upcomingRounds,
                RecentAuditLogs = recentAudits,

                DeptNames = deptNames,
                DeptPlacedCounts = deptPlaced,
                DeptTotalCounts = deptTotal,

                CompanyNames = compNames,
                CompanyOfferCounts = compOffers,

                TrendMonths = trendMonths,
                TrendPlacedCounts = trendCounts
            };

            return View(vm);
        }
    }
}
