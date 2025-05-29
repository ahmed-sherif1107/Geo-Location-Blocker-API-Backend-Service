namespace Countries.Dtos
{
	public class IPGeolocationResponse
	{
		public string? Ip { get; set; }
		public string? CountryCode { get; set; }
		public string? CountryName { get; set; }
		public string? Isp { get; set; }
		// add other if needed
	}
}
