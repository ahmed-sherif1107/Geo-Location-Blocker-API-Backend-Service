using System;

namespace Countries.Models
{
    public class BlockedAttemptLog
    {
        public Guid Id { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool BlockedStatus { get; set; }
        public string RequestPath { get; set; } = string.Empty;
    }
}