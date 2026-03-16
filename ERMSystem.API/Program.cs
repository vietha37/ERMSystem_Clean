using Microsoft.EntityFrameworkCore;
using ERMSystem.Application.Interfaces;
using ERMSystem.Application.Services;
using ERMSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ERMSystem.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ??ng k d?ch v? sinh OpenAPI document
builder.Services.AddOpenApi();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // C?u h�nh endpoint ?? sinh ra file OpenAPI JSON (m?c ??nh l� /openapi/v1.json)
    app.MapOpenApi();

    // K�ch ho?t giao di?n Swagger UI v� tr? t?i file JSON ? tr�n
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
        options.RoutePrefix = "swagger"; // Truy c?p UI t?i https://localhost:<port>/swagger
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();