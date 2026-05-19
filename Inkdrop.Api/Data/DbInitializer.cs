using Inkdrop.Api.DTOs.Requests;
using Inkdrop.Api.Entities;
using Inkdrop.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inkdrop.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userService = serviceProvider.GetRequiredService<IUserService>();

        // Ensure database is created and migrations applied
        await context.Database.MigrateAsync();

        // Check if any user exists
        if (!await context.Users.AnyAsync())
        {
            // Create initial Admin user using the UserService to ensure salt/hash logic is consistent
            var adminRequest = new RegisterRequest(
                Username: "admin",
                Email: "admin@inkdrop.com",
                Password: "Admin@123!",
                Role: UserRole.Admin
            );

            await userService.CreateUserAsync(adminRequest);
        }
    }
}
