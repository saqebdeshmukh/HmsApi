using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class PatientRequestModel
    {
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? DoctorId { get; set; }
        public string? GeneticFrom { get; set; }
        public bool BookAppointment { get; set; } = false;
        public List<PatientDocumentModel>? Documents { get; set; }
        public List<GeneticDiseaseViewModel>? GeneticDiseases { get; set; }
        public List<string>? DeletedDocumentIds { get; set; }
    }
    public class PatientDocumentModel
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentBase64 { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
    public class PatientViewModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? Diabetic { get; set; }
        public string? Hypertension { get; set; }
        public string? GeneticFrom { get; set; }
        public List<PatientReportDocViewModel>? PatientReportDocs { get; set; }
        public List<GeneticDiseaseViewModel>? GeneticDiseases { get; set; }
    }
    public class PatientDetailsViewModel
    {
        public string? Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Diabetic { get; set; }
        public string? Hypertension { get; set; }
        public string? GeneticFrom { get; set; }
        public int MedicalRecordCount { get; set; }
        public int PrescriptionCount { get; set; }
        public List<AppointmentDto> Appointments { get; set; }
        public List<PatientReportDocViewModel>? PatientReportDocs { get; set; }
    }

    public class PatientReportDocViewModel
    {
        public string? Id { get; set; }
        public string? DocAddress { get; set; }
        public string? DocType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FileName => string.IsNullOrEmpty(DocAddress) ? null : System.IO.Path.GetFileName(DocAddress);
    }
}
