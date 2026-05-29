using System.Security.Cryptography;
using System.Text;
using Inkdrop.Api.Data;
using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.DTOs.Responses;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Core;
using Microsoft.EntityFrameworkCore;
using Inkdrop.Api.Notifications;

namespace Inkdrop.Api.Services;

public sealed class ApiKeyService(ApplicationDbContext dbContext, NotificationContext notificationContext) : IApiKeyService
{
    public async Task<ServiceResult<string>> CreateKeyAsync(CreateApiKeyRequest request)
    {
        // Generate a secure random key
        var plainKey = GenerateSecureRandomKey();
        var hashedKey = HashKey(plainKey);

        var apiKey = new ApiKey(request.Name, hashedKey);

        if (!apiKey.IsValid)
        {
            notificationContext.AddNotifications(apiKey);
            return ServiceResult<string>.Failure();
        }
        
        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync();

        // Return the plain key only once. It's not stored in the DB.
        return ServiceResult<string>.Success(plainKey);
    }

    public async Task<IEnumerable<ApiKeyResponse>> GetActiveKeysAsync()
    {
        return await dbContext.ApiKeys
            .Where(a => a.IsActive)
            .Select(a => new ApiKeyResponse(a.Id, a.Name, a.CreatedAt, a.LastUsedAt))
            .ToListAsync();
    }

    public async Task<ServiceResult<bool>> UpdateKeyNameAsync(Guid id, UpdateApiKeyRequest request)
    {
        var apiKey = await dbContext.ApiKeys.FindAsync([id]);
        if (apiKey == null) return ServiceResult<bool>.NotFound();

        apiKey.UpdateName(request.Name);

        if (!apiKey.IsValid)
        {
            notificationContext.AddNotifications(apiKey);
            return ServiceResult<bool>.Failure();
        }

        await dbContext.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> RevokeKeyAsync(Guid id)
    {
        var apiKey = await dbContext.ApiKeys.FindAsync([id]);
        if (apiKey == null) return ServiceResult<bool>.NotFound();

        apiKey.Deactivate();
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static string GenerateSecureRandomKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(bytes);
    }
}
