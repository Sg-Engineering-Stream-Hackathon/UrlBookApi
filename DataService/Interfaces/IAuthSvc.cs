using ModelService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Interfaces
{
    public interface IAuthSvc
    {
        Task<TokenResponseModel> Auth(TokenRequestModel model);
        Task<bool> LogoutUserAsync();
        void DeleteAllCookies(IEnumerable<string> cookiesToDelete);
        void DeleteCookie(string name);
    }
}
