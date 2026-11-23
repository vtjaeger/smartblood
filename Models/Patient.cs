namespace SmartBlood.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int HeightCm { get; set; }
        public double WeightKg { get; set; }
        public double BMI { get; set; }
        public int CholesterolMgDl { get; set; }
        public int BloodGlucoseMgDl { get; set; }
        public string Smoking { get; set; } = string.Empty;
        public string AlcoholIntake { get; set; } = string.Empty;
        public string PhysicalActivity { get; set; } = string.Empty;
        public string FamilyHistory { get; set; } = string.Empty;
    }
}
