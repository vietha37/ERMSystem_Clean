using ERMSystem.Application.Interfaces;
using ERMSystem.Application.Services;
using ERMSystem.Infrastructure.Repositories;
using ERMSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ERMSystem.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── DI – Auth ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── DI – Patient ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

// ── DI – Doctor ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

// ── DI – Medicine ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<IMedicineService, MedicineService>();

// ── DI – Appointment ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// ── DI – MedicalRecord ────────────────────────────────────────────────────────
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();

// ── DI – Prescription ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();

// ── DI – Dashboard ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                   ?? new[] { "http://localhost:3000", "http://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .SetIsOriginAllowed(origin =>
            {
                if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!builder.Environment.IsDevelopment())
                {
                    return false;
                }

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                var host = uri.Host;
                if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host == "127.0.0.1")
                {
                    return true;
                }

                if (host.StartsWith("192.168.", StringComparison.Ordinal))
                {
                    return true;
                }

                if (host.StartsWith("10.", StringComparison.Ordinal))
                {
                    return true;
                }

                if (host.StartsWith("172.", StringComparison.Ordinal))
                {
                    var parts = host.Split('.');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var secondOctet))
                    {
                        return secondOctet >= 16 && secondOctet <= 31;
                    }
                }

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ── HTTP Pipeline ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
