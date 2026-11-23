namespace SmartBlood.DTO
{
    public class MeasurementHistoryResponse
    {
        public int MeasurementId { get; set; }

        public int PatientId { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public int Systolic { get; set; }

        public int Diastolic { get; set; }

        public int HeartRate { get; set; }

        public DateTime Timestamp { get; set; }

        public AIAnalysisHistoryResponse? AIAnalysis { get; set; }
    }

    public class AIAnalysisHistoryResponse
    {
        public string Classification { get; set; } = string.Empty;

        public Dictionary<string, double> Probabilities { get; set; }            = new Dictionary<string, double>();
    }
}