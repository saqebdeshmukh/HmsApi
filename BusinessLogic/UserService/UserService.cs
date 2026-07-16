using BusinessLogic.CommonService;
using BusinessLogic.TokenService;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using ViewModels;

namespace BusinessLogic.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _hasher;
        private readonly ITokenService _tokenService;
        private readonly ICommonService _commonService;
        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> hasher, ITokenService tokenService, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _tokenService = tokenService;
            _commonService = commonService;
        }
        public async Task<ResponseObj<string>> CreateUser(UserRequestModel userRequestModel)
        {
            var response = new ResponseObj<string>();
            var ExistUser = await _unitOfWork.UserRepository.GetWithoutTracking().Where(x => x.Email == userRequestModel.Email).FirstOrDefaultAsync();
            if (ExistUser == null) 
            {
                response.IsSuccess = false;
                response.Message = "User with this email already exists.";
                return response;
            }
            User user = new User();
            user.Id = Guid.NewGuid().ToString();
            user.Email = userRequestModel.Email;
            user.CreatedAt = _commonService.GetCurrentIndianTime();
            user.Password = _hasher.HashPassword(user, userRequestModel.Password);
            user.Role = userRequestModel.Role;
            await _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "User created successfully.";
            return response;
        }
        public async Task<ResponseObj<UserResponseModel>> Login(UserLoginRequestModel userLoginRequestModel)
        {
            var response = new ResponseObj<UserResponseModel>();
            var user = await _unitOfWork.UserRepository.GetWithoutTracking().Where(x => x.Email == userLoginRequestModel.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User not found.";
                return response;
            }
            var result = _hasher.VerifyHashedPassword(user, user.Password, userLoginRequestModel.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                response.IsSuccess = false;
                response.Message = "Invalid email or password.";
                return response;
            }
            var tokens = await GenerateTokens(user);
            var userResponse = new UserResponseModel { Id = user.Id, Email = user.Email, Role = user.Role, CreatedAt = user.CreatedAt, AuthResponse = tokens };
            response.IsSuccess = true;
            response.Message = "Login successful.";
            response.Data = userResponse;

            return response;
        }
        public async Task<ResponseObj<AuthResponse>> RefreshTokenAsync(string token)
        {
            ResponseObj<AuthResponse> response = new ResponseObj<AuthResponse>();
            var storedToken = await _unitOfWork.RefreshTokenRepository.GetWithoutTracking().Where(x => x.Token == token).FirstOrDefaultAsync(); ;

            if (storedToken == null || storedToken.IsExpired)
                throw new Exception("Invalid refresh token");
            storedToken.IsRevoked = true;
            response.IsSuccess = true;
            response.Message = "Token refreshed successfully.";
            response.Data = await GenerateTokens(storedToken.User);
            return response;
        }
        public async Task<ResponseObj<string>> Logout(string userId)
        {
            var response = new ResponseObj<string>();
            var tokens = await _unitOfWork.RefreshTokenRepository.GetWithoutTracking().Where(x => x.UserId == userId).ToListAsync(); ;
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _unitOfWork.RefreshTokenRepository.UpdateRange(tokens);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "User logged out successfully.";
            return response;
        }
        private async Task<AuthResponse> GenerateTokens(User user)
        {
            var accessToken = _tokenService.CreateToken(user);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = _commonService.GetCurrentIndianTime().AddDays(7),
                UserId = user.Id
            };

            await _unitOfWork.RefreshTokenRepository.Add(refreshToken);
            await _unitOfWork.Complete();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }
    }
}
