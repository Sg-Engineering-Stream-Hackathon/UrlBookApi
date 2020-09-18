using ModelService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataService.Interfaces
{
    public interface IUserSvc
    {
        Task<ResponseObject> RegisterUserAsync(RegisterViewModel model);
        Task<List<ApplicationUser>> GetUsers();

        Task<bool> UpdateProfileAsync(ApplicationUser user);
    }
}
