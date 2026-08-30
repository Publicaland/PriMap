using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PriMap.Data
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
