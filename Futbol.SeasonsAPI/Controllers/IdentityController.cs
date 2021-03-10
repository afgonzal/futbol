using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Futbol.SeasonsAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly ISecurityService _securityService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(IPartnerService partnerService, ISecurityService securityService, IConfiguration configuration, ILogger<IdentityController> logger)
        {
            _partnerService = partnerService;
            _securityService = securityService;
            _configuration = configuration;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateGoogleToken([FromQuery] string token)
        {
            try
            {
                var googleAuthSection =
                    _configuration.GetSection("Authentication:Google");

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string>() {googleAuthSection["ClientId"]}
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                var user = await _partnerService.GetUserByEmailAsync(payload.Email);
                if (user != null)
                {
                    var myToken = _securityService.GenerateToken(user);
                    return Ok(new ValidateUserResponse {IsAuthorized = true, Token = myToken, Roles = user.Roles});
                }

                return Ok(new ValidateUserResponse {IsAuthorized = false});
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error validationg Google Token", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error validationg Google Token.");
            }
        }
    }
}
