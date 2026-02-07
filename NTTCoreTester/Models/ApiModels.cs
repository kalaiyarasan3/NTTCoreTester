using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
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

    // Generic response wrapper
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string RequestID { get; set; }
        public T ResponceDataObject { get; set; } 
    }

    public class LoginData
    {
        public string susertoken { get; set; }
        public string uname { get; set; }
        public string uid { get; set; }
        public string email { get; set; }
    }

    public class GeneralData
    {
        public string requesttime { get; set; }
        public string status { get; set; }
        public string Message { get; set; }
    }
}
