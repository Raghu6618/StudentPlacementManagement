using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.Services
{
    public class EligibilityService : IEligibilityService
    {
        private readonly ApplicationDbContext _context;

        public EligibilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EligibilityResult> CheckEligibilityAsync(int studentId, int driveId)
        {
            var student = await _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
            {
                return new EligibilityResult { IsEligible = false, Reason = "Student record not found." };
            }

            var drive = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.DriveDepartments)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);

            if (drive == null)
            {
                return new EligibilityResult { IsEligible = false, Reason = "Placement drive not found." };
            }

            var alreadyApplied = await _context.Applications
                .AsNoTracking()
                .AnyAsync(a => a.DriveId == driveId && a.StudentId == studentId);

            return EvaluateEligibility(student, drive, alreadyApplied);
        }

        public EligibilityResult EvaluateEligibility(Student student, PlacementDrive drive, bool alreadyApplied)
        {
            if (student == null)
            {
                return new EligibilityResult { IsEligible = false, Reason = "Student record not found." };
            }

            if (drive == null)
            {
                return new EligibilityResult { IsEligible = false, Reason = "Placement drive not found." };
            }

            // 1. Check if drive is Open
            if (!string.Equals(drive.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Placement drive is currently {drive.Status}."
                };
            }

            // 2. Check application window
            var now = DateTime.UtcNow;
            if (drive.ApplicationStartDate > now)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Applications will open on {drive.ApplicationStartDate:dd MMM yyyy, hh:mm tt} UTC."
                };
            }

            if (drive.ApplicationEndDate < now)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Application deadline passed on {drive.ApplicationEndDate:dd MMM yyyy, hh:mm tt} UTC."
                };
            }

            // 3. Check if already applied
            if (alreadyApplied)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = "You have already applied for this placement drive."
                };
            }

            // 4. Check department eligibility
            var isDeptEligible = drive.DriveDepartments != null && drive.DriveDepartments.Any(dd => dd.DepartmentId == student.DepartmentId);
            if (!isDeptEligible)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Department '{student.Department?.DepartmentName ?? "Your Department"}' is not eligible for this drive."
                };
            }

            // 5. Check Passing Year
            if (drive.PassingYear > 0 && student.PassingYear != drive.PassingYear)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Eligible passing year is {drive.PassingYear}. Your passing year is {student.PassingYear}."
                };
            }

            // 6. Check CGPA
            if (student.CGPA < drive.MinCGPA)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"CGPA ({student.CGPA:F2}) is below the required minimum of {drive.MinCGPA:F2}."
                };
            }

            // 7. Check Backlogs
            if (student.Backlogs > drive.MaxBacklogs)
            {
                return new EligibilityResult
                {
                    IsEligible = false,
                    Reason = $"Your active backlog count ({student.Backlogs}) exceeds the allowed limit ({drive.MaxBacklogs})."
                };
            }

            return new EligibilityResult
            {
                IsEligible = true,
                Reason = "Student satisfies all eligibility criteria."
            };
        }

        public async Task<List<int>> GetEligibleStudentIdsAsync(int driveId)
        {
            var drive = await _context.PlacementDrives
                .AsNoTracking()
                .Include(d => d.DriveDepartments)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);

            if (drive == null) return new List<int>();

            var eligibleDeptIds = drive.DriveDepartments.Select(dd => dd.DepartmentId).ToList();

            var query = _context.Students
                .AsNoTracking()
                .Where(s => eligibleDeptIds.Contains(s.DepartmentId))
                .Where(s => s.CGPA >= drive.MinCGPA)
                .Where(s => s.Backlogs <= drive.MaxBacklogs);

            if (drive.PassingYear > 0)
            {
                query = query.Where(s => s.PassingYear == drive.PassingYear);
            }

            return await query.Select(s => s.StudentId).ToListAsync();
        }
    }
}
