using System;
using System.Collections.Generic;
using System.Text;

namespace DataService.Interfaces
{
    public interface ICookieSvc
    {
        void SetCookie(string key, string value, int? expireTime, bool isSecure, bool isHttpOnly);
        void SetCookie(string key, string value, int? expireTime);
        void DeleteCookie(string key);
        void DeleteAllCookies(IEnumerable<string> cookiesToDelete);
        string Get(string key);
        string GetUserIP();
        string GetUserOS();
    }
}
