namespace StudentPlacementManagement.Models
{
    public class DriveDepartment
    {
        public int DriveDepartmentId { get; set; }
        public int DriveId { get; set; }
        public int DepartmentId { get; set; }

        // Navigation Properties
        public virtual PlacementDrive? PlacementDrive { get; set; }
        public virtual Department? Department { get; set; }
    }
}
