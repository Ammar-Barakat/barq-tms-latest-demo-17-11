using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using BarqTMS.API.Data;
using BarqTMS.API.Services;
using BarqTMS.API.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<BarqTMSDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? ""))
        };

        // Allow SignalR connections to authenticate via the access_token query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // If the request is for our SignalR hubs and contains an access token, use it
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/notifications") ||
                     path.StartsWithSegments("/hub/notifications") ||
                     path.StartsWithSegments("/notificationHub") ||
                     path.StartsWithSegments("/hubs/notificationHub")))
                {
                    context.Token = accessToken!;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add global exception handler
builder.Services.AddGlobalExceptionHandler();

// Add services
builder.Services.AddScoped<BarqTMS.API.Services.AuthService>();
builder.Services.AddScoped<BarqTMS.API.Services.IAuditService, BarqTMS.API.Services.AuditService>();
builder.Services.AddScoped<BarqTMS.API.Services.IFileStorageService, BarqTMS.API.Services.LocalFileStorageService>();
builder.Services.AddScoped<BarqTMS.API.Services.IEmailService, BarqTMS.API.Services.EmailService>();
builder.Services.AddScoped<BarqTMS.API.Services.ISearchService, BarqTMS.API.Services.SearchService>();
builder.Services.AddScoped<BarqTMS.API.Services.IRealTimeService, BarqTMS.API.Services.RealTimeService>();
builder.Services.AddScoped<BarqTMS.API.Services.ICalendarService, BarqTMS.API.Services.CalendarService>();
builder.Services.AddScoped<BarqTMS.API.Services.IReportingService, BarqTMS.API.Services.ReportingService>();
builder.Services.AddScoped<BarqTMS.API.Services.ISecurityService, BarqTMS.API.Services.SecurityService>();
// Register client service
builder.Services.AddScoped<BarqTMS.API.Services.IClientService, BarqTMS.API.Services.ClientService>();
// Register task service
builder.Services.AddScoped<BarqTMS.API.Services.ITaskService, BarqTMS.API.Services.TaskService>();

// Register background services
builder.Services.AddHostedService<BarqTMS.API.Services.OverdueTaskNotificationService>();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Add HTTP context accessor for audit service
builder.Services.AddHttpContextAccessor();

// Add controllers with JSON configuration to preserve PascalCase
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Keep PascalCase property names to match C# DTOs
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        
        // ✅ REMOVED JsonStringEnumConverter to return enums as NUMBERS
        // This fixes login issue - Frontend expects User.Role as NUMBER not STRING
        // options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
   c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
   {
       Title = "Barq Task Management System API",
       Version = "v1",
       Description = "A comprehensive task management system for organizations"
   });
});

builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins(
                    "http://127.0.0.1:5501" // Frontend web server alternative
                )
                .SetIsOriginAllowed(origin => true) // Allow any origin including file:///
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
       c.SwaggerEndpoint("/swagger/v1/swagger.json", "Barq TMS API V1");
       c.RoutePrefix = string.Empty;
    });

    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Only enforce HTTPS in non-dev
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

// Add exception handling
app.UseExceptionHandler();

// Add activity logging middleware
app.UseActivityLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<BarqTMS.API.Hubs.NotificationHub>("/hubs/notifications");

// Configure static files for file uploads
app.UseStaticFiles();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BarqTMSDbContext>();
    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
    
    try
    {
        await BarqTMS.API.Data.DatabaseSeeder.SeedDatabaseAsync(context, authService);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
