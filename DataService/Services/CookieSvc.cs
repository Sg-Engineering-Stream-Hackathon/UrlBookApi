using DataService.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DataService.Services
{
    public class CookieSvc : ICookieSvc
    {
        private readonly CookieOptions _cookieOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieSvc(CookieOptions cookieOptions, IHttpContextAccessor httpContextAccessor)
        {
            _cookieOptions = cookieOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Get(string key)
        {
            return _httpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public void SetCookie(string key, string value, int? expireTime, bool isSecure, bool isHttpOnly)
        {
            _cookieOptions.Expires = expireTime.HasValue ? DateTime.Now.AddMinutes(expireTime.Value) : DateTime.Now.AddMilliseconds(10);
            _cookieOptions.Secure = isSecure;
            _cookieOptions.HttpOnly = isHttpOnly;
            _cookieOptions.IsEssential = true;
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, _cookieOptions);
        }

        public void SetCookie(string key, string value, int? expireTime)
        {
            if (expireTime.HasValue)
            {
                _cookieOptions.Secure = true;
                _cookieOptions.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            }
            else
            {
                _cookieOptions.Secure = true;
                _cookieOptions.Expires = DateTime.Now.AddMilliseconds(10);
            }
            _cookieOptions.HttpOnly = true;
            _cookieOptions.SameSite = SameSiteMode.Strict;
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, _cookieOptions);
        }

        public void DeleteCookie(string key)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
        }

        public void DeleteAllCookies(IEnumerable<string> cookiesToDelete)
        {
            foreach (var key in cookiesToDelete)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
            }
        }

        public string GetUserIP()
        {
            string userIp = "unknown";

            try
            {
                userIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userIp;
        }

        public string GetUserOS()
        {
            string userOs = "unknown";
            try
            {
                userOs = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return userOs;
        }
    }
}
