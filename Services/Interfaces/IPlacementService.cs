using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Services.Interfaces
{
    public interface IPlacementService
    {
        Task<(bool Success, string? ErrorMessage)> SelectStudentAsync(int applicationId, decimal? customPackage = null, DateTime? joiningDate = null, string? adminUserId = null);
        Task<(bool Success, string? ErrorMessage)> RejectStudentAsync(int applicationId, string? remarks = null, string? adminUserId = null);
        Task<(bool Success, string? ErrorMessage)> ShortlistStudentAsync(int applicationId, string? remarks = null, string? adminUserId = null);
        Task<List<Placement>> GetAllPlacementsAsync();
    }
}
