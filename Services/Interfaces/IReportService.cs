using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Services.Interfaces
{
    public class OverallPlacementReportDto
    {
        public int TotalStudents { get; set; }
        public int PlacedStudents { get; set; }
        public int UnplacedStudents { get; set; }
        public decimal PlacementPercentage { get; set; }
        public decimal HighestPackage { get; set; }
        public decimal AveragePackage { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalDrives { get; set; }
        public int TotalOffers { get; set; }
    }

    public class DepartmentPlacementReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int PlacedStudents { get; set; }
        public decimal PlacementPercentage { get; set; }
        public decimal HighestPackage { get; set; }
        public decimal AveragePackage { get; set; }
    }

    public class CompanySelectionReportDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public int DrivesConducted { get; set; }
        public int TotalApplications { get; set; }
        public int SelectedCandidates { get; set; }
        public decimal MaxPackageOffered { get; set; }
    }

    public interface IReportService
    {
        Task<OverallPlacementReportDto> GetOverallPlacementReportAsync(int? passingYear = null);
        Task<List<DepartmentPlacementReportDto>> GetDepartmentWiseReportAsync(int? passingYear = null);
        Task<List<CompanySelectionReportDto>> GetCompanyWiseReportAsync();
        Task<List<Placement>> GetSelectedStudentsReportAsync(int? departmentId = null, int? passingYear = null);
        Task<List<Student>> GetUnplacedStudentsReportAsync(int? departmentId = null, int? passingYear = null);
        Task<byte[]> ExportPlacementsToCsvAsync();
        Task<byte[]> ExportStudentsToCsvAsync(int? departmentId = null, string? placementStatus = null);
    }
}
