using BusinessLogic.MedicalRecordService;
using DataAccess.DBEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }
        [HttpPost("CreateMedicalRecord")]
        public async Task<ResponseObj<PrescriptionIdModel>> CreateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel)
            => await _medicalRecordService.CreateMedicalRecord(medicalAndPrescriptionModel);
        [HttpPut("UpdateMedicalRecord/{medicalRecordId}")]
        public async Task<ResponseObj<string>> UpdateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel, string medicalRecordId)
            => await _medicalRecordService.UpdateMedicalRecord(medicalAndPrescriptionModel, medicalRecordId);
        
        [HttpGet("GetMedicalRecordsByPatientId/{patientId}")]
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordsByPatientId(string patientId)
            => await _medicalRecordService.GetMedicalRecordsByPatientId(patientId);
        [HttpPost("GetMedicalRecords")]
        public async Task<PagedResponse<MedicalRecordResponseModel>> GetMedicalRecords(PaginationRequest request)
            => await _medicalRecordService.GetMedicalRecords(request);
        [HttpGet("GetMedicalRecordById/{id}")]
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordById(string id)
            => await _medicalRecordService.GetMedicalRecordById(id);
        
        [HttpGet("GetMedicalRecordByIdForPres/{id}")]
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordByIdForPres(string id)
            => await _medicalRecordService.GetMedicalRecordByIdForPres(id);

        [HttpGet("GetMedicalRecordForEdit/{id}")]
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id)
            => await _medicalRecordService.GetMedicalRecordForEdit(id);
        [HttpPost("GetHealthReport/{patientId}")]
        public async Task<PagedResponse<HealthReportViewModel>> GetHealthReport(string patientId, [FromBody] PaginationRequest request)
            => await _medicalRecordService.GetHealthReport(patientId, request);
        [HttpGet("GetMedicalHistory/{patientId}")]
        public async Task<ResponseObj<List<HistoryModel>>> GetMedicalHistory(string patientId)
            => await _medicalRecordService.GetMedicalHistory(patientId);
        [HttpGet("DuplicateRecord/{medicalRecordId}/{appointmentId}")]
        public async Task<ResponseObj<string>> DuplicateRecord(string medicalRecordId, string appointmentId)
            => await _medicalRecordService.DuplicateRecord(medicalRecordId, appointmentId);
        [HttpPost("CreatePatientAllergy")]
        public async Task<ResponseObj<List<PatientAllergy>>> CreatePatientAllergy(PatientAllergy patientAllergy)
            => await _medicalRecordService.CreatePatientAllergy(patientAllergy);
        [HttpDelete("RemovePatientAllergy")]
        public async Task<ResponseObj<List<PatientAllergy>>> RemovePatientAllergy(int allergyId, string patientId)
            => await _medicalRecordService.RemovePatientAllergy(allergyId, patientId);
        [HttpGet("GetAllergiesByPatientId/{patientId}")]
        public async Task<ResponseObj<List<PatientAllergy>>> GetAllergiesByPatientId(string patientId)
            => await _medicalRecordService.GetAllergiesByPatientId(patientId);

    }
}
