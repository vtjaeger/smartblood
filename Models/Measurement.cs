namespace SmartBlood.Models
{
    public class Measurement
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public int Systolic { get; set; }

        public int Diastolic { get; set; }

        public int HeartRate { get; set; }

        public DateTime Timestamp { get; set; }

        public AIAnalysis? AIAnalysis { get; set; }
    }
}