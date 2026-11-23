
using Microsoft.EntityFrameworkCore;
using SmartBlood.Data;
using SmartBlood.Services;
using SmartBlood.Simulator;
using SmartBlood.FHIR;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Measurement Service
builder.Services.AddScoped<MeasurementService>();

// FHIR Service
builder.Services.AddScoped<FhirObservationService>();

// Patient Service
builder.Services.AddScoped<PatientService>();

// AI Service
var aiBaseUrl = builder.Configuration["AI:BaseUrl"]
    ?? throw new InvalidOperationException(
        "A configuração AI:BaseUrl não foi encontrada."
    );

builder.Services.AddHttpClient<AIService>(client =>
{
    client.BaseAddress = new Uri(aiBaseUrl);
});

// MySQL + Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SmartBloodDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Wearable Simulator
builder.Services.AddHttpClient();

var simulatorEnabled = builder.Configuration.GetValue<bool>(
    "WearableSimulator:Enabled"
);

if (simulatorEnabled)
{
    builder.Services.AddHostedService<WearableSimulator>();
}

// Build
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
