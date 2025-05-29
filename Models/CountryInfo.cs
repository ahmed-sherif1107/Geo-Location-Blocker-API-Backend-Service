using System.Text.Json.Serialization;

namespace Countries.Models
{
	public class CountryInfo
	{
		[JsonPropertyName("name")]
		public CountryName Name { get; set; } = new();

		[JsonPropertyName("cca2")]
		public string CountryCode { get; set; } = string.Empty;

		[JsonPropertyName("flag")]
		public string Flag { get; set; } = string.Empty;

		[JsonPropertyName("capital")]
		public List<string> Capital { get; set; } = new();

		[JsonPropertyName("region")]
		public string Region { get; set; } = string.Empty;

		[JsonPropertyName("subregion")]
		public string Subregion { get; set; } = string.Empty;

		[JsonPropertyName("population")]
		public long Population { get; set; }
	}

	public class CountryName
	{
		[JsonPropertyName("common")]
		public string Common { get; set; } = string.Empty;

		[JsonPropertyName("official")]
		public string Official { get; set; } = string.Empty;
	}
}
