using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.MedicalRecordService
{
    public interface IMedicalRecordService
    {
        Task<ResponseObj<PrescriptionIdModel>> CreateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel);
        Task<ResponseObj<string>> UpdateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel, string medicalRecordId);
        Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordsByPatientId(string patientId);
        Task<PagedResponse<MedicalRecordResponseModel>> GetMedicalRecords(PaginationRequest request);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordById(string id);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordByIdForPres(string id);
        Task<PagedResponse<HealthReportViewModel>> GetHealthReport(string patientId, PaginationRequest request);
        Task<ResponseObj<List<HistoryModel>>> GetMedicalHistory(string patientId);
        Task<ResponseObj<string>> DuplicateRecord(string medicalRecordId, string appointmentId);
        Task<ResponseObj<List<PatientAllergy>>> CreatePatientAllergy(PatientAllergy patientAllergy);
        Task<ResponseObj<List<PatientAllergy>>> RemovePatientAllergy(int allergyId, string patientId);
        Task<ResponseObj<List<PatientAllergy>>> GetAllergiesByPatientId(string patientId);
    }
}
