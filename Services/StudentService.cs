using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public StudentService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public int CalculateProfileCompletion(Student student)
        {
            if (student == null) return 0;

            int totalPoints = 10;
            int earnedPoints = 0;

            if (!string.IsNullOrWhiteSpace(student.FirstName)) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.LastName)) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.Email)) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.Phone)) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.Gender)) earnedPoints++;
            if (student.DateOfBirth.HasValue) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.City)) earnedPoints++;
            if (student.DepartmentId > 0) earnedPoints++;
            if (student.CGPA > 0) earnedPoints++;
            if (!string.IsNullOrWhiteSpace(student.ResumePath)) earnedPoints++;

            return (int)((earnedPoints / (double)totalPoints) * 100);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(Student updatedStudent)
        {
            var student = await _context.Students.FindAsync(updatedStudent.StudentId);
            if (student == null) return (false, "Student record not found.");

            // Check if enrollment number is taken by another student
            var duplicateEnrollment = await _context.Students
                .AnyAsync(s => s.EnrollmentNo == updatedStudent.EnrollmentNo && s.StudentId != updatedStudent.StudentId);

            if (duplicateEnrollment)
            {
                return (false, $"Enrollment Number '{updatedStudent.EnrollmentNo}' is already registered to another student.");
            }

            student.FirstName = updatedStudent.FirstName;
            student.LastName = updatedStudent.LastName;
            student.Gender = updatedStudent.Gender;
            student.DateOfBirth = updatedStudent.DateOfBirth;
            student.Phone = updatedStudent.Phone;
            student.Address = updatedStudent.Address;
            student.City = updatedStudent.City;
            student.State = updatedStudent.State;
            student.DepartmentId = updatedStudent.DepartmentId;
            student.PassingYear = updatedStudent.PassingYear;
            student.CGPA = updatedStudent.CGPA;
            student.Backlogs = updatedStudent.Backlogs;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? FileName, string? FilePath, string? ErrorMessage)> UploadResumeAsync(int studentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, null, "Please select a file to upload.");
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return (false, null, null, "Student record not found.");

            var maxMb = _configuration.GetValue<int>("FileUpload:MaxResumeSizeMB", 5);
            var maxBytes = maxMb * 1024 * 1024;
            if (file.Length > maxBytes)
            {
                return (false, null, null, $"File size exceeds maximum allowed limit of {maxMb} MB.");
            }

            var allowedExts = _configuration.GetSection("FileUpload:AllowedResumeExtensions").Get<string[]>() 
                ?? new[] { ".pdf", ".doc", ".docx" };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
            {
                return (false, null, null, $"Invalid file format. Allowed extensions: {string.Join(", ", allowedExts)}");
            }

            // Target uploads directory
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "resumes");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Remove previous resume if exists
            if (!string.IsNullOrEmpty(student.ResumePath))
            {
                var existingFilePath = Path.Combine(_environment.WebRootPath, student.ResumePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(existingFilePath))
                {
                    try { File.Delete(existingFilePath); } catch { /* ignore if locked */ }
                }
            }

            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var fullSavePath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(fullSavePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/resumes/{safeFileName}";
            student.ResumeFileName = file.FileName;
            student.ResumePath = relativePath;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, file.FileName, relativePath, null);
        }
    }
}
