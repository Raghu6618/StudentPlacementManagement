using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Student?> GetStudentByUserIdAsync(string userId);
        Task<Student?> GetStudentByIdAsync(int studentId);
        int CalculateProfileCompletion(Student student);
        Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(Student student);
        Task<(bool Success, string? FileName, string? FilePath, string? ErrorMessage)> UploadResumeAsync(int studentId, IFormFile file);
    }
}
