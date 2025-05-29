using Countries.Models;

namespace Countries.Services
{
	public interface IGeoLocationService
	{
		Task<IPLookupResponse> GetLocationByIPAsync(string ipAddress);
		Task<IPLookupResponse> GetCurrentLocationAsync();
	}
}
