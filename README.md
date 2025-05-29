# Countries API

A .NET 8 Web API for country blocking and geolocation management with comprehensive IP monitoring and logging capabilities. The application provides real-time country information validation, IP geolocation services, and maintains detailed audit logs of all blocking operations.

## 🚀 Features

### Country Management
- **Block/Unblock Countries**: Permanently or temporarily block countries by their ISO country codes
- **Country Validation**: Validate country codes against the REST Countries API
- **Automatic Country Information**: Fetch real-time country data including official names, currencies, and regional information
- **Temporary Blocking**: Set time-based country blocks with automatic expiration (1-1440 minutes)
- **Background Cleanup**: Automated removal of expired temporary blocks every 5 minutes

### IP Geolocation & Monitoring
- **Real-time IP Lookup**: Determine country location for any IP address using IPGeolocation.io API
- **Smart IP Detection**: Automatically detect client IP addresses through proxy headers (X-Forwarded-For, X-Real-IP)
- **Public IP Resolution**: Fallback to public IP detection for private/local addresses
- **Block Status Checking**: Verify if an IP address originates from a blocked country

### Comprehensive Logging & Audit Trail
- **Operation Logging**: Track all blocking, unblocking, and IP check operations
- **Detailed Metadata**: Log IP addresses, timestamps, user agents, country information, and request paths
- **Search & Filter**: Search logs by country code, country name, or IP address
- **Pagination Support**: Browse through logged attempts with configurable page sizes
- **Action Categories**: 
  - `BLOCK_COUNTRY` - Permanent country blocking
  - `TEMPORARY_BLOCK_COUNTRY` - Time-limited country blocking
  - `UNBLOCK_COUNTRY` - Country unblocking
  - `IP_CHECK` - IP block status verification

### Additional Features
- **Thread-Safe Operations**: In-memory repositories with concurrent access support
- **Proxy-Friendly**: Full support for reverse proxies, load balancers, and ngrok
- **Docker Support**: Complete containerization with multi-stage builds
- **Swagger Documentation**: Interactive API testing interface
- **Robust Error Handling**: Comprehensive exception handling with detailed error responses
- **Configurable Settings**: Flexible configuration for API keys, rate limits, and blocking parameters

## 🏗️ Architecture

### Technology Stack
- **.NET 8**: Latest LTS framework with enhanced performance
- **ASP.NET Core**: Web API framework with built-in DI container
- **In-Memory Storage**: Thread-safe repositories for high-performance operations
- **HTTP Clients**: Integrated HTTP client factory for external API calls
- **Background Services**: Hosted services for automated cleanup tasks

### External APIs
- **REST Countries API**: Real-time country information and validation
- **IPGeolocation.io**: IP-to-location conversion and public IP detection

### Key Components
```
├── Controllers/
│   ├── CountriesController.cs    # Country blocking/unblocking operations
│   ├── IPController.cs           # IP geolocation and block checking
│   └── LogsController.cs         # Audit log retrieval
├── Services/
│   ├── BlockedCountriesRepository.cs     # Country blocking storage
│   ├── BlockedAttemptsRepository.cs      # Audit log storage
│   ├── CountryService.cs                 # REST Countries API integration
│   ├── GeoLocationService.cs             # IPGeolocation.io integration
│   └── TemporaryBlockCleanupService.cs   # Background cleanup service
├── Models/
│   ├── BlockedCountry.cs         # Country blocking entity
│   ├── BlockedAttemptLog.cs      # Audit log entity
│   ├── CountryInfo.cs            # Country information model
│   └── IPLookupResponse.cs       # Geolocation response model
└── Dtos/
    ├── BlockCountryRequest.cs    # API request models
    ├── PaginationRequest.cs      # Pagination parameters
    └── PaginatedResponse.cs      # Paginated response wrapper
```

