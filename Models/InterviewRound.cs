using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class InterviewRound
    {
        public int RoundId { get; set; }

        public int DriveId { get; set; }

        [Required(ErrorMessage = "Round Name is required")]
        [StringLength(100)]
        [Display(Name = "Round Name")]
        public string RoundName { get; set; } = string.Empty; // e.g. Aptitude Test, Technical Interview, HR Round

        [Range(1, 20, ErrorMessage = "Round Number must be 1 or greater")]
        [Display(Name = "Round Number")]
        public int RoundNumber { get; set; } = 1;

        [DataType(DataType.DateTime)]
        [Display(Name = "Round Date & Time")]
        public DateTime RoundDate { get; set; } = DateTime.UtcNow.AddDays(2);

        [StringLength(1000)]
        [Display(Name = "Description / Venue / Link")]
        public string? Description { get; set; }

        [Display(Name = "Mandatory Round")]
        public bool IsRequired { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual PlacementDrive? PlacementDrive { get; set; }
        public virtual ICollection<RoundResult> RoundResults { get; set; } = new List<RoundResult>();
    }
}
