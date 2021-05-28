using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProCode.FileHosterRepo.Api
{
    public class Token
    {
        #region Constants
        public const string ClaimTypeNameUserId = "userId";
        #endregion

        #region Fields
        private readonly IJwtAuthenticationManager authenticationManager;
        #endregion

        #region Constructors
        public Token(IJwtAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }
        #endregion

        public string Generate(int userId, string email, Dto.Common.UserRole role)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypeNameUserId, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(
                    authenticationManager.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}

