using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;
using System;
using System.Threading.Tasks;

namespace FunctionalService
{
    public class FunctionalSvc : IFunctionalSvc
    {
        private readonly AppUserOptions _appUserOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _env;

        public FunctionalSvc(IOptions<AppUserOptions> appUserOptions,
            UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _appUserOptions = appUserOptions.Value;
            _userManager = userManager;
            _env = env;
        }

        public async Task CreateDefaultUser(string role)
        {
            try
            {
                bool isAdmin = role == "Administrator" ? true : false;
                var appUser = new ApplicationUser
                {
                    Email = _appUserOptions.Email,
                    UserName = _appUserOptions.Username,
                    Firstname = _appUserOptions.Firstname,
                    Lastname = _appUserOptions.Lastname,
                    UserRole = role,
                    isAdmin = isAdmin
                 };

                var result = await _userManager.CreateAsync(appUser, _appUserOptions.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, role);
                    Log.Information("App User Created {UserName}", appUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
        
    }
}
