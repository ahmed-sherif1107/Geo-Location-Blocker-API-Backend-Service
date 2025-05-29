namespace Countries.Dtos
{
    public class BlockCountryRequest
    {
        public string CountryCode { get; set; } = string.Empty;
        public bool IsTemporary { get; set; }
        public int? DurationMinutes { get; set; }
    }
}