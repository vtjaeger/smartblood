using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using SmartBlood.FHIR;
using SmartBlood.Models;
using SmartBlood.Services;

namespace SmartBlood.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeasurementsController : ControllerBase
    {
        private readonly MeasurementService _measurementService;
        private readonly FhirObservationService _fhirObservationService;

        public MeasurementsController(MeasurementService measurementService, FhirObservationService fhirObservationService)
        {
            _measurementService = measurementService;
            _fhirObservationService = fhirObservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeasurement([FromBody] Measurement measurement, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _measurementService.CreateMeasurement(measurement, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetMeasurements()
        {
            var measurements = _measurementService.GetMeasurements();
            return Ok(measurements);
        }

        [HttpGet("patient/{id}")]
        public async Task<IActionResult> GetMeasurementsByPatient(int id, CancellationToken cancellationToken)
        {
            var patientExists = await _measurementService.PatientExistsAsync(id, cancellationToken);

            if (!patientExists)
                return NotFound(new { error = "Paciente não encontrado." });

            var measurements = await _measurementService.GetMeasurementsByPatientAsync(id, cancellationToken);
            return Ok(measurements);
        }

        // Consulta uma medição específica com sua análise
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeasurementById(int id, CancellationToken cancellationToken)
        {
            var measurement = await _measurementService.GetMeasurementWithAnalysisAsync(id, cancellationToken);

            if (measurement == null)
                return NotFound(new { error = "Medição não encontrada." });
            return Ok(measurement);
        }

        // Última medição de um paciente
        [HttpGet("patient/{id}/latest")]
        public async Task<IActionResult> GetLatestMeasurementByPatient(int id, CancellationToken cancellationToken)
        {
            var patientExists = await _measurementService.PatientExistsAsync(id, cancellationToken);

            if (!patientExists)
                return NotFound(new { error = "Paciente não encontrado." });

            var measurement = await _measurementService.GetLatestMeasurementByPatientAsync(id, cancellationToken);

            if (measurement == null)
                return NotFound(new { error = "O paciente ainda não possui medições." });

            return Ok(measurement);
        }

        // Retorna a Observation FHIR da pressão arterial
        [HttpGet("{id}/fhir")]
        public IActionResult GetFhirObservation(int id)
        {
            var measurement = _measurementService.GetMeasurementById(id);

            if (measurement == null)
                return NotFound();

            var observation = _fhirObservationService.CreateBloodPressureObservation(measurement);
            var serializer = new FhirJsonSerializer();
            var json = serializer.SerializeToString(observation);

            return Content(json, "application/fhir+json");
        }

        [HttpGet("{id}/fhir/heart-rate")]
        public IActionResult GetFhirHeartRateObservation(int id)
        {
            var measurement = _measurementService.GetMeasurementById(id);

            if (measurement == null)
                return NotFound();

            var observation = _fhirObservationService.CreateHeartRateObservation(measurement);
            var serializer = new FhirJsonSerializer();
            var json = serializer.SerializeToString(observation);

            return Content(json, "application/fhir+json");
        }
    }
}