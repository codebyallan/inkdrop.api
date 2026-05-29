using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Inkdrop.Api.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.Encodings.Web;

namespace Inkdrop.Api.Extensions;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKey";
    public string HeaderName { get; set; } = "X-API-KEY";
}

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder urlEncoder) 
        : base(options, loggerFactory, urlEncoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Options == null) return AuthenticateResult.Fail("Options are missing.");

        if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var dbContext = this.Context.RequestServices.GetRequiredService<ApplicationDbContext>();

        var hashedKey = HashKey(providedApiKey);
        var apiKeyEntity = await dbContext.ApiKeys
            .FirstOrDefaultAsync(a => a.KeyHash == hashedKey && a.IsActive)
            .ConfigureAwait(false);

        if (apiKeyEntity == null)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        apiKeyEntity.UpdateLastUsed();
        await dbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, apiKeyEntity.Name),
            new Claim(ClaimTypes.Role, "Bot"),
            new Claim("ApiKeyId", apiKeyEntity.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string HashKey(string key)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(bytes);
    }
}
