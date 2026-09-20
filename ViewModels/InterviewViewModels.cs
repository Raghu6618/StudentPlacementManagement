using System.ComponentModel.DataAnnotations;
using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.ViewModels
{
    public class InterviewRoundCreateViewModel
    {
        public int RoundId { get; set; }

        [Required]
        public int DriveId { get; set; }
        public string? DriveRole { get; set; }
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Round Name is required (e.g. Technical Interview)")]
        [StringLength(100)]
        [Display(Name = "Round Name")]
        public string RoundName { get; set; } = string.Empty;

        [Required]
        [Range(1, 20)]
        [Display(Name = "Round Sequence Number")]
        public int RoundNumber { get; set; } = 1;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Scheduled Date & Time")]
        public DateTime RoundDate { get; set; } = DateTime.UtcNow.AddDays(2);

        [StringLength(1000)]
        [Display(Name = "Description / Meeting Link / Venue")]
        public string? Description { get; set; }

        [Display(Name = "Mandatory Round")]
        public bool IsRequired { get; set; } = true;
    }

    public class CandidateRoundEvaluationViewModel
    {
        public int RoundResultId { get; set; }
        public int RoundId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EnrollmentNo { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal CGPA { get; set; }
        public string? ResumePath { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Passed, Failed, Absent
        public decimal? Score { get; set; }
        public string? Remarks { get; set; }
    }

    public class RoundGradingViewModel
    {
        public InterviewRound Round { get; set; } = null!;
        public List<CandidateRoundEvaluationViewModel> Candidates { get; set; } = new List<CandidateRoundEvaluationViewModel>();
    }
}
