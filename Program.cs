using Countries.Services;
using Countries.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

namespace Countries
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>(client =>
            {
                client.BaseAddress = new Uri("https://api.ipgeolocation.io/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });


			builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
			{
				client.BaseAddress = new Uri("https://restcountries.com/");
				client.Timeout = TimeSpan.FromSeconds(30);
				client.DefaultRequestHeaders.Add("User-Agent", "Countries-API-Client");
			});


			// Register repositories
			builder.Services.AddSingleton<IBlockedCountriesRepository, BlockedCountriesRepository>();
            builder.Services.AddSingleton<IBlockedAttemptsRepository, BlockedAttemptsRepository>();

            // Register background service
            builder.Services.AddHostedService<TemporaryBlockCleanupService>();

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Add configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();

            // Registering the IConfiguration instance which IP Geolocation Service will use
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable forwarded headers for proxies/ngrok
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
