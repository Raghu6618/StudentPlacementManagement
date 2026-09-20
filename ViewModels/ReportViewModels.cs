using Microsoft.AspNetCore.Mvc.Rendering;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.ViewModels
{
    public class ReportViewModel
    {
        public int? SelectedDepartmentId { get; set; }
        public int? SelectedPassingYear { get; set; }
        public int? SelectedCompanyId { get; set; }
        public string? SelectedPlacementStatus { get; set; }

        public IEnumerable<SelectListItem>? DepartmentOptions { get; set; }
        public IEnumerable<SelectListItem>? CompanyOptions { get; set; }
        public IEnumerable<SelectListItem>? PassingYearOptions { get; set; }

        public OverallPlacementReportDto? OverallReport { get; set; }
        public List<DepartmentPlacementReportDto> DepartmentReports { get; set; } = new List<DepartmentPlacementReportDto>();
        public List<CompanySelectionReportDto> CompanyReports { get; set; } = new List<CompanySelectionReportDto>();
        public List<Placement> SelectedStudents { get; set; } = new List<Placement>();
        public List<Student> UnplacedStudents { get; set; } = new List<Student>();
    }
}
