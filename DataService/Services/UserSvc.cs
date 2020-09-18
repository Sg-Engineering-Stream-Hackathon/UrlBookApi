using DataService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public  class UserSvc : IUserSvc
    {
        private readonly UserManager<ApplicationUser> _userManager;
                
        public UserSvc(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            
        }
        public async Task<ResponseObject> RegisterUserAsync(RegisterViewModel model)
        {
            // Will hold all the errors related to registration
            var errorList = new List<string>();

            ResponseObject responseObject = new ResponseObject();

            try
            {
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserRole = "User",
                    Firstname = model.Firstname,
                    Lastname = model.Lastname,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Firstname,
                    isAdmin = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    var dynamicProperties = new Dictionary<string, object> { ["User"] = user };

                    responseObject.IsValid = true;
                    responseObject.Message = "Success";

                    responseObject.Data = dynamicProperties;
                    return responseObject;
                }

                foreach (var error in result.Errors)
                {
                    errorList.Add(error.Description);
                }
                responseObject.IsValid = false;
                responseObject.Message = "Failed";
                responseObject.Data = errorList;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while registering new user  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return responseObject;
        }

        public async Task<List<ApplicationUser>> GetUsers()
        {
            var appusers = new List<ApplicationUser>();
            foreach (var user in _userManager.Users)
            {
                var appuser = new ApplicationUser();
                appuser.UserName = user.UserName;
                appuser.UserRole = user.UserRole;
                appuser.isAdmin = user.isAdmin;
                appusers.Add(appuser);
            }
            return appusers;
        }

        public async Task<bool> UpdateProfileAsync(ApplicationUser user)
        {
            try
            {
                var usertobeupdated = await _userManager.FindByNameAsync(user.UserName);
                var role = user.isAdmin ? "Administrator" : "User";
                usertobeupdated.UserRole = role;
                usertobeupdated.isAdmin = user.isAdmin;
                await _userManager.UpdateAsync(usertobeupdated);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while updating profile {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }
    }
}
