using BusinessLogic.DoctorService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        [HttpPost("CreateDoctor")]
        public async Task<ResponseObj<string>> CreateDoctor(DoctorRequestModel doctorRequestModel)
            => await _doctorService.CreateDoctor(doctorRequestModel);
        [HttpGet("GetDoctors")]
        public async Task<ResponseObj<List<DoctorViewModel>>> GetDoctors()
            => await _doctorService.GetDoctors();
        [HttpPut("UpdateDoctor/{doctorId}")]
        public async Task<ResponseObj<string>> UpdateDoctor(DoctorRequestModel doctorRequest, string doctorId)
            => await _doctorService.UpdateDoctor(doctorRequest, doctorId);
        [HttpGet("GetDoctorById/{Id}")]
        public async Task<ResponseObj<DoctorViewModel>> GetDoctorById(string Id)
            => await _doctorService.GetDoctorById(Id);
    }
}
