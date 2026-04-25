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
        Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecords();
        Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordById(string id);
       Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id);
    }
}
