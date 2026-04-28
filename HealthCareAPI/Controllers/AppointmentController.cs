using BusinessLogic.AppointmentService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [HttpPost("CreateAppointment")]
        public async Task<ResponseObj<string>> CreateAppointment(AppointmentRequestModel appointmentRequestModel)
            => await _appointmentService.CreateAppointment(appointmentRequestModel);
        [HttpGet("GetAppointments")]
        public async Task<ResponseObj<DoctorDetailAppointmentsViewModel>> GetAppointments()
            => await _appointmentService.GetAppointments();
        [HttpGet("AppointmentDetails/{id}")]
        public async Task<ResponseObj<AppointmentDetailsViewModel>> AppointmentDetails(string id)
            => await _appointmentService.AppointmentDetails(id);
        [HttpPut("UpdateAppointmentStatus/{appointmentId}/{status}")]
        public async Task<ResponseObj<string>> UpdateAppointmentStatus(string appointmentId, string status)
            => await _appointmentService.UpdateAppointmentStatus(appointmentId, status);
        [HttpDelete("DeleteAppointment/{appointmentId}")]
        public async Task<ResponseObj<string>> DeleteAppointment(string appointmentId)
            => await _appointmentService.DeleteAppointment(appointmentId);
        [HttpGet("Calendar/{Month}/{Year}")]
        public async Task<ResponseObj<List<AppointmentViewModel>>> Calendar(int? Month, int? Year)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return await _appointmentService.Calendar(Month, Year, userId, role);
        }
    }
}
