using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20)]
        [Display(Name = "Department Code")]
        public string DepartmentCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<DriveDepartment> DriveDepartments { get; set; } = new List<DriveDepartment>();
    }
}
