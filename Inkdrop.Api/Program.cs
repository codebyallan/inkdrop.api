using Inkdrop.Api.Data;
using Inkdrop.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Get the connection string from configuration
var connectionString = builder.Configuration.GetSection("DbConfig:ConnectionString").Value
    ?? throw new InvalidOperationException("Connection string 'DbConfig:ConnectionString' not found."); ;

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<LocationService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();