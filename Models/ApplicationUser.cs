using Microsoft.AspNetCore.Identity;

namespace StudentPlacementManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? UserRole { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
