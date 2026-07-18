using System;

namespace ZenithPM.Web.Models.Entities
{
    public class SecurityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // Login, FailedAttempt, MfaVerified, Logout
        public DateTime Timestamp { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}