using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<PlacementDrive> PlacementDrives { get; set; }
        public DbSet<DriveDepartment> DriveDepartments { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<InterviewRound> InterviewRounds { get; set; }
        public DbSet<RoundResult> RoundResults { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Primary Keys
            builder.Entity<Student>().HasKey(s => s.StudentId);
            builder.Entity<Department>().HasKey(d => d.DepartmentId);
            builder.Entity<Company>().HasKey(c => c.CompanyId);
            builder.Entity<PlacementDrive>().HasKey(pd => pd.DriveId);
            builder.Entity<DriveDepartment>().HasKey(dd => dd.DriveDepartmentId);
            builder.Entity<Application>().HasKey(a => a.ApplicationId);
            builder.Entity<InterviewRound>().HasKey(ir => ir.RoundId);
            builder.Entity<RoundResult>().HasKey(rr => rr.RoundResultId);
            builder.Entity<Placement>().HasKey(p => p.PlacementId);
            builder.Entity<Notification>().HasKey(n => n.NotificationId);
            builder.Entity<AuditLog>().HasKey(al => al.AuditLogId);

            // Unique constraints & performance indexes
            builder.Entity<Student>()
                .HasIndex(s => s.EnrollmentNo)
                .IsUnique();

            builder.Entity<Student>()
                .HasIndex(s => s.UserId);

            builder.Entity<Application>()
                .HasIndex(a => new { a.DriveId, a.StudentId })
                .IsUnique();

            builder.Entity<Application>()
                .HasIndex(a => a.StudentId);

            builder.Entity<DriveDepartment>()
                .HasIndex(dd => new { dd.DriveId, dd.DepartmentId })
                .IsUnique();

            builder.Entity<InterviewRound>()
                .HasIndex(ir => new { ir.DriveId, ir.RoundNumber })
                .IsUnique();

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            builder.Entity<PlacementDrive>()
                .HasIndex(pd => pd.Status);

            // Decimal Precision configurations
            builder.Entity<Student>()
                .Property(s => s.CGPA)
                .HasPrecision(4, 2);

            builder.Entity<PlacementDrive>()
                .Property(pd => pd.MinCGPA)
                .HasPrecision(4, 2);

            builder.Entity<PlacementDrive>()
                .Property(pd => pd.SalaryPackage)
                .HasPrecision(10, 2);

            builder.Entity<Placement>()
                .Property(p => p.SalaryPackage)
                .HasPrecision(10, 2);

            builder.Entity<RoundResult>()
                .Property(r => r.Score)
                .HasPrecision(5, 2);

            // Relationships & Delete Behaviors (preventing cycle or multiple cascade paths in SQL Server)
            builder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DriveDepartment>()
                .HasOne(dd => dd.PlacementDrive)
                .WithMany(pd => pd.DriveDepartments)
                .HasForeignKey(dd => dd.DriveId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DriveDepartment>()
                .HasOne(dd => dd.Department)
                .WithMany(d => d.DriveDepartments)
                .HasForeignKey(dd => dd.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Application>()
                .HasOne(a => a.PlacementDrive)
                .WithMany(pd => pd.Applications)
                .HasForeignKey(a => a.DriveId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Application>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InterviewRound>()
                .HasOne(ir => ir.PlacementDrive)
                .WithMany(pd => pd.InterviewRounds)
                .HasForeignKey(ir => ir.DriveId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoundResult>()
                .HasOne(rr => rr.InterviewRound)
                .WithMany(ir => ir.RoundResults)
                .HasForeignKey(rr => rr.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoundResult>()
                .HasOne(rr => rr.Student)
                .WithMany(s => s.RoundResults)
                .HasForeignKey(rr => rr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Placement>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Placements)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Placement>()
                .HasOne(p => p.PlacementDrive)
                .WithMany(pd => pd.Placements)
                .HasForeignKey(p => p.DriveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Placement>()
                .HasOne(p => p.Company)
                .WithMany(c => c.Placements)
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
