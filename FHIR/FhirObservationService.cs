using Hl7.Fhir.Model;
using SmartBlood.Models;

namespace SmartBlood.FHIR
{
    public class FhirObservationService
    {
        public Observation CreateBloodPressureObservation(Measurement measurement)
        {
            var observation = new Observation
            {
                Status = ObservationStatus.Final,

                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://loinc.org",
                            Code = "85354-9",
                            Display = "Blood pressure panel"
                        }
                    }
                },

                Subject = new ResourceReference
                {
                    Reference = $"Patient/{measurement.PatientId}"
                },

                Effective = new FhirDateTime(measurement.Timestamp)
            };

            // Pressão sistólica
            observation.Component.Add(new Observation.ComponentComponent
            {
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://loinc.org",
                                Code = "8480-6",
                                Display = "Systolic blood pressure"
                            }
                        }
                },

                Value = new Quantity
                {
                    Value = measurement.Systolic,
                    Unit = "mmHg",
                    System = "http://unitsofmeasure.org",
                    Code = "mm[Hg]"
                }
            }
            );

            // Pressão diastólica
            observation.Component.Add(new Observation.ComponentComponent
            {
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://loinc.org",
                                Code = "8462-4",
                                Display = "Diastolic blood pressure"
                            }
                        }
                },

                Value = new Quantity
                {
                    Value = measurement.Diastolic,
                    Unit = "mmHg",
                    System = "http://unitsofmeasure.org",
                    Code = "mm[Hg]"
                }
            }
            );

            return observation;
        }

        public Observation CreateHeartRateObservation(Measurement measurement)
        {
            var observation = new Observation
            {
                Status = ObservationStatus.Final,

                Code = new CodeableConcept
                {
                    Coding = new List<Coding>            {
                    new Coding
                    {
                        System = "http://loinc.org",
                        Code = "8867-4",
                        Display = "Heart rate"
                    }
                 }
                },

                Subject = new ResourceReference
                {
                    Reference = $"Patient/{measurement.PatientId}"
                },

                Effective = new FhirDateTime(measurement.Timestamp),

                Value = new Quantity
                {
                    Value = measurement.HeartRate,
                    Unit = "beats/minute",
                    System = "http://unitsofmeasure.org",
                    Code = "/min"
                }
            };

            return observation;
        }

        public Hl7.Fhir.Model.Patient CreatePatientResource(    SmartBlood.Models.Patient patient)
        {
            var gender = patient.Gender.ToLower() switch
            {
                "male" or "masculino" => AdministrativeGender.Male,
                "female" or "feminino" => AdministrativeGender.Female,
                "other" or "outro" => AdministrativeGender.Other,
                _ => AdministrativeGender.Unknown
            };

            return new Hl7.Fhir.Model.Patient
            {
                Id = patient.Id.ToString(),
                Gender = gender
            };
        }

        public Bundle CreatePatientBundle(Models.Patient patient, Measurement measurement)
        {
            if (patient.Id != measurement.PatientId)
                throw new ArgumentException("A medição não pertence ao paciente informado.");

            var patientResource = CreatePatientResource(patient);

            var bloodPressureObservation = CreateBloodPressureObservation(measurement);
            var heartRateObservation = CreateHeartRateObservation(measurement);

            var patientFullUrl = $"urn:uuid:{Guid.NewGuid()}";

            bloodPressureObservation.Subject.Reference = patientFullUrl;
            heartRateObservation.Subject.Reference = patientFullUrl;

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Collection
            };

            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = patientFullUrl,
                Resource = patientResource
            });

            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                Resource = bloodPressureObservation
            });

            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                Resource = heartRateObservation
            });

            return bundle;
        }
    }
}