using System.Text;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OverallPlacementReportDto> GetOverallPlacementReportAsync(int? passingYear = null)
        {
            var studentQuery = _context.Students.AsNoTracking().AsQueryable();
            if (passingYear.HasValue && passingYear.Value > 0)
            {
                studentQuery = studentQuery.Where(s => s.PassingYear == passingYear.Value);
            }

            var totalStudents = await studentQuery.CountAsync();
            var placedStudents = await studentQuery.CountAsync(s => s.PlacementStatus == "Placed");
            var unplacedStudents = totalStudents - placedStudents;

            decimal placementPct = totalStudents > 0
                ? Math.Round(((decimal)placedStudents / totalStudents) * 100, 2)
                : 0m;

            var placementQuery = _context.Placements.AsNoTracking().AsQueryable();
            if (passingYear.HasValue && passingYear.Value > 0)
            {
                placementQuery = placementQuery.Where(p => p.Student != null && p.Student.PassingYear == passingYear.Value);
            }

            var totalOffers = await placementQuery.CountAsync();
            var highestPackage = totalOffers > 0 ? await placementQuery.MaxAsync(p => p.SalaryPackage) : 0m;
            var averagePackage = totalOffers > 0 ? Math.Round(await placementQuery.AverageAsync(p => p.SalaryPackage), 2) : 0m;

            var totalCompanies = await _context.Companies.AsNoTracking().CountAsync(c => c.IsActive);
            var totalDrives = await _context.PlacementDrives.AsNoTracking().CountAsync();

            return new OverallPlacementReportDto
            {
                TotalStudents = totalStudents,
                PlacedStudents = placedStudents,
                UnplacedStudents = unplacedStudents,
                PlacementPercentage = placementPct,
                HighestPackage = highestPackage,
                AveragePackage = averagePackage,
                TotalCompanies = totalCompanies,
                TotalDrives = totalDrives,
                TotalOffers = totalOffers
            };
        }

        public async Task<List<DepartmentPlacementReportDto>> GetDepartmentWiseReportAsync(int? passingYear = null)
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Students)
                .ToListAsync();

            var result = new List<DepartmentPlacementReportDto>();

            foreach (var dept in departments)
            {
                var students = dept.Students.AsQueryable();
                if (passingYear.HasValue && passingYear.Value > 0)
                {
                    students = students.Where(s => s.PassingYear == passingYear.Value);
                }

                var total = students.Count();
                var placed = students.Count(s => s.PlacementStatus == "Placed");
                var pct = total > 0 ? Math.Round(((decimal)placed / total) * 100, 2) : 0m;

                var studentIds = students.Where(s => s.PlacementStatus == "Placed").Select(s => s.StudentId).ToList();
                var placements = await _context.Placements.AsNoTracking().Where(p => studentIds.Contains(p.StudentId)).ToListAsync();

                var maxPkg = placements.Any() ? placements.Max(p => p.SalaryPackage) : 0m;
                var avgPkg = placements.Any() ? Math.Round(placements.Average(p => p.SalaryPackage), 2) : 0m;

                result.Add(new DepartmentPlacementReportDto
                {
                    DepartmentId = dept.DepartmentId,
                    DepartmentName = dept.DepartmentName,
                    TotalStudents = total,
                    PlacedStudents = placed,
                    PlacementPercentage = pct,
                    HighestPackage = maxPkg,
                    AveragePackage = avgPkg
                });
            }

            return result;
        }

        public async Task<List<CompanySelectionReportDto>> GetCompanyWiseReportAsync()
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .Include(c => c.PlacementDrives)
                    .ThenInclude(d => d.Applications)
                .Include(c => c.Placements)
                .ToListAsync();

            var result = new List<CompanySelectionReportDto>();

            foreach (var comp in companies)
            {
                var totalApps = comp.PlacementDrives.SelectMany(d => d.Applications).Count();
                var selected = comp.Placements.Count;
                var maxPkg = comp.Placements.Any() ? comp.Placements.Max(p => p.SalaryPackage) : (comp.PlacementDrives.Any() ? comp.PlacementDrives.Max(d => d.SalaryPackage) : 0m);

                result.Add(new CompanySelectionReportDto
                {
                    CompanyId = comp.CompanyId,
                    CompanyName = comp.CompanyName,
                    Industry = comp.Industry ?? "General",
                    DrivesConducted = comp.PlacementDrives.Count,
                    TotalApplications = totalApps,
                    SelectedCandidates = selected,
                    MaxPackageOffered = maxPkg
                });
            }

            return result.OrderByDescending(r => r.SelectedCandidates).ToList();
        }

        public async Task<List<Placement>> GetSelectedStudentsReportAsync(int? departmentId = null, int? passingYear = null)
        {
            var query = _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(p => p.Student != null && p.Student.DepartmentId == departmentId.Value);
            }

            if (passingYear.HasValue && passingYear.Value > 0)
            {
                query = query.Where(p => p.Student != null && p.Student.PassingYear == passingYear.Value);
            }

            return await query.OrderByDescending(p => p.PlacementDate).ToListAsync();
        }

        public async Task<List<Student>> GetUnplacedStudentsReportAsync(int? departmentId = null, int? passingYear = null)
        {
            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .Where(s => s.PlacementStatus != "Placed")
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            if (passingYear.HasValue && passingYear.Value > 0)
            {
                query = query.Where(s => s.PassingYear == passingYear.Value);
            }

            return await query.OrderByDescending(s => s.CGPA).ToListAsync();
        }

        public async Task<byte[]> ExportPlacementsToCsvAsync()
        {
            var placements = await _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .OrderByDescending(p => p.PlacementDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PlacementId,EnrollmentNo,StudentName,Department,Company,JobRole,SalaryPackageLPA,PlacementDate,JoiningDate,Status");

            foreach (var p in placements)
            {
                var enroll = EscapeCsv(p.Student?.EnrollmentNo);
                var name = EscapeCsv(p.Student?.FullName);
                var dept = EscapeCsv(p.Student?.Department?.DepartmentName);
                var comp = EscapeCsv(p.Company?.CompanyName);
                var role = EscapeCsv(p.JobRole);
                var pkg = p.SalaryPackage.ToString("F2");
                var pDate = p.PlacementDate.ToString("yyyy-MM-dd");
                var jDate = p.JoiningDate.HasValue ? p.JoiningDate.Value.ToString("yyyy-MM-dd") : "Pending";
                var status = EscapeCsv(p.Status);

                sb.AppendLine($"{p.PlacementId},{enroll},{name},{dept},{comp},{role},{pkg},{pDate},{jDate},{status}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportStudentsToCsvAsync(int? departmentId = null, string? placementStatus = null)
        {
            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Department)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(placementStatus))
            {
                query = query.Where(s => s.PlacementStatus == placementStatus);
            }

            var students = await query.OrderBy(s => s.EnrollmentNo).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("EnrollmentNo,FirstName,LastName,Email,Phone,Department,PassingYear,CGPA,Backlogs,PlacementStatus");

            foreach (var s in students)
            {
                var enroll = EscapeCsv(s.EnrollmentNo);
                var fn = EscapeCsv(s.FirstName);
                var ln = EscapeCsv(s.LastName);
                var email = EscapeCsv(s.Email);
                var phone = EscapeCsv(s.Phone);
                var dept = EscapeCsv(s.Department?.DepartmentName);
                var year = s.PassingYear;
                var cgpa = s.CGPA.ToString("F2");
                var backlogs = s.Backlogs;
                var status = EscapeCsv(s.PlacementStatus);

                sb.AppendLine($"{enroll},{fn},{ln},{email},{phone},{dept},{year},{cgpa},{backlogs},{status}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return $"\"{field}\"";
        }
    }
}
