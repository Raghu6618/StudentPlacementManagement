using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }

        public int DriveId { get; set; }
        public int StudentId { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Applied"; // Applied, UnderReview, Shortlisted, Rejected, Selected, Withdrawn

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual PlacementDrive? PlacementDrive { get; set; }
        public virtual Student? Student { get; set; }
    }
}
