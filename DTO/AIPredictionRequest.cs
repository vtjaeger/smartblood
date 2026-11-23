using System.Text.Json.Serialization;

namespace SmartBlood.DTO
{
    public class AIPredictionRequest
    {
        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("height_cm")]
        public int HeightCm { get; set; }

        [JsonPropertyName("weight_kg")]
        public double WeightKg { get; set; }

        [JsonPropertyName("bmi")]
        public double Bmi { get; set; }

        [JsonPropertyName("systolic_bp")]
        public int SystolicBp { get; set; }

        [JsonPropertyName("diastolic_bp")]
        public int DiastolicBp { get; set; }

        [JsonPropertyName("cholesterol_mg_dL")]
        public int CholesterolMgDl { get; set; }

        [JsonPropertyName("blood_glucose_mg_dL")]
        public int BloodGlucoseMgDl { get; set; }

        [JsonPropertyName("smoking")]
        public string Smoking { get; set; } = string.Empty;

        [JsonPropertyName("alcohol_intake")]
        public string AlcoholIntake { get; set; } = string.Empty;

        [JsonPropertyName("physical_activity")]
        public string PhysicalActivity { get; set; } = string.Empty;

        [JsonPropertyName("family_history")]
        public string FamilyHistory { get; set; } = string.Empty;
    }
}