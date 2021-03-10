using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Futbol.Seasons.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Futbol.Seasons.Services
{
    public interface ISecurityService : ISecurityTokenValidator
    {
        string GenerateToken(User user);
        string GetClaim(string token, string claimType);
        bool ValidateCurrentToken(string token);
      
        bool CanSetResults(ClaimsPrincipal user, string entityName);
    }

    public class SecurityService : ISecurityService
    {
        private readonly IConfiguration _config;

        public SecurityService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var configuration = _config.GetSection("SPF").GetSection("Futbol");


            var claims = user.Roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())).ToList();
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Email));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["TokenSecret"])),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);

            var claimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return claimValue;
        }

        public bool ValidateCurrentToken(string token)
        {
            var configuration = _config.GetSection("SPF").GetSection("Futbol");
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["TokenSecret"]));

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = mySecurityKey
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool IsAdmin(ClaimsPrincipal user, string entityName)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (claim == null)
                return false;
            if (claim.Value == UserRole.Admin.ToString())
                return true;
            return false;
        }

        private bool IsContributorOrAdmin(ClaimsPrincipal user, string entityName)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (claim == null)
                return false;
            if (claim.Value == UserRole.Admin.ToString() || claim.Value == UserRole.Contributor.ToString())
                return true;
            return false;
        }


        public bool CanSetResults(ClaimsPrincipal user, string entityName)
        {
            return IsContributorOrAdmin(user, entityName);
        }

        public bool CanReadToken(string securityToken)
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters,
            out SecurityToken validatedToken)
        {
            throw new NotImplementedException();
        }

        public bool CanValidateToken { get; }
        public int MaximumTokenSizeInBytes { get; set; }
    }
}
