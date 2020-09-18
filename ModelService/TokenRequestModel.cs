using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService
{
    public class TokenRequestModel
    {
        // password or refresh_token    
        public string GrantType { get; set; }
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public string Password { get; set; }
    }
}
