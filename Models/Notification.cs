using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "General"; // General, Placement, Application, Interview, Result

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }
    }
}
