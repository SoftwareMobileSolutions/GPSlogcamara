using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpslogcamara.Models
{
    public class camaraConfigDataModel
    {
        public string?  appsecret { get; set; } = null;
        public string? sign_method { get; set; } = null;
        public string? app_key { get; set; } = null;
        public string? user_id { get; set; } = null;
        public string? password { get; set; } = null;
        public string? passwordMD5 { get; set; } = null;
        public string? method { get; set; } = null;
        public string? expires_in { get; set; } = null;
        public string? urlrequest { get; set; } = null; 
        public string? accessToken { get; set; } = null;
        public string? account { get; set; } = null;
        public string? expiredDateToken { get; set; } = null;
        public string? fechaactual { get; set; } = null;
        public string? refreshToken { get; set; } = null;
        public int? expiresIn { get; set; } = null;
        public string? time { get; set; } = null;
        public string? appKey { get; set; } = null;
    }
}
