using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class TokenService : ITokenService
    {
        private const int FIVE_MINUTES = 300;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        public TokenService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public string CreateTokenForAccount(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var roles = _userManager.GetRolesAsync(user);
            foreach (var role in roles.Result)
            {
                claims.Add(new Claim("role", role));
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("SECRET_KEY") ?? _configuration["jwt:secret"]));

            var credential = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                _configuration.GetValue<string>("SECRET_ISSUER") ?? _configuration["jwt:issuer"],
                _configuration.GetValue<string>("SECRET_AUDIENCE") ?? _configuration["jwt:audience"],
                claims,
                expires: DateTime.UtcNow.AddSeconds(FIVE_MINUTES),
                signingCredentials: credential);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
