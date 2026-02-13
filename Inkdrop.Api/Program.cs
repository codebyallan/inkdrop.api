using Inkdrop.Api.Data;
using Inkdrop.Api.Filters;
using Inkdrop.Api.Notifications;
using Inkdrop.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


// Get the connection string from configuration
var connectionString = builder.Configuration.GetSection("DbConfig:ConnectionString").Value
    ?? throw new InvalidOperationException("Connection string 'DbConfig:ConnectionString' not found."); ;

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<PrinterService>();
builder.Services.AddScoped<TonerService>();
builder.Services.AddScoped<MovementsService>();
builder.Services.AddScoped<NotificationContext>();

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
    });
}

app.UseExceptionHandler();

app.MapControllers();
app.Run();