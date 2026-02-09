using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models.Auth
{
    public class SendOtpRequest
    {
        public string uid { get; set; }
        public string pwd { get; set; }
    }

    public class LoginRequest
    {
        public string site { get; set; }
        public string username { get; set; }
        public string uid { get; set; }
        public string password { get; set; }
        public string pwd { get; set; }
        public string otp { get; set; }
        public int TwoFA { get; set; }
        public string source { get; set; } = "web";
    }

    public class CheckLoginRequest
    {
        public string sessionkey { get; set; }
    }

    public class ForgotPwdRequest
    {
        public string site { get; set; }
        public string loginid { get; set; }
        public string uid { get; set; }
        public string logintoken { get; set; }
    }

    public class ResetPwdRequest
    {
        public string uid { get; set; }
        public string otp { get; set; }
        public string pwd { get; set; }
    }
}

