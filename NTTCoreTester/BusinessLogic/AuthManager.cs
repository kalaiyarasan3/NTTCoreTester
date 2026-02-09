using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Services;
using NTTCoreTester.Validators;
using System;
using System.Threading.Tasks;
using NTTCoreTester.Models.Common;
using NTTCoreTester.Models.Auth;

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
        private readonly IResponseValidator _responseValidator;
        private readonly ApiConfiguration _cfg;
        private UserSession _session;
        private string _lastUid;

        public AuthManager(
            IApiService api,
            IValidator validator,
            IResponseValidator responseValidator,
            ApiConfiguration cfg)
        {
            _api = api;
            _validator = validator;
            _responseValidator = responseValidator;
            _cfg = cfg;
        }

        public async Task<(bool ok, string msg, TestResult test)> SendOtpAndValidate(
            string uid, string pwd, string scenario)
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

            // Check critical errors only (HTTP, JSON) - stops execution
            bool criticalOk = _validator.CheckCriticalErrors(status, res != null, out string criticalErr);

            if (!criticalOk)
            {
                test.Result = Status.FAIL;
                test.Error = criticalErr;
                test.ValidJson = false;
                return (false, criticalErr, test);
            }

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            if (!allTechOk)
            {
                Console.WriteLine($"⚠️ Technical issue: {allTechErr}");
            }

            // Complete 3-level validation
            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                test.Result = Status.FAIL;
                test.Error = validation.GetAllErrorsFormatted();
                Console.WriteLine("\n❌ Validation Failed:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
                return (false, validation.GetSummary(), test);
            }

            // Business logic
            if (res.StatusCode == 0)
            {
                
                test.Result = allTechOk ? Status.PASS : Status.FAIL;
                test.Error = allTechOk ? "" : allTechErr;

                Console.WriteLine("✅ SendOTP validation passed!");
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
                TwoFA = 1,
                source = "WEB"
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

            bool criticalOk = _validator.CheckCriticalErrors(status, res != null, out string criticalErr);

            if (!criticalOk)
            {
                test.Result = Status.FAIL;
                test.Error = criticalErr;
                test.ValidJson = false;
                return (false, criticalErr, null, test);
            }

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            if (!allTechOk)
            {
                Console.WriteLine($"⚠️ Technical issue: {allTechErr}");
            }

            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                test.Result = Status.FAIL;
                test.Error = validation.GetAllErrorsFormatted();
                Console.WriteLine("\n❌ Login Validation Failed:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
                return (false, validation.GetSummary(), null, test);
            }

            if (res.StatusCode == 0 && res.ResponceDataObject != null)
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

                test.Result = allTechOk ? Status.PASS : Status.FAIL;
                test.Error = allTechOk ? "" : allTechErr;

                Console.WriteLine("✅ Login validation passed!");
                return (true, res.Message, _session, test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = res.Message;
                return (false, res.Message, null, test);
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

            bool criticalOk = _validator.CheckCriticalErrors(status, res != null, out string criticalErr);

            if (!criticalOk)
            {
                test.Result = Status.FAIL;
                test.Error = criticalErr;
                test.ValidJson = false;
                return (false, criticalErr, test);
            }

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            if (!allTechOk)
            {
                Console.WriteLine($"⚠️ Technical issue: {allTechErr}");
            }

            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                test.Result = Status.FAIL;
                test.Error = validation.GetAllErrorsFormatted();
                Console.WriteLine("\n❌ CheckLogin Validation Failed:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
                return (false, validation.GetSummary(), test);
            }

            if (res.Message == "LoggedIn")
            {
                test.Result = allTechOk ? Status.PASS : Status.FAIL;
                test.Error = allTechOk ? "" : allTechErr;

                Console.WriteLine("✅ CheckLogin validation passed!");
                return (true, "Session active", test);
            }
            else
            {
                test.Result = Status.FAIL;
                test.Error = res.Message;
                return (false, res.Message, test);
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

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                Console.WriteLine("\n⚠️ Logout Validation Issues:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
            }
            else
            {
                Console.WriteLine("✅ Logout validation passed!");
            }

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

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                Console.WriteLine("\n❌ ForgotPassword Validation Failed:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
            }

            if (res.StatusCode == 0)
            {
                test.Result = allTechOk ? Status.PASS : Status.FAIL;
                test.Error = allTechOk ? "" : allTechErr;
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

            bool allTechOk = _validator.CheckTechnical(status, time, res != null, out string allTechErr);
            test.ValidJson = allTechOk;

            var validation = _responseValidator.Validate(res);

            if (!validation.IsValid)
            {
                Console.WriteLine("\n❌ ResetPassword Validation Failed:");
                Console.WriteLine(validation.GetAllErrorsFormatted());
            }

            if (res.StatusCode == 0)
            {
                test.Result = allTechOk ? Status.PASS : Status.FAIL;
                test.Error = allTechOk ? "" : allTechErr;
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
