using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCompanies { get; set; }
        public int ActiveDrives { get; set; }
        public int TotalApplications { get; set; }
        public int ShortlistedStudents { get; set; }
        public int PlacedStudents { get; set; }
        public int UnplacedStudents { get; set; }
        public decimal PlacementPercentage { get; set; }
        public decimal HighestPackage { get; set; }
        public decimal AveragePackage { get; set; }

        public List<PlacementDrive> RecentDrives { get; set; } = new List<PlacementDrive>();
        public List<Application> RecentApplications { get; set; } = new List<Application>();
        public List<Placement> RecentlyPlacedStudents { get; set; } = new List<Placement>();
        public List<InterviewRound> UpcomingRounds { get; set; } = new List<InterviewRound>();
        public List<AuditLog> RecentAuditLogs { get; set; } = new List<AuditLog>();

        // Chart.js Data payloads
        public List<string> DeptNames { get; set; } = new List<string>();
        public List<int> DeptPlacedCounts { get; set; } = new List<int>();
        public List<int> DeptTotalCounts { get; set; } = new List<int>();

        public List<string> CompanyNames { get; set; } = new List<string>();
        public List<int> CompanyOfferCounts { get; set; } = new List<int>();

        public List<string> TrendMonths { get; set; } = new List<string>();
        public List<int> TrendPlacedCounts { get; set; } = new List<int>();
    }

    public class StudentDashboardViewModel
    {
        public Student Student { get; set; } = null!;
        public int ProfileCompletionPercentage { get; set; }

        public int AvailableDrivesCount { get; set; }
        public int AppliedDrivesCount { get; set; }
        public int ShortlistedDrivesCount { get; set; }
        public int SelectedDrivesCount { get; set; }

        public List<PlacementDrive> UpcomingDrives { get; set; } = new List<PlacementDrive>();
        public List<Application> RecentApplications { get; set; } = new List<Application>();
        public List<InterviewRound> UpcomingInterviews { get; set; } = new List<InterviewRound>();
        public List<Notification> RecentNotifications { get; set; } = new List<Notification>();
    }
}
