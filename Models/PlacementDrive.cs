using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPlacementManagement.Models
{
    public class PlacementDrive
    {
        public int DriveId { get; set; }

        [Required(ErrorMessage = "Company is required")]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Job Role is required")]
        [StringLength(100)]
        [Display(Name = "Job Role")]
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
        public string EmploymentType { get; set; } = "Full Time"; // Full Time, Internship, Trainee

        [Required(ErrorMessage = "Salary Package is required")]
        [Range(0.1, 1000.0, ErrorMessage = "Salary package must be greater than 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Salary Package (LPA in ₹)")]
        public decimal SalaryPackage { get; set; }

        [Range(0.0, 10.0, ErrorMessage = "Min CGPA must be between 0.00 and 10.00")]
        [Column(TypeName = "decimal(4,2)")]
        [Display(Name = "Minimum CGPA")]
        public decimal MinCGPA { get; set; } = 6.0m;

        [Range(0, 50, ErrorMessage = "Max backlogs cannot be negative")]
        [Display(Name = "Maximum Backlogs Allowed")]
        public int MaxBacklogs { get; set; } = 0;

        [Display(Name = "Eligible Passing Year")]
        public int PassingYear { get; set; } = DateTime.UtcNow.Year;

        [DataType(DataType.Date)]
        [Display(Name = "Drive Date")]
        public DateTime DriveDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Application Start Date")]
        public DateTime ApplicationStartDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        [Display(Name = "Application End Date")]
        public DateTime ApplicationEndDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Required Skills")]
        public string? RequiredSkills { get; set; }

        [StringLength(1000)]
        [Display(Name = "Eligibility Details")]
        public string? EligibilityDescription { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Upcoming"; // Upcoming, Open, Closed, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ICollection<DriveDepartment> DriveDepartments { get; set; } = new List<DriveDepartment>();
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<InterviewRound> InterviewRounds { get; set; } = new List<InterviewRound>();
        public virtual ICollection<Placement> Placements { get; set; } = new List<Placement>();
    }
}
