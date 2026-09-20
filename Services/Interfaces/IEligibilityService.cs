namespace StudentPlacementManagement.Services.Interfaces
{
    public class EligibilityResult
    {
        public bool IsEligible { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public interface IEligibilityService
    {
        Task<EligibilityResult> CheckEligibilityAsync(int studentId, int driveId);
        EligibilityResult EvaluateEligibility(StudentPlacementManagement.Models.Student student, StudentPlacementManagement.Models.PlacementDrive drive, bool alreadyApplied);
        Task<List<int>> GetEligibleStudentIdsAsync(int driveId);
    }
}
