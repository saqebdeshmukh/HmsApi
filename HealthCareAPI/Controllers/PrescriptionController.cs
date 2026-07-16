using BusinessLogic.PrescriptionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        [HttpPost("CreatePrescription")]
        public async Task<ResponseObj<PrescriptionIdModel>> CreatePrescription([FromBody] PrescriptionRequestModel prescriptionRequestModel)
            => await _prescriptionService.CreatePrescription(prescriptionRequestModel);
        [HttpPut("UpdatePrescription/{PrescriptionId}")]
        public async Task<ResponseObj<string>> UpdatePrescription([FromBody] PrescriptionRequest prescriptionRequestModel, string PrescriptionId)
            => await _prescriptionService.UpdatePrescription(prescriptionRequestModel, PrescriptionId);
        [HttpGet("Prescriptions/{doctorId?}")]
        public async Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(PrescriptionQueryParameters? query = null)
            => await _prescriptionService.Prescriptions(query);
        [HttpPost("InternalPrescriptions")]
        public async Task<PagedResponse<PrescriptionViewModel>> InternalPrescriptions(PaginationRequest request)
            => await _prescriptionService.InternalPrescriptions(request, null);
        [HttpPost("ExternalPrescriptions")]
        public async Task<PagedResponse<PrescriptionViewModel>> ExternalPrescriptions(PaginationRequest request)
            => await _prescriptionService.ExternalPrescriptions(request, null);
        [HttpGet("GetPrescriptionsByPatientId/{patientId}")]
        public async Task<ResponseObj<List<PrintViewModel>>> GetPrescriptionsByPatientId(string patientId)
            => await _prescriptionService.GetPrescriptionsByPatientId(patientId);
        [HttpGet("GetPrescriptionById/{id}")]
        public async Task<ResponseObj<PrintViewModel>> GetPrescriptionById(string id)
            => await _prescriptionService.PrescriptionById(id);
        [HttpGet("GetPrescriptionByIdForPrint/{id}")]
        public async Task<ResponseObj<PrintViewModel>> PrescriptionByIdForPrint(string id)
            => await _prescriptionService.PrescriptionByIdForPrint(id);
    }
}
