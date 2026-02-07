using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Services;
using NTTCoreTester.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.BusinessLogic
{
    public interface IAuthManager
    {
        Task<(bool ok, string msg, TestResult test)> SendOtpAndValidate(string uid, string pwd, string scenario);
        Task<(bool ok, string msg, UserSession session, TestResult test)> LoginAndValidate(string uid, string pwd, string otp, string scenario);
        Task<(bool ok, string msg, TestResult test)> CheckSessionAndValidate(string scenario);
        Task<(bool ok, string msg, TestResult test)> LogoutAndValidate(string scenario);
        Task<(bool ok, string msg, TestResult test)> ForgotPwdAndValidate(string uid, string token, string scenario);
        Task<(bool ok, string msg, TestResult test)> ResetPwdAndValidate(string uid, string otp, string newPwd, string scenario);
        UserSession GetSession();
    }

    public class AuthManager : IAuthManager
    {
        private readonly IApiService _api;
        private readonly IValidator _validator;
        private readonly ApiConfiguration _cfg;
        private UserSession _session;
        private string _lastUid; // remember uid for forgot password flow

        public AuthManager(IApiService api, IValidator validator, ApiConfiguration cfg)
        {
            _api = api;
            _validator = validator;
            _cfg = cfg;
        }

        public async Task<(bool ok, string msg, TestResult test)> SendOtpAndValidate(string uid, string pwd, string scenario)
        {
            _lastUid = uid; 

            var req = new SendOtpRequest { uid = uid, pwd = pwd };
            var (res, time, status) = await _api.SendOtp(req);

            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "SendOTP",
                ResponseMs = time,
                HttpCode = status
            };

            
            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            test.ValidJson = techOk;

            
            if (res.StatusCode == 0)
            {
                test.Result = Status.PASS;
                return (true, res.Message, test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = res.Message;
                return (false, res.Message, test);
            }
        }

        public async Task<(bool ok, string msg, UserSession session, TestResult test)> LoginAndValidate(
            string uid, string pwd, string otp, string scenario)
        {
            var req = new LoginRequest
            {
                site = _cfg.Site,
                username = uid,
                uid = uid,
                password = pwd,
                pwd = pwd,
                otp = otp,
                source = "web"
            };

            var (res, time, status) = await _api.Login(req);

            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "Login",
                ResponseMs = time,
                HttpCode = status
            };

            
            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            bool bizOk = _validator.CheckLogin(res, uid, out string bizErr);
            test.ValidJson = techOk && bizOk;

            if (bizOk && res.ResponceDataObject != null)
            {
                
                _session = new UserSession
                {
                    UserId = res.ResponceDataObject.uid,
                    UserName = res.ResponceDataObject.uname,
                    Token = res.ResponceDataObject.susertoken,
                    LoginTime = DateTime.Now,
                    IsActive = true
                };

                _lastUid = uid;
                test.Result = Status.PASS;
                return (true, res.Message, _session, test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = bizErr;
                return (false, bizErr, null, test);
            }
        }

        public async Task<(bool ok, string msg, TestResult test)> CheckSessionAndValidate(string scenario)
        {
            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "CheckLogin"
            };

            if (_session == null || string.IsNullOrEmpty(_session.Token))
            {
                test.Result = Status.FAIL;
                test.Error = "No session";
                return (false, "No active session", test);
            }

            var req = new CheckLoginRequest { sessionkey = _session.Token };
            var (res, time, status) = await _api.CheckLogin(req);

            test.ResponseMs = time;
            test.HttpCode = status;

            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            bool sessOk = _validator.CheckSession(res, out string sessErr);
            test.ValidJson = techOk && sessOk;

            if (sessOk)
            {
                test.Result = Status.PASS;
                return (true, "Session active", test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = sessErr;
                return (false, sessErr, test);
            }
        }

        public async Task<(bool ok, string msg, TestResult test)> LogoutAndValidate(string scenario)
        {
            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "Logout"
            };

            if (_session == null)
            {
                test.Result = Status.SKIP;
                test.Error = "No session";
                return (false, "No session to logout", test);
            }

            var (res, time, status) = await _api.Logout(_session.Token, _session.UserId);

            test.ResponseMs = time;
            test.HttpCode = status;

            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            test.ValidJson = techOk;

            _session = null; 

            test.Result = Status.PASS;
            return (true, "Logged out", test);
        }

        public async Task<(bool ok, string msg, TestResult test)> ForgotPwdAndValidate(
            string uid, string token, string scenario)
        {
            
            string actualUid = string.IsNullOrEmpty(uid) ? _lastUid : uid;

            var req = new ForgotPwdRequest
            {
                site = _cfg.Site,
                loginid = actualUid,
                uid = actualUid,
                logintoken = token
            };

            var (res, time, status) = await _api.ForgotPassword(req);

            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "FgtPwdOTP",
                ResponseMs = time,
                HttpCode = status
            };

            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            test.ValidJson = techOk;

            if (res.StatusCode == 0)
            {
                test.Result = Status.PASS;
                return (true, res.Message, test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = res.Message;
                return (false, res.Message, test);
            }
        }

        public async Task<(bool ok, string msg, TestResult test)> ResetPwdAndValidate(
            string uid, string otp, string newPwd, string scenario)
        {
            
            string actualUid = string.IsNullOrEmpty(uid) ? _lastUid : uid;

            var req = new ResetPwdRequest
            {
                uid = actualUid,
                otp = otp,
                pwd = newPwd
            };

            var (res, time, status) = await _api.ResetPassword(req);

            var test = new TestResult
            {
                Time = DateTime.Now,
                Scenario = scenario,
                Api = "ValOTPStPwd",
                ResponseMs = time,
                HttpCode = status
            };

            bool techOk = _validator.CheckTechnical(status, time, res != null, out string techErr);
            test.ValidJson = techOk;

            if (res.StatusCode == 0)
            {
                test.Result = Status.PASS;
                return (true, res.Message, test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = res.Message;
                return (false, res.Message, test);
            }
        }

        public UserSession GetSession()
        {
            return _session;
        }
    }

}
