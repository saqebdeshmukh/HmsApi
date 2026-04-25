using BusinessLogic.ReceptionistService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReceptionistController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;
        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }
        [HttpPost("CreateReceptionist")]
        public async Task<ResponseObj<string>> CreateReceptionist(ReceptionistRequestModel receptionstRequestModel)
            => await _receptionistService.CreateReceptionist(receptionstRequestModel);
        [HttpGet("Receptionists")]
        public async Task<ResponseObj<List<ReceptionistViewModel>>> Receptionists()
            => await _receptionistService.Receptionists();
    }
}
