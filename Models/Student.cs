using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPlacementManagement.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enrollment Number is required")]
        [StringLength(50)]
        [Display(Name = "Enrollment Number")]
        public string EnrollmentNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [StringLength(10)]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Range(2000, 2100, ErrorMessage = "Please enter a valid passing year")]
        [Display(Name = "Passing Year")]
        public int PassingYear { get; set; } = DateTime.UtcNow.Year;

        [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0.00 and 10.00")]
        [Column(TypeName = "decimal(4,2)")]
        public decimal CGPA { get; set; } = 0.0m;

        [Range(0, 50, ErrorMessage = "Backlogs cannot be negative")]
        public int Backlogs { get; set; } = 0;

        [StringLength(255)]
        [Display(Name = "Resume File Name")]
        public string? ResumeFileName { get; set; }

        [StringLength(500)]
        [Display(Name = "Resume Path")]
        public string? ResumePath { get; set; }

        [StringLength(500)]
        [Display(Name = "Profile Image")]
        public string? ProfileImagePath { get; set; }

        [StringLength(20)]
        [Display(Name = "Placement Status")]
        public string PlacementStatus { get; set; } = "NotPlaced"; // "NotPlaced" or "Placed"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<Placement> Placements { get; set; } = new List<Placement>();
        public virtual ICollection<RoundResult> RoundResults { get; set; } = new List<RoundResult>();
    }
}
