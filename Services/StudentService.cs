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

            // Validate file content signature (magic bytes) to prevent disguised files
            if (!IsValidFileSignature(file, ext))
            {
                return (false, null, null, "File content does not match the expected document format.");
            }

            // Target uploads directory
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "resumes");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Remove previous resume if exists safely
            if (!string.IsNullOrEmpty(student.ResumePath))
            {
                var cleanExisting = student.ResumePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var existingFilePath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, cleanExisting));
                var fullUploadsBase = Path.GetFullPath(uploadsDir);
                if (existingFilePath.StartsWith(fullUploadsBase, StringComparison.OrdinalIgnoreCase) && File.Exists(existingFilePath))
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
            var sanitizedOriginalFileName = Path.GetFileName(file.FileName);
            student.ResumeFileName = sanitizedOriginalFileName;
            student.ResumePath = relativePath;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, sanitizedOriginalFileName, relativePath, null);
        }

        private static bool IsValidFileSignature(IFormFile file, string ext)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new BinaryReader(stream);
                var headerBytes = reader.ReadBytes(8);
                if (headerBytes.Length < 4) return false;

                if (ext == ".pdf")
                {
                    // %PDF
                    return headerBytes[0] == 0x25 &&
                           headerBytes[1] == 0x50 &&
                           headerBytes[2] == 0x44 &&
                           headerBytes[3] == 0x46;
                }

                if (ext == ".doc")
                {
                    // OLECF header: D0 CF 11 E0 A1 B1 1A E1
                    return headerBytes.Length >= 8 &&
                           headerBytes[0] == 0xD0 && headerBytes[1] == 0xCF &&
                           headerBytes[2] == 0x11 && headerBytes[3] == 0xE0 &&
                           headerBytes[4] == 0xA1 && headerBytes[5] == 0xB1 &&
                           headerBytes[6] == 0x1A && headerBytes[7] == 0xE1;
                }

                if (ext == ".docx")
                {
                    // ZIP header: PK\x03\x04
                    return headerBytes[0] == 0x50 &&
                           headerBytes[1] == 0x4B &&
                           (headerBytes[2] == 0x03 || headerBytes[2] == 0x05 || headerBytes[2] == 0x07) &&
                           (headerBytes[3] == 0x04 || headerBytes[3] == 0x06 || headerBytes[3] == 0x08);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
