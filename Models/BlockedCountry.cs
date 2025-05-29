using System;

namespace Countries.Models
{
    public class BlockedCountry
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
        public bool IsTemporary { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? BlockedBy { get; set; }
    }
}