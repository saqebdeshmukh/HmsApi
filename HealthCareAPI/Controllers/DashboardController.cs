using BusinessLogic.DashboardService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("AdminDashboard")]
        public async Task<ResponseObj<DashboardViewModel>> AdminDashboard()
            => await _dashboardService.AdminDashboard();
        [Authorize(Roles = "Doctor")]
        [HttpGet("DoctorDashboard")]
        public async Task<ResponseObj<DoctorDashboardModel>> DoctorDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _dashboardService.GetDoctorDashboard(userId);
        }
        [Authorize(Roles = "Receptionist")]
        [HttpGet("ReceptionistDashboard")]
        public async Task<ResponseObj<ReceptionistDashboardModel>> ReceptionistDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _dashboardService.GetReceptionistDashboard(userId);
        }

    }
}
