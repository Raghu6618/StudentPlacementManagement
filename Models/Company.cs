using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class Company
    {
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        [Display(Name = "HR Name")]
        public string? HRName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid HR Email")]
        [StringLength(100)]
        [Display(Name = "HR Email")]
        public string? HREmail { get; set; }

        [Phone(ErrorMessage = "Please enter a valid HR Phone Number")]
        [StringLength(20)]
        [Display(Name = "HR Phone")]
        public string? HRPhone { get; set; }

        [StringLength(500)]
        [Display(Name = "Company Logo")]
        public string? LogoPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<PlacementDrive> PlacementDrives { get; set; } = new List<PlacementDrive>();
        public virtual ICollection<Placement> Placements { get; set; } = new List<Placement>();
    }
}
