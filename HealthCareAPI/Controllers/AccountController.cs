using BusinessLogic.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("Register")]
        public async Task<ResponseObj<string>> Register(UserRequestModel userRequestModel)
            =>  await _userService.CreateUser(userRequestModel);
        [HttpPost("Login")]
        public async Task<ResponseObj<UserResponseModel>> Login(UserLoginRequestModel userLoginRequestModel)
            => await _userService.Login(userLoginRequestModel);
        [HttpPost("RefreshToken")]
        public async Task<ResponseObj<AuthResponse>> RefreshToken(string token)
            => await _userService.RefreshTokenAsync(token);
        [HttpPost("Logout")]
        public async Task<ResponseObj<string>> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _userService.Logout(userId);
        }
    }
}
