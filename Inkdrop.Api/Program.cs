using Inkdrop.Api.Data;
using Inkdrop.Api.Extensions;
using Inkdrop.Api.Filters;
using Inkdrop.Api.Interfaces;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


// Get the connection string from configuration
var connectionString = builder.Configuration.GetSection("DbConfig:ConnectionString").Value
    ?? throw new InvalidOperationException("Connection string 'DbConfig:ConnectionString' not found.");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<ITonerService, TonerService>();
builder.Services.AddScoped<IMovementsService, MovementsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddScoped<NotificationContext>();
builder.Services.AddCustomCors(builder.Configuration);

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Inkdrop.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName, options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Technician"));
    options.AddPolicy("BotPolicy", policy => 
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.SchemeName)
              .RequireAuthenticatedUser());
});

builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddRateLimiter(options =>
{
    // Policy for Auth endpoints: Strict limit per IP
    options.AddFixedWindowLimiter("auth-policy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Policy for Bot endpoints: Limit per API Key
    options.AddPolicy("bot-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-API-KEY"].ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(30),
                QueueLimit = 0
            }));

    // General policy for authenticated users
    options.AddFixedWindowLimiter("general-policy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        var errorResponse = new Inkdrop.Api.DTOs.Responses.ErrorResponse(
            new[] { new NotificationMessage("RateLimitExceeded", "Too many requests. Please try again later.") });

        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, token);
    };
});

// Global exception handling
builder.Services.AddExceptionHandler<Inkdrop.Api.Handlers.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
    options.Filters.Add<NotificationFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Inkdrop Api",
        Description = "An ASP.NET Core Web API for managing Inkdrop resources"
    });
});

var app = builder.Build();

app.UseCors("DefaultCors");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable Swagger only in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
        options.InjectJavascript("/swagger-compat.js");
    });
}

app.UseExceptionHandler();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.MapControllers();
app.Run();