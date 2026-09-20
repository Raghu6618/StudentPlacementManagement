using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Services.Interfaces;
using StudentPlacementManagement.ViewModels;

namespace StudentPlacementManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;

        public ReportController(IReportService reportService, ApplicationDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        public async Task<IActionResult> Index(int? departmentId, int? passingYear, int? companyId, string? placementStatus)
        {
            var overallReport = await _reportService.GetOverallPlacementReportAsync(passingYear);
            var deptReports = await _reportService.GetDepartmentWiseReportAsync(passingYear);
            var compReports = await _reportService.GetCompanyWiseReportAsync();
            var selectedStudents = await _reportService.GetSelectedStudentsReportAsync(departmentId, passingYear);
            var unplacedStudents = await _reportService.GetUnplacedStudentsReportAsync(departmentId, passingYear);

            var depts = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                .ToListAsync();

            var companies = await _context.Companies
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName })
                .ToListAsync();

            var passingYears = await _context.Students
                .Select(s => s.PassingYear)
                .Distinct()
                .OrderByDescending(y => y)
                .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() })
                .ToListAsync();

            var vm = new ReportViewModel
            {
                SelectedDepartmentId = departmentId,
                SelectedPassingYear = passingYear,
                SelectedCompanyId = companyId,
                SelectedPlacementStatus = placementStatus,
                DepartmentOptions = depts,
                CompanyOptions = companies,
                PassingYearOptions = passingYears,
                OverallReport = overallReport,
                DepartmentReports = deptReports,
                CompanyReports = compReports,
                SelectedStudents = selectedStudents,
                UnplacedStudents = unplacedStudents
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPlacementsCsv()
        {
            var csvBytes = await _reportService.ExportPlacementsToCsvAsync();
            var fileName = $"Placement_Report_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudentsCsv(int? departmentId, string? placementStatus)
        {
            var csvBytes = await _reportService.ExportStudentsToCsvAsync(departmentId, placementStatus);
            var fileName = $"Students_Report_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
            return File(csvBytes, "text/csv", fileName);
        }
    }
}
