using System.Collections.Generic;
using System.Threading.Tasks;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Futbol.SeasonsAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly ISecurityService _securityService;
        private readonly IConfiguration _configuration;

        public IdentityController(IPartnerService partnerService, ISecurityService securityService, IConfiguration configuration)
        {
            _partnerService = partnerService;
            _securityService = securityService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateGoogleToken([FromQuery] string token)
        {
            var googleAuthSection =
                _configuration.GetSection("Authentication:Google");

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string>() { googleAuthSection["ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
            var user = await _partnerService.GetUserByEmailAsync(payload.Email);
            if (user != null)
            {
                var myToken = _securityService.GenerateToken(user);
                return Ok(new ValidateUserResponse { IsAuthorized = true, Token = myToken, Role = user.Role });
            }

            return Ok(new ValidateUserResponse { IsAuthorized = false });
        }
    }
}
