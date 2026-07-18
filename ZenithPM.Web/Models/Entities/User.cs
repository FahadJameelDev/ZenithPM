using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public DateTime? LockEndUtc { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsMfaEnabled { get; set; }
        public string? MfaSecretKey { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<SecurityLog> SecurityLogs { get; set; } = new List<SecurityLog>();
    }
}