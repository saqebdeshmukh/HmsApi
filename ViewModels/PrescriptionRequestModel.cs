using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class PrescriptionRequestModel
    {
        public string? MedicalRecordId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string Type { get; set; } = "MedicalStore";
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
    }
    public class PrescriptionItemRequest
    {
        public string? Id { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string MealTiming { get; set; }
        public string Duration { get; set; } = string.Empty;

    }
    public class PrescriptionViewModel
    {
        public string Id {  get; set; }
        public string? MedicalRecordId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName {  get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string Type { get; set; } = "MedicalStore";
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
    }
    public class PrintViewModel
    {
        public string Id { get; set; }
        public string PatientName { get; set; }
        public string? MedicalRecordId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;

        public string PatientPhoneNo { get; set; }
        public string Instructions { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Type { get; set; }
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
        public DateTime PrescriptionDate { get; set; }
    }
    public class PrescriptionDetailsViewModel
    {
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
        public DateTime VisitDate { get; set; }
        public string Id { get; set; }
        public string Diagnosis { get; set; }
        public string Instructions { get; set; }
        public string MedicalRecordId { get; set; }
        public string PatientName { get; set; }
         public string DoctorName { get; set; }
         public string Specialization { get; set; }
    }
}
