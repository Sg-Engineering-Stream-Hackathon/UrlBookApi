using System;
using System.Net;
using System.Threading.Tasks;
using DataService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelService;
using Serilog;

namespace WebApiCatalog.Controllers.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[AutoValidateAntiforgeryToken]
    public class AccountController : ControllerBase
    {
        private readonly IUserSvc _userSvc;
        private readonly IAuthSvc _authSvc;
        string[] _cookiesToDelete = { "loginStatus", "access_token", "userRole", "username", "refreshToken" };

        public AccountController(IUserSvc userSvc, IAuthSvc authSvc)
        {
            _userSvc = userSvc;
            _authSvc = authSvc;

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var result = await _userSvc.RegisterUserAsync(model);

            if (result.Message.Equals("Success") && result.IsValid)
            {
                Log.Information($"New User Created => {result.Data["User"].UserName}");

                return Ok(new { username = result.Data["User"].UserName, email = result.Data["User"].Email, status = 1, message = "Registration Successful" });
            }
            return BadRequest(new JsonResult(result.Data));
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Auth([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var jwtToken = await _authSvc.Auth(model);

                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return Unauthorized(new { LoginError = jwtToken.ResponseInfo.Message });
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (jwtToken.ResponseInfo.StatusCode == HttpStatusCode.BadRequest)
                {
                    _authSvc.DeleteAllCookies(_cookiesToDelete);
                    return BadRequest(new { LoginError = jwtToken.ResponseInfo.Message });
                }

                return Ok(jwtToken);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return Unauthorized();

        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authSvc.LogoutUserAsync();
            if (result)
            {
                return Ok(new { Message = "Logout Successful" });
            }

            return BadRequest(new { Message = "Invalid Request" });
        }
    }
}
