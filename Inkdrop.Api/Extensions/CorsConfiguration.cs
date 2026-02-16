namespace Inkdrop.Api.Extensions;

public static class CorsConfiguration
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("AllowedOrigins").Get<string[]>();

        if (origins == null || origins.Length == 0)
        {
            throw new InvalidOperationException("CORS Origins not found in configuration.");
        }

        return services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });
    }
}