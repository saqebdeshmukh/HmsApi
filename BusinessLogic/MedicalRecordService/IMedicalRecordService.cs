using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.MedicalRecordService
{
    public interface IMedicalRecordService
    {
        Task<ResponseObj<string>> CreateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel);
        Task<ResponseObj<string>> UpdateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel, string medicalRecordId);
        Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordsByPatientId(string patientId);
        Task<PagedResponse<MedicalRecordResponseModel>> GetMedicalRecords(PaginationRequest request);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordById(string id);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id);
        Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordByIdForPres(string id);
        Task<PagedResponse<HealthReportViewModel>> GetHealthReport(string patientId, PaginationRequest request);
    }
}
