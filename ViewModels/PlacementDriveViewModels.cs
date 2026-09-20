using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.ViewModels
{
    public class PlacementDriveCreateViewModel
    {
        public int DriveId { get; set; }

        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Job Role is required")]
        [StringLength(100)]
        [Display(Name = "Job Role / Designation")]
        public string JobRole { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job Description is required")]
        [StringLength(3000)]
        [Display(Name = "Job Description")]
        public string JobDescription { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Job Location")]
        public string? JobLocation { get; set; }

        [StringLength(50)]
        [Display(Name = "Employment Type")]
        public string EmploymentType { get; set; } = "Full Time";

        [Required(ErrorMessage = "Salary package is required")]
        [Range(0.1, 1000.0, ErrorMessage = "Package must be greater than 0")]
        [Display(Name = "Package (LPA in ₹)")]
        public decimal SalaryPackage { get; set; }

        [Range(0.0, 10.0, ErrorMessage = "Min CGPA must be between 0.00 and 10.00")]
        [Display(Name = "Minimum CGPA Required")]
        public decimal MinCGPA { get; set; } = 6.0m;

        [Range(0, 50, ErrorMessage = "Max backlogs cannot be negative")]
        [Display(Name = "Max Backlogs Allowed")]
        public int MaxBacklogs { get; set; } = 0;

        [Display(Name = "Target Passing Year")]
        public int PassingYear { get; set; } = DateTime.UtcNow.Year;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Drive Date")]
        public DateTime DriveDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Application Start Date")]
        public DateTime ApplicationStartDate { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Application End Date")]
        public DateTime ApplicationEndDate { get; set; } = DateTime.UtcNow.AddDays(5);

        [StringLength(500)]
        [Display(Name = "Required Skills")]
        public string? RequiredSkills { get; set; }

        [StringLength(1000)]
        [Display(Name = "Eligibility Criteria Description")]
        public string? EligibilityDescription { get; set; }

        [Display(Name = "Drive Status")]
        public string Status { get; set; } = "Open";

        [Required(ErrorMessage = "Select at least one eligible department")]
        [Display(Name = "Eligible Departments")]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? CompanyList { get; set; }
        public IEnumerable<SelectListItem>? DepartmentList { get; set; }
    }

    public class PlacementDriveDetailsViewModel
    {
        public PlacementDrive Drive { get; set; } = null!;
        public EligibilityResult? UserEligibility { get; set; }
        public bool HasAlreadyApplied { get; set; }
        public Application? ExistingApplication { get; set; }
        public int TotalApplicantsCount { get; set; }
        public int EligibleStudentsCount { get; set; }
        public List<InterviewRound> InterviewRounds { get; set; } = new List<InterviewRound>();
    }
}
