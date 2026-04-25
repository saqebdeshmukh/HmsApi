using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.UserService
{
    public interface IUserService
    {
        Task<ResponseObj<string>> CreateUser(UserRequestModel userRequestModel);
        Task<ResponseObj<UserResponseModel>> Login(UserLoginRequestModel userLoginRequestModel);
        Task<ResponseObj<AuthResponse>> RefreshTokenAsync(string token);
        Task<ResponseObj<string>> Logout(string userId);
    }
}
