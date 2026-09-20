using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentPlacementManagement.ViewModels
{
    public class StudentProfileViewModel
    {
        public int StudentId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enrollment Number is required")]
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

        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Address Line")]
        public string? Address { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Please select a department")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        [Range(2000, 2100, ErrorMessage = "Enter a valid passing year")]
        [Display(Name = "Passing Year")]
        public int PassingYear { get; set; } = DateTime.UtcNow.Year;

        [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0.00 and 10.00")]
        [Display(Name = "Cumulative GPA (CGPA)")]
        public decimal CGPA { get; set; }

        [Range(0, 50, ErrorMessage = "Backlogs cannot be negative")]
        [Display(Name = "Active Backlogs")]
        public int Backlogs { get; set; }

        public string? ResumeFileName { get; set; }
        public string? ResumePath { get; set; }

        [Display(Name = "Placement Status")]
        public string PlacementStatus { get; set; } = "NotPlaced";

        public int ProfileCompletionPercentage { get; set; }

        // Select list for departments
        public IEnumerable<SelectListItem>? DepartmentList { get; set; }

        // For Resume upload
        [Display(Name = "Upload New Resume (PDF / DOC / DOCX, Max 5MB)")]
        public IFormFile? ResumeFile { get; set; }
    }
}
