using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.ViewModels
{
    public class ApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public int DriveId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogoPath { get; set; }
        public string JobRole { get; set; } = string.Empty;
        public decimal SalaryPackage { get; set; }
        public DateTime DriveDate { get; set; }

        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EnrollmentNo { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal StudentCGPA { get; set; }
        public int StudentBacklogs { get; set; }
        public string? ResumePath { get; set; }

        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = "Applied";
        public string? Remarks { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ApplicationFilterViewModel
    {
        public int? DriveId { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<ApplicationViewModel> Applications { get; set; } = new List<ApplicationViewModel>();
    }

    public class ApplicationStatusUpdateViewModel
    {
        public int ApplicationId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public decimal? PackageOffered { get; set; }
        public DateTime? JoiningDate { get; set; }
    }
}
