using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelService;

namespace WebApiCatalog.Controllers.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserSvc _userSvc;
        public ProfileController(IUserSvc userSvc)
        {
            _userSvc = userSvc;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userSvc.GetUsers();

            return Ok(result);
            
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateProfile([FromBody] List<ApplicationUser> model)
        {
            bool result = false;

            foreach (var user in model)
            {
                result = await _userSvc.UpdateProfileAsync(user);
            }

            if (result)
            {
                return Ok(new { Message = "Profiles updated Successfully!" });
            }

            return BadRequest(new { Message = "Could not Update Profiles." });
        }
    }
}
