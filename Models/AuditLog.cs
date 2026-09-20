using System.ComponentModel.DataAnnotations;

namespace StudentPlacementManagement.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }
    }
}
