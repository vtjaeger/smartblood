namespace SmartBlood.Models
{
    public class AIAnalysis
    {
        public int Id { get; set; }

        public int MeasurementId { get; set; }

        public string Classification { get; set; }

        public double NormalProbability { get; set; }

        public double Stage1Probability { get; set; }

        public double Stage2Probability { get; set; }

        public DateTime CreatedAt { get; set; }

        public Measurement Measurement { get; set; } = null!;
    }
}