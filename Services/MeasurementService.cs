using Microsoft.EntityFrameworkCore;
using SmartBlood.Data;
using SmartBlood.DTO;
using SmartBlood.Models;

namespace SmartBlood.Services
{
    public class MeasurementService
    {
        private readonly SmartBloodDbContext _context;
        private readonly AIService _aiService;

        public MeasurementService(SmartBloodDbContext context, AIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<object> CreateMeasurement(     Measurement measurement,     CancellationToken cancellationToken = default)
        {
            if (measurement == null)
                throw new ArgumentException("Os dados da medição são obrigatórios.");

            if (measurement.PatientId <= 0)
                throw new ArgumentException("Informe um PatientId válido.");

            if (string.IsNullOrWhiteSpace(measurement.DeviceId))
                throw new ArgumentException("O DeviceId do wearable é obrigatório.");

            if (measurement.Systolic < 50 || measurement.Systolic > 300)
                throw new ArgumentException("A pressão sistólica deve estar entre 50 e 300 mmHg.");

            if (measurement.Diastolic < 30 || measurement.Diastolic > 200)
                throw new ArgumentException("A pressão diastólica deve estar entre 30 e 200 mmHg.");

            if (measurement.HeartRate < 20 || measurement.HeartRate > 250)
                throw new ArgumentException("A frequência cardíaca deve estar entre 20 e 250 bpm.");

            var patient = await _context.Patients                .FirstOrDefaultAsync(                    x => x.Id == measurement.PatientId,                    cancellationToken                );

            if (patient == null)
                throw new KeyNotFoundException(                    $"Paciente {measurement.PatientId} não encontrado."                );

            if (measurement.Timestamp == default)
                measurement.Timestamp = DateTime.UtcNow;

            _context.Measurements.Add(measurement);
            await _context.SaveChangesAsync(cancellationToken);

            var request = new AIPredictionRequest
            {
                Age = patient.Age,
                Gender = patient.Gender,
                HeightCm = patient.HeightCm,
                WeightKg = patient.WeightKg,
                Bmi = patient.BMI,
                SystolicBp = measurement.Systolic,
                DiastolicBp = measurement.Diastolic,
                CholesterolMgDl = patient.CholesterolMgDl,
                BloodGlucoseMgDl = patient.BloodGlucoseMgDl,
                Smoking = patient.Smoking,
                AlcoholIntake = patient.AlcoholIntake,
                PhysicalActivity = patient.PhysicalActivity,
                FamilyHistory = patient.FamilyHistory
            };

            try
            {
                var prediction = await _aiService.PredictAsync(                    request,                    cancellationToken                );

                if (prediction == null)
                {
                    return new
                    {
                        measurement,
                        ai = (AIPredictionResponse?)null,
                        aiError = "A IA não retornou um resultado."
                    };
                }

                // Recupera as probabilidades
                prediction.Probabilities.TryGetValue("Normal", out double normal);
                prediction.Probabilities.TryGetValue("Stage 1", out double stage1);
                prediction.Probabilities.TryGetValue("Stage 2", out double stage2);

                // Cria a análise vinculada à medição
                var analysis = new AIAnalysis
                {
                    MeasurementId = measurement.Id,
                    Classification = prediction.Classification,
                    NormalProbability = normal,
                    Stage1Probability = stage1,
                    Stage2Probability = stage2,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AIAnalyses.Add(analysis);
                await _context.SaveChangesAsync(cancellationToken);

                return new
                {
                    measurement,
                    ai = prediction,
                    aiError = (string?)null
                };
            }
            catch (HttpRequestException ex)
            {
                return new
                {
                    measurement,
                    ai = (AIPredictionResponse?)null,
                    aiError = "A medição foi salva, mas não foi possível consultar a IA: " + ex.Message
                };
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                return new
                {
                    measurement,
                    ai = (AIPredictionResponse?)null,
                    aiError = "A medição foi salva, mas a IA excedeu o tempo limite: " + ex.Message
                };
            }
        }

        public List<Measurement> GetMeasurements()
        {
            return _context.Measurements                .OrderByDescending(x => x.Timestamp)                .ToList();
        }

        public Measurement? GetMeasurementById(int id)
        {
            return _context.Measurements.FirstOrDefault(x => x.Id == id);
        }

        public async Task<bool> PatientExistsAsync(    int patientId,    CancellationToken cancellationToken = default)
        {
            return await _context.Patients                .AnyAsync(x => x.Id == patientId, cancellationToken);
        }

        public async Task<List<MeasurementHistoryResponse>> GetMeasurementsByPatientAsync(            int patientId,            CancellationToken cancellationToken = default)
        {
            var measurements = await _context.Measurements
                .AsNoTracking()
                .Include(x => x.AIAnalysis)
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken);

            return measurements.Select(MapMeasurementHistory).ToList();
        }

        public async Task<MeasurementHistoryResponse?> GetMeasurementWithAnalysisAsync(            int id,            CancellationToken cancellationToken = default)
        {
            var measurement = await _context.Measurements
                .AsNoTracking()
                .Include(x => x.AIAnalysis)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (measurement == null)
                return null;

            return MapMeasurementHistory(measurement);
        }

        public async Task<MeasurementHistoryResponse?> GetLatestMeasurementByPatientAsync(            int patientId,            CancellationToken cancellationToken = default)
        {
            var measurement = await _context.Measurements
                .AsNoTracking()
                .Include(x => x.AIAnalysis)
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (measurement == null)
                return null;

            return MapMeasurementHistory(measurement);
        }

        private static MeasurementHistoryResponse MapMeasurementHistory(            Measurement measurement)
        {
            return new MeasurementHistoryResponse
            {
                MeasurementId = measurement.Id,
                PatientId = measurement.PatientId,
                DeviceId = measurement.DeviceId,
                Systolic = measurement.Systolic,
                Diastolic = measurement.Diastolic,
                HeartRate = measurement.HeartRate,
                Timestamp = measurement.Timestamp,

                AIAnalysis = measurement.AIAnalysis == null
                    ? null
                    : new AIAnalysisHistoryResponse
                    {
                        Classification = measurement.AIAnalysis.Classification,

                        Probabilities = new Dictionary<string, double>
                        {
                    { "Normal", measurement.AIAnalysis.NormalProbability },
                    { "Stage 1", measurement.AIAnalysis.Stage1Probability },
                    { "Stage 2", measurement.AIAnalysis.Stage2Probability }
                        }
                    }
            };
        }

        public async Task<Measurement?> GetLatestMeasurementForFhirAsync(    int patientId,    CancellationToken cancellationToken = default)
        {
            return await _context.Measurements
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
