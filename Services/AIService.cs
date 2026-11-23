using SmartBlood.DTO;
using System.Net.Http.Json;

namespace SmartBlood.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AIPredictionResponse?> PredictAsync(AIPredictionRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("predict", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AIPredictionResponse>(cancellationToken: cancellationToken);
        }
    }
}
