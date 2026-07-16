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
        [HttpPost("GetPatients")]
        public async Task<PagedResponse<PatientViewModel>> GetPatients(PaginationRequest request)
           => await _patientService.GetPatients(request);
        [HttpPut("UpdatePatient/{patientId}")]
        public async Task<ResponseObj<string>> UpdatePatient(PatientRequestModel patientRequestModel, string patientId)
           => await _patientService.UpdatePatient(patientRequestModel, patientId);
        [HttpGet("GetPatientById/{id}")]
        public async Task<ResponseObj<PatientViewModel>> GetPatientById(string Id)
            => await _patientService.GetPatientById(Id);
        [HttpGet("PatientDetails/{id}")]
        public async Task<ResponseObj<PatientDetailsViewModel>> PatientDetails(string Id)
            =>await _patientService.PatientDetails(Id);
    }
}
