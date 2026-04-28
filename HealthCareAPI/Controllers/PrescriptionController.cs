using BusinessLogic.PrescriptionService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
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
        public async Task<ResponseObj<string>> CreatePrescription([FromBody] PrescriptionRequestModel prescriptionRequestModel)
            => await _prescriptionService.CreatePrescription(prescriptionRequestModel);
        [HttpPut("UpdatePrescription/{PrescriptionId}")]
        public async Task<ResponseObj<string>> UpdatePrescription([FromBody] PrescriptionRequestModel prescriptionRequestModel, string PrescriptionId)
            => await _prescriptionService.UpdatePrescription(prescriptionRequestModel, PrescriptionId);
        [HttpGet("Prescriptions/{doctorId?}")]
        public async Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(string? doctorId)
            => await _prescriptionService.Prescriptions(doctorId);
        [HttpGet("GetPrescriptionById/{id}")]
        public async Task<ResponseObj<PrintViewModel>> GetPrescriptionById(string id)
            => await _prescriptionService.PrescriptionById(id);
    }
}
