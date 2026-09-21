using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class MedicalRecordRequestModel
    {
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string? Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    public class MedicalAndPrescriptionModel
    {
        public MedicalRecordRequestModel medicalRecordRequestModel { get; set; }
        public PrescriptionRequestModel prescriptionRequestModel { get; set; }
    }
    public class MedicalRecordResponseModel
    {
        public string Id { get; set; }
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public string PatientPhone { get; set; }
        public string BloodGroup { get; set; }
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string AppointmentId { get; set; }
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; }
        public string Symptoms { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? Diabetic { get; set; }
        public string? HyperTension { get; set; }
        public string? GeneticFrom { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int PatientAge { get; set; }
        public List<HealthHistoryDto>? HealthHistory { get; set; }
        public List<PrescriptionViewModel> Prescriptions { get; set; }
        public List<GeneticDiseaseViewModel> GeneticDiseases { get; set; }
        public AppointmentViewModel? Appointment { get; set; }
    }
    public class MedicalRecordPrescription
    {
        public string PrescriptionId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public string Type { get; set; }
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
    }
    public class HealthHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? SugarAfterMeal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class HistoryModel
    {
        public string MedicalRecordId { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? Weight { get; set; }
        public string? BloodPressure { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public List<PrescriptionViewModel> Prescriptions { get; set; }
    }
}
