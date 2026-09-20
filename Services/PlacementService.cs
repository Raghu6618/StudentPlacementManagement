using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services.Interfaces;

namespace StudentPlacementManagement.Services
{
    public class PlacementService : IPlacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public PlacementService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string? ErrorMessage)> SelectStudentAsync(
            int applicationId, decimal? customPackage = null, DateTime? joiningDate = null, string? adminUserId = null)
        {
            var app = await _context.Applications
                .Include(a => a.Student)
                    .ThenInclude(s => s!.User)
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (app == null || app.Student == null || app.PlacementDrive == null)
            {
                return (false, "Application or related student/drive records not found.");
            }

            // 1. Mark Application as Selected
            app.Status = "Selected";
            app.UpdatedAt = DateTime.UtcNow;

            // 2. Mark Student as Placed
            app.Student.PlacementStatus = "Placed";
            app.Student.UpdatedAt = DateTime.UtcNow;

            // 3. Create or update Placement record
            var existingPlacement = await _context.Placements
                .FirstOrDefaultAsync(p => p.StudentId == app.StudentId && p.DriveId == app.DriveId);

            var finalPackage = customPackage ?? app.PlacementDrive.SalaryPackage;

            if (existingPlacement == null)
            {
                var placement = new Placement
                {
                    StudentId = app.StudentId,
                    DriveId = app.DriveId,
                    CompanyId = app.PlacementDrive.CompanyId,
                    JobRole = app.PlacementDrive.JobRole,
                    SalaryPackage = finalPackage,
                    PlacementDate = DateTime.UtcNow,
                    JoiningDate = joiningDate,
                    Status = "Selected",
                    Remarks = $"Selected via Drive '{app.PlacementDrive.JobRole}'"
                };
                _context.Placements.Add(placement);
            }
            else
            {
                existingPlacement.SalaryPackage = finalPackage;
                existingPlacement.JoiningDate = joiningDate ?? existingPlacement.JoiningDate;
                existingPlacement.Status = "Selected";
            }

            // 4. Record Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminUserId,
                Action = "Student Selected & Placement Created",
                EntityName = "Placement",
                EntityId = app.ApplicationId.ToString(),
                Description = $"Student '{app.Student.FullName}' selected for '{app.PlacementDrive.JobRole}' at '{app.PlacementDrive.Company?.CompanyName}' with package ₹{finalPackage:F2} LPA.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // 5. Send Congratulations notification
            if (app.Student.User != null)
            {
                await _notificationService.SendNotificationAsync(
                    app.Student.User.Id,
                    "🎉 Congratulations on Your Selection!",
                    $"Congratulations {app.Student.FirstName}! You have been selected by {app.PlacementDrive.Company?.CompanyName ?? "the company"} for the role of {app.PlacementDrive.JobRole} at ₹{finalPackage:F2} LPA.",
                    "Placement"
                );
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectStudentAsync(int applicationId, string? remarks = null, string? adminUserId = null)
        {
            var app = await _context.Applications
                .Include(a => a.Student)
                    .ThenInclude(s => s!.User)
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (app == null || app.Student == null)
            {
                return (false, "Application not found.");
            }

            app.Status = "Rejected";
            app.Remarks = remarks ?? "Application not shortlisted in this drive.";
            app.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminUserId,
                Action = "Application Rejected",
                EntityName = "Application",
                EntityId = app.ApplicationId.ToString(),
                Description = $"Application of '{app.Student.FullName}' for '{app.PlacementDrive?.JobRole}' rejected. Remarks: {remarks}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            if (app.Student.User != null)
            {
                await _notificationService.SendNotificationAsync(
                    app.Student.User.Id,
                    "Application Update",
                    $"Your application for {app.PlacementDrive?.JobRole ?? "the role"} at {app.PlacementDrive?.Company?.CompanyName ?? "the company"} has been updated to: Rejected.",
                    "Application"
                );
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> ShortlistStudentAsync(int applicationId, string? remarks = null, string? adminUserId = null)
        {
            var app = await _context.Applications
                .Include(a => a.Student)
                    .ThenInclude(s => s!.User)
                .Include(a => a.PlacementDrive)
                    .ThenInclude(d => d!.Company)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

            if (app == null || app.Student == null)
            {
                return (false, "Application not found.");
            }

            app.Status = "Shortlisted";
            app.Remarks = remarks ?? "Application shortlisted for next rounds.";
            app.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminUserId,
                Action = "Application Shortlisted",
                EntityName = "Application",
                EntityId = app.ApplicationId.ToString(),
                Description = $"Application of '{app.Student.FullName}' for '{app.PlacementDrive?.JobRole}' shortlisted.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            if (app.Student.User != null)
            {
                await _notificationService.SendNotificationAsync(
                    app.Student.User.Id,
                    "🌟 Application Shortlisted!",
                    $"Congratulations! Your application for {app.PlacementDrive?.JobRole} at {app.PlacementDrive?.Company?.CompanyName} has been shortlisted for upcoming interview rounds.",
                    "Application"
                );
            }

            return (true, null);
        }

        public async Task<List<Placement>> GetAllPlacementsAsync()
        {
            return await _context.Placements
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s!.Department)
                .Include(p => p.Company)
                .Include(p => p.PlacementDrive)
                .OrderByDescending(p => p.PlacementDate)
                .ToListAsync();
        }
    }
}
