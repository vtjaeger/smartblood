using SmartBlood.Data;
using SmartBlood.Models;

namespace SmartBlood.Services
{
    public class PatientService
    {
        private readonly SmartBloodDbContext _context;

        public PatientService(SmartBloodDbContext context)
        {
            _context = context;
        }

        public Patient CreatePatient(Patient patient)
        {
            _context.Patients.Add(patient);
            _context.SaveChanges();

            return patient;
        }

        public List<Patient> GetPatients()
        {
            return _context.Patients                .OrderBy(p => p.Id)                .ToList();
        }

        public Patient? GetPatientById(int id)
        {
            return _context.Patients                .FirstOrDefault(p => p.Id == id);
        }
    }
}