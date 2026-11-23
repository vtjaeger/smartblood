using System.Net.Http.Json;
using SmartBlood.Models;

namespace SmartBlood.Simulator
{
    public class WearableSimulator : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WearableSimulator> _logger;
        private readonly Random _random = new Random();

        public WearableSimulator(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WearableSimulator> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var apiBaseUrl = _configuration["WearableSimulator:ApiBaseUrl"] ?? "http://localhost:5109/";

            var patientId = _configuration.GetValue<int>("WearableSimulator:PatientId", 1);
            var deviceId = _configuration["WearableSimulator:DeviceId"] ?? "wrist-04a7";
            var intervalSeconds = _configuration.GetValue<int>("WearableSimulator:IntervalSeconds", 5);

            if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
            {
                _logger.LogError("URL inválida para o simulador IoT: {ApiBaseUrl}", apiBaseUrl);
                return;
            }

            if (patientId <= 0)
            {
                _logger.LogError("Configure um PatientId válido para o simulador IoT.");
                return;
            }

            if (intervalSeconds <= 0)
            {
                intervalSeconds = 5;
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = baseUri;
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            _logger.LogInformation("Simulador IoT iniciado. API: {ApiBaseUrl} | Paciente: {PatientId} | Dispositivo: {DeviceId}", apiBaseUrl, patientId, deviceId);

            while (!stoppingToken.IsCancellationRequested)
            {
                var measurement = new Measurement
                {
                    PatientId = patientId,
                    DeviceId = deviceId,
                    Systolic = _random.Next(110, 161),
                    Diastolic = _random.Next(70, 101),
                    HeartRate = _random.Next(60, 101),
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    var response = await httpClient.PostAsJsonAsync("api/measurements", measurement, stoppingToken);

                    response.EnsureSuccessStatusCode();

                    _logger.LogInformation("Medição enviada: {Systolic}/{Diastolic} mmHg | {HeartRate} bpm | HTTP {StatusCode}", measurement.Systolic, measurement.Diastolic, measurement.HeartRate, response.StatusCode);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Falha ao enviar medição do dispositivo {DeviceId}. Verifique se a API está disponível e se o paciente {PatientId} existe.", deviceId, patientId);
                }
                catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Tempo limite ao enviar medição do dispositivo {DeviceId}.", deviceId);
                }

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
        }
    }
}
