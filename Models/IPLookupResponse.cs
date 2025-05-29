using System.Text.Json.Serialization;

namespace Countries.Models
{
    public class IPLookupResponse
    {
        [JsonPropertyName("ip")]
        public string? IP { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public LocationInfo Location { get; set; } = new();

        [JsonPropertyName("country_metadata")]
        public CountryMetadata CountryMetadata { get; set; } = new();

        [JsonPropertyName("currency")]
        public CurrencyInfo Currency { get; set; } = new();
    }

    public class LocationInfo
    {
        [JsonPropertyName("continent_code")]
        public string ContinentCode { get; set; } = string.Empty;

        [JsonPropertyName("continent_name")]
        public string ContinentName { get; set; } = string.Empty;

        [JsonPropertyName("country_code2")]
        public string CountryCode2 { get; set; } = string.Empty;

        [JsonPropertyName("country_code3")]
        public string CountryCode3 { get; set; } = string.Empty;

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("country_name_official")]
        public string CountryNameOfficial { get; set; } = string.Empty;

        [JsonPropertyName("country_capital")]
        public string CountryCapital { get; set; } = string.Empty;

        [JsonPropertyName("state_prov")]
        public string StateProv { get; set; } = string.Empty;

        [JsonPropertyName("state_code")]
        public string StateCode { get; set; } = string.Empty;

        [JsonPropertyName("district")]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("zipcode")]
        public string Zipcode { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; } = string.Empty;

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; } = string.Empty;

        [JsonPropertyName("is_eu")]
        public bool IsEu { get; set; }

        [JsonPropertyName("country_flag")]
        public string CountryFlag { get; set; } = string.Empty;

        [JsonPropertyName("geoname_id")]
        public string GeonameId { get; set; } = string.Empty;

        [JsonPropertyName("country_emoji")]
        public string CountryEmoji { get; set; } = string.Empty;
    }

    public class CountryMetadata
    {
        [JsonPropertyName("calling_code")]
        public string CallingCode { get; set; } = string.Empty;

        [JsonPropertyName("tld")]
        public string Tld { get; set; } = string.Empty;

        [JsonPropertyName("languages")]
        public List<string> Languages { get; set; } = new();
    }

    public class CurrencyInfo
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }
}