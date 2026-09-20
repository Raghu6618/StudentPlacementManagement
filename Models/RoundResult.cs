using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPlacementManagement.Models
{
    public class RoundResult
    {
        public int RoundResultId { get; set; }

        public int RoundId { get; set; }
        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Passed, Failed, Absent

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual InterviewRound? InterviewRound { get; set; }
        public virtual Student? Student { get; set; }
    }
}
