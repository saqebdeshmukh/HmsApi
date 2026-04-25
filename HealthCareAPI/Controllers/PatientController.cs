using BusinessLogic.PatientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }
        [HttpPost("CreatePatient")]
        public async Task<ResponseObj<string>> CreatePatient(PatientRequestModel patientRequestModel)
           => await _patientService.CreatePatient(patientRequestModel);
        [HttpGet("GetPatients/{doctorId?}")]
        public async Task<ResponseObj<List<PatientViewModel>>> GetPatients(string? doctorId = null)
           => await _patientService.GetPatients(doctorId);
        [HttpPut("UpdatePatient/{patientId}")]
        public async Task<ResponseObj<string>> UpdatePatient(PatientRequestModel patientRequestModel, string patientId)
           => await _patientService.UpdatePatient(patientRequestModel, patientId);
        [HttpGet("GetPatientById/{id}")]
        public async Task<ResponseObj<PatientViewModel>> GetPatientById(string Id)
            => await _patientService.GetPatientById(Id);
    }
}
