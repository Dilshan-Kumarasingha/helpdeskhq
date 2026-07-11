using Microsoft.EntityFrameworkCore;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<HelpDeskHQ.Infrastructure.Data.HelpDeskHQDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISlaService, SlaService>();
builder.Services.AddScoped<ITicketService, TicketService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISlaPolicyService, SlaPolicyService>();

// Background jobs — registered so Hangfire can resolve them via DI when triggered
builder.Services.AddScoped<HelpDeskHQ.API.Jobs.SlaEscalationJob>();
builder.Services.AddScoped<HelpDeskHQ.API.Jobs.AutoCloseJob>();

// Hangfire — uses the same PostgreSQL database
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddScoped<HelpDeskHQ.Core.Interfaces.IRealtimeNotifier, HelpDeskHQ.API.Hubs.SignalRNotifier>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed initial Team/Category/SlaPolicy data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HelpDeskHQ.Infrastructure.Data.HelpDeskHQDbContext>();
    await HelpDeskHQ.Infrastructure.Data.DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<HelpDeskHQ.API.Middleware.ExceptionHandlingMiddleware>();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard — accessible at /hangfire in dev
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapHub<HelpDeskHQ.API.Hubs.TicketHub>("/hubs/tickets");

// Register recurring background jobs
RecurringJob.AddOrUpdate<HelpDeskHQ.API.Jobs.SlaEscalationJob>(
    "sla-escalation-check",
    job => job.ExecuteAsync(),
    "*/5 * * * *"); // every 5 minutes

RecurringJob.AddOrUpdate<HelpDeskHQ.API.Jobs.AutoCloseJob>(
    "auto-close-resolved-tickets",
    job => job.ExecuteAsync(),
    "0 * * * *"); // every hour

app.Run();