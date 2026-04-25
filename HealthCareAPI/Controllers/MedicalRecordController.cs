using BusinessLogic.MedicalRecordService;
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
        public async Task<ResponseObj<string>> CreateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel)
            => await _medicalRecordService.CreateMedicalRecord(medicalRecordRequestModel);
        [HttpPut("UpdateMedicalRecord/{medicalRecordId}")]
        public async Task<ResponseObj<string>> UpdateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel, string medicalRecordId)
            => await _medicalRecordService.UpdateMedicalRecord(medicalRecordRequestModel, medicalRecordId);
        
        [HttpGet("GetMedicalRecordsByPatientId/{patientId}")]
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordsByPatientId(string patientId)
            => await _medicalRecordService.GetMedicalRecordsByPatientId(patientId);
        [HttpGet("GetMedicalRecords")]
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecords()
            => await _medicalRecordService.GetMedicalRecords();
        [HttpGet("GetMedicalRecordById/{id}")]
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordById(string id)
            => await _medicalRecordService.GetMedicalRecordById(id);
        [HttpGet("GetMedicalRecordForEdit/{id}")]
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id)
            => await _medicalRecordService.GetMedicalRecordForEdit(id);
    }
}
