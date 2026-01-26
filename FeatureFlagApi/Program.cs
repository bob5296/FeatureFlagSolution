using FeatureFlagApi.Middleware;
using FeatureFlagCore.Data;
using FeatureFlagCore.Interfaces;
using FeatureFlagCore.Services;
using FeatureFlags.Core.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Feature Flags API", Version = "v1" });
});

// Database configuration - SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=featureflags.db";
builder.Services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
    
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FeatureFlagDbContext>();
    dbContext.Database.EnsureCreated();
}
// Configure the HTTP request pipeline
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
