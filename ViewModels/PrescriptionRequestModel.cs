using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class PrescriptionRequest
    {
        public string? MedicalRecordId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string Type { get; set; } = "MedicalStore";
        public DateTime? AppointmentDate { get; set; }
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
    }
    public class PrescriptionRequestModel
    {
        public PrescriptionRequest? InternalPrescriptionRequest { get; set; }
        public PrescriptionRequest? ExternalPrescriptionRequest { get; set; }
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
        public string AppointmentId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime CreatedAt {  get; set; }
        public DateTime? NextAppointment {  get; set; }
        public string PatientPhoneNo { get; set; }
        public string Instructions { get; set; }
        public string Diagnosis { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Type { get; set; }
        public string GeneticFrom { get; set; }
        public List<PrescriptionItemRequest> prescriptionItemRequests { get; set; }
        public List<GeneticDiseaseViewModel> GeneticDiseases { get; set; }
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
    public class PrescriptionIdModel
    {
        public string InternalId { get; set; }
        public string ExternalId { get; set; }
    }

    public class PrescriptionQueryParameters
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "PrescriptionDate";
        public string SortDir { get; set; } = "desc"; // "asc" or "desc"
        private int _pageNumber = 1;
        private int _pageSize = 20;
        public int PageNumber { get => _pageNumber < 1 ? 1 : _pageNumber; set => _pageNumber = value; }
        public int PageSize { get => _pageSize < 1 ? 20 : (_pageSize > 100 ? 100 : _pageSize); set => _pageSize = value; }
    }
}