## 🚦 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [IPGeolocation.io API Key](https://ipgeolocation.io/) (Free tier available - 30,000 requests/month)
- [ngrok](https://ngrok.com/) (Optional - for public testing)

### Installation

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd Countries
   ```

2. **Configure API Key**
   
   Edit `appsettings.json`:
   ```json
   {
     "IPGeolocation": {
       "ApiKey": "YOUR_IPGEOLOCATION_API_KEY_HERE"
     }
   }
   ```

3. **Build and Run**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
   
   The API will be available at: `http://localhost:5053`

4. **Access Swagger UI**
   
   Navigate to: `http://localhost:5053/swagger`

### Docker Deployment

```bash
# Build the image
docker build -t countries-api .

# Run the container
docker run -p 5053:8080 -e IPGeolocation__ApiKey=YOUR_API_KEY countries-api
```

### Public Access with ngrok

```bash
# Expose local API publicly
ngrok http 5053

# Use the provided HTTPS URL for external access
# Example: https://abc123.ngrok-free.app
```

## 📚 API Documentation

### Country Management Endpoints

#### Block a Country
```http
POST /api/countries/block
Content-Type: application/json

{
  "countryCode": "US",
  "isTemporary": false
}
```

#### Temporarily Block a Country
```http
POST /api/countries/temporal-block
Content-Type: application/json

{
  "countryCode": "RU",
  "isTemporary": true,
  "durationMinutes": 120
}
```

#### Unblock a Country
```http
DELETE /api/countries/block/US
```

#### List Blocked Countries
```http
GET /api/countries/blocked?page=1&pageSize=10&searchTerm=United
```

#### Validate Country Code
```http
GET /api/countries/validate/US
```

### IP Geolocation Endpoints

#### Lookup IP Location
```http
# Lookup specific IP
GET /api/IP/country-lookup/8.8.8.8

# Lookup caller's IP
GET /api/IP/country-lookup/
```

#### Check Block Status
```http
# Check specific IP
GET /api/IP/check-block/8.8.8.8

# Check caller's IP
GET /api/IP/check-block/
```

### Audit Log Endpoints

#### Retrieve Blocked Attempts
```http
# Get all attempts
GET /api/logs/blocked-attempts

# Search and paginate
GET /api/logs/blocked-attempts?searchTerm=US&page=1&pageSize=20
```

## 📊 Response Examples

### Country Blocking Response
```json
{
  "countryCode": "US",
  "countryName": "United States",
  "blockedAt": "2025-05-29T20:10:07.123Z",
  "isTemporary": false,
  "expiresAt": null,
  "blockedBy": null
}
```

### IP Lookup Response
```json
{
  "ip": "8.8.8.8",
  "location": {
    "countryCode2": "US",
    "countryName": "United States",
    "city": "Mountain View",
    "region": "California",
    "latitude": 37.4056,
    "longitude": -122.0775,
    "timezone": "America/Los_Angeles"
  }
}
```

### Block Check Response
```json
{
  "isBlocked": true,
  "country": {
    "code": "US",
    "name": "United States"
  }
}
```

### Audit Log Response
```json
{
  "items": [
    {
      "id": "a5b6de8a-2f3b-4345-998c-fb5404703750",
      "ipAddress": "203.0.113.1",
      "timestamp": "2025-05-29T20:11:59.227Z",
      "countryCode": "US",
      "countryName": "United States",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
      "blockedStatus": true,
      "requestPath": "BLOCK_COUNTRY: /api/countries/block"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## ⚙️ Configuration

### Application Settings
```json
{
  "IPGeolocation": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.ipgeolocation.io/",
    "RateLimit": {
      "RequestsPerMinute": 30
    }
  },
  "Blocking": {
    "MinTemporaryBlockMinutes": 1,
    "MaxTemporaryBlockMinutes": 1440,
    "DefaultPageSize": 10,
    "MaxPageSize": 100
  }
}
```

### Environment Variables
- `IPGeolocation__ApiKey`: Your IPGeolocation.io API key
- `ASPNETCORE_ENVIRONMENT`: Runtime environment (Development/Production)
- `ASPNETCORE_URLS`: Binding URLs (default: http://localhost:5053)

## 🔄 Background Services

### Temporary Block Cleanup Service
- **Frequency**: Every 5 minutes
- **Function**: Automatically removes expired temporary country blocks
- **Logging**: Detailed startup and completion logging
- **Error Handling**: Graceful error recovery with continued operation

## 🛡️ Security Considerations

- **API Key Protection**: Store API keys in secure configuration or environment variables
- **Input Validation**: All inputs are validated against expected formats and ranges
- **Rate Limiting**: Built-in support for API rate limiting configuration
- **Proxy Headers**: Secure handling of forwarded headers for real IP detection
- **Error Information**: Detailed error responses without sensitive data exposure

## 🚀 Deployment Scenarios

### Development
```bash
dotnet run --environment Development
```

### Production
```bash
dotnet publish -c Release -o ./publish
cd ./publish
dotnet Countries.dll
```

### Docker Production
```bash
docker build -t countries-api:latest .
docker run -d -p 80:8080 --name countries-api-prod countries-api:latest
```

## 📈 Performance

- **In-Memory Storage**: High-performance thread-safe operations
- **HTTP Client Pooling**: Efficient connection reuse for external APIs
- **Background Processing**: Non-blocking cleanup operations
- **Minimal Dependencies**: Lightweight runtime with essential packages only

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the Swagger documentation at `/swagger`
- Review the application logs for detailed error information
- Contact me using phone : +20 101 537 4542  Or via Linkedin : https://www.linkedin.com/feed/](https://www.linkedin.com/in/ahmedsheriff/

---

**Built with ❤️ using .NET 8 and ASP.NET Core**
