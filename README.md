# Countries API

A .NET Core Web API for managing blocked countries and validating IP addresses using third-party geolocation APIs (IPGeolocation.io). The application uses in-memory data storage and is suitable for demo, test, or small-scale production use.

## Features

- **Block/Unblock Countries:**
  - Add or remove countries from a blocked list (in-memory, thread-safe).
- **List Blocked Countries:**
  - Paginated and filterable endpoint to view all blocked countries.
- **IP Geolocation Lookup:**
  - Lookup country and location info for any IP address using IPGeolocation.io.
  - If no IP is provided, attempts to use the real client IP (supports proxies/ngrok).
- **Check If IP is Blocked:**
  - Checks if the caller's (or provided) IP is from a blocked country.
  - Logs all block check attempts (in-memory, paginated).
- **Temporarily Block Countries:**
  - Temporarily block a country for a set duration (e.g., 2 hours), with automatic unblocking via a background service.
- **Swagger UI:**
  - Interactive API documentation and testing interface.

## Setup Instructions

### Prerequisites
- [.NET 7/8/9 SDK](https://dotnet.microsoft.com/download)
- [ngrok](https://ngrok.com/) (for public testing)
- Free API key from [IPGeolocation.io](https://ipgeolocation.io/)

### 1. Clone the Repository
```bash
git clone <your-repo-url>
cd Countries
```

### 2. Configure API Key
Edit `appsettings.json` and add your IPGeolocation.io API key:
```json
{
  "IPGeolocation": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### 3. Run the API
```bash
dotnet run
```
<<<<<<< HEAD
The API will start (by default on `https://localhost:7004` or similar).

### 4. Expose the API via ngrok
```bash
ngrok http 7004
=======
The API will start (by default on `http://localhost:5053` or similar).

### 4. Expose the API via ngrok
```bash
ngrok http 5053
>>>>>>> b1d9e79ae0447b6f374097b03f2d1b27123ad361
```
- ngrok will provide a public URL (e.g., `https://xxxx.ngrok-free.app`).
- Use this URL to access the API from any device or network.

## API Endpoints

### Blocked Countries
- `POST /api/countries/block` — Block a country (by code)
- `DELETE /api/countries/block/{countryCode}` — Unblock a country
- `GET /api/countries/blocked` — List blocked countries (pagination, filtering)
- `POST /api/countries/temporal-block` — Temporarily block a country

### IP Lookup & Block Check
- `GET /api/IP/country-lookup/{ipAddress?}` — Lookup country info for an IP (or caller if omitted)
- `GET /api/IP/check-block/{ipAddress?}` — Check if an IP (or caller) is from a blocked country

### Blocked Attempts Log
- `GET /api/logs/blocked-attempts` — Paginated log of block check attempts

### Swagger UI
- Visit `/swagger` on your ngrok/public URL for interactive API docs.

## Notes on IP Detection
- The API attempts to detect the real client IP, even when behind ngrok or a proxy, by checking the `X-Forwarded-For` header.
- If the client IP is private/local, the API falls back to using the public IP as seen by the backend.
- For best results, test from a device on a different network (e.g., mobile data) and use the ngrok public URL.

## Example Usage
- Block a country: `POST /api/countries/block` with body `{ "countryCode": "US" }`
- Lookup your country: `GET https://<ngrok-url>/api/IP/country-lookup/`
- Check if your IP is blocked: `GET https://<ngrok-url>/api/IP/check-block/`

## License
<<<<<<< HEAD
MIT 
=======
MIT 
>>>>>>> b1d9e79ae0447b6f374097b03f2d1b27123ad361
