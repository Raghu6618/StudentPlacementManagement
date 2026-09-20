using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPlacementManagement.Models
{
    public class Placement
    {
        public int PlacementId { get; set; }

        public int StudentId { get; set; }
        public int DriveId { get; set; }
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Job Role")]
        public string JobRole { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Package (LPA in ₹)")]
        public decimal SalaryPackage { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Placement Date")]
        public DateTime PlacementDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime? JoiningDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Selected"; // Selected, OfferReceived, Joined, NotJoined

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Navigation Properties
        public virtual Student? Student { get; set; }
        public virtual PlacementDrive? PlacementDrive { get; set; }
        public virtual Company? Company { get; set; }
    }
}
