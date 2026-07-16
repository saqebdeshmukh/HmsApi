using BusinessLogic.CommonService;
using DataAccess.DBEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLogic.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ICommonService _commonService;
        public TokenService(IConfiguration config, ICommonService commonService)
        {
            _config = config;
            _commonService = commonService;
        }
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim("id", user.Id),
        new Claim("email", user.Email),
        new Claim("role", user.Role),
        new Claim(ClaimTypes.Role, user.Role),
        // Optional standard claims
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: _commonService.GetCurrentIndianTime().AddMinutes(
                    Convert.ToDouble(_config["Jwt:DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
