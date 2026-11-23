
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using SmartBlood.FHIR;
using SmartBlood.Models;
using SmartBlood.Services;

namespace SmartBlood.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;
        private readonly FhirObservationService _fhirObservationService;
        private readonly MeasurementService _measurementService;
        public PatientsController(PatientService patientService, FhirObservationService fhirObservationService, MeasurementService measurementService)
        {
            _patientService = patientService;
            _fhirObservationService = fhirObservationService;
            _measurementService = measurementService;
        }

        [HttpPost]
        public IActionResult CreatePatient(Patient patient)
        {
            var result = _patientService.CreatePatient(patient);

            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetPatients()
        {
            var patients = _patientService.GetPatients();

            return Ok(patients);
        }

        [HttpGet("{id}")]
        public IActionResult GetPatientById(int id)
        {
            var patient = _patientService.GetPatientById(id);

            if (patient == null)
                return NotFound();

            return Ok(patient);
        }

        [HttpGet("{id}/fhir")]
        public IActionResult GetFhirPatient(int id)
        {
            var patient = _patientService.GetPatientById(id);

            if (patient == null)
                return NotFound(new { error = "Paciente não encontrado." });

            var fhirPatient = _fhirObservationService.CreatePatientResource(patient);
            var serializer = new FhirJsonSerializer();
            var json = serializer.SerializeToString(fhirPatient);

            return Content(json, "application/fhir+json");
        }

        [HttpGet("{id}/fhir/bundle")]
        public async Task<IActionResult> GetFhirPatientBundle(int id)
        {
            var patient = _patientService.GetPatientById(id);

            if (patient == null)
                return NotFound(new { error = "Paciente não encontrado." });

            var measurement = await _measurementService
                .GetLatestMeasurementForFhirAsync(id);

            if (measurement == null)
                return NotFound(new { error = "Nenhuma medição encontrada para este paciente." });

            var bundle = _fhirObservationService.CreatePatientBundle(                patient,                measurement            );

            var serializer = new FhirJsonSerializer();
            var json = serializer.SerializeToString(bundle);

            return Content(json, "application/fhir+json");
        }
    }
}
