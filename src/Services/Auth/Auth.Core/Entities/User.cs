using Shared.Common.Entities;
using System.ComponentModel.DataAnnotations;

namespace Auth.Core.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        // Refresh Token Support
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        // Status Fields
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
    }

    public enum UserRole
    {
        User = 0,
        Admin = 1,
        SuperAdmin = 2
    }
}
