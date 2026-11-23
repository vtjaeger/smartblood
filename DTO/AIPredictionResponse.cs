namespace SmartBlood.DTO
{
    public class AIPredictionResponse
    {
        public string Classification { get; set; } = string.Empty;

        public Dictionary<string, double> Probabilities { get; set; } = new Dictionary<string, double>();
    }
}