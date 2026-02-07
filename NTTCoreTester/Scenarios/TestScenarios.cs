using NTTCoreTester.BusinessLogic;
using NTTCoreTester.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Scenarios
{
    public interface ITestScenarios
    {
        Task<bool> RunNormalLogin(string uid, string pwd);
        Task<bool> RunSessionValidation(string uid, string pwd);
        Task<bool> RunForgotPassword(string uid, string token, string newPwd);
    }

    public class TestScenarios : ITestScenarios
    {
        private readonly IAuthManager _auth;
        private readonly ICsvReport _report;

        public TestScenarios(IAuthManager auth, ICsvReport report)
        {
            _auth = auth;
            _report = report;
        }

        public async Task<bool> RunNormalLogin(string uid, string pwd)
        {
            Console.WriteLine("\n=== SCENARIO A: Normal Login ===\n");

            bool allGood = true;

            // step 1 - send otp
            Console.WriteLine("[1] Sending OTP...");
            var otpRes = await _auth.SendOtpAndValidate(uid, pwd, "NormalLogin");
            _report.Add(otpRes.test);

            if (!otpRes.ok)
            {
                Console.WriteLine($" Failed: {otpRes.msg}");
                return false;
            }
            Console.WriteLine($" {otpRes.msg}");

            // step 2 - get otp from user
            Console.Write("\n[2] Enter OTP: ");
            string otp = Console.ReadLine();

            // step 3 - login
            Console.WriteLine("\n[3] Logging in...");
            var loginRes = await _auth.LoginAndValidate(uid, pwd, otp, "NormalLogin");
            _report.Add(loginRes.test);

            if (!loginRes.ok)
            {
                Console.WriteLine($" Failed: {loginRes.msg}");
                return false;
            }
            Console.WriteLine($" Welcome {loginRes.session.UserName}!");

            // step 4 - check session
            Console.WriteLine("\n[4] Checking session...");
            var checkRes = await _auth.CheckSessionAndValidate("NormalLogin");
            _report.Add(checkRes.test);

            if (!checkRes.ok)
            {
                Console.WriteLine($" Failed: {checkRes.msg}");
                allGood = false;
            }
            else
            {
                Console.WriteLine($" {checkRes.msg}");
            }

            // step 5 - logout
            Console.WriteLine("\n[5] Logging out...");
            var logoutRes = await _auth.LogoutAndValidate("NormalLogin");
            _report.Add(logoutRes.test);

            if (!logoutRes.ok)
            {
                Console.WriteLine($" Warning: {logoutRes.msg}");
            }
            else
            {
                Console.WriteLine($" {logoutRes.msg}");
            }

            Console.WriteLine($"\n>>> Scenario A: {(allGood ? "PASSED" : "FAILED")}");
            return allGood;
        }

        public async Task<bool> RunSessionValidation(string uid, string pwd)
        {
            Console.WriteLine("\n=== SCENARIO B: Session Validation ===\n");

            bool allGood = true;

            // login first
            Console.WriteLine("[1] Sending OTP...");
            var otpRes = await _auth.SendOtpAndValidate(uid, pwd, "SessionCheck");
            _report.Add(otpRes.test);

            if (!otpRes.ok)
            {
                Console.WriteLine(" OTP failed");
                return false;
            }

            Console.Write("\n[2] Enter OTP: ");
            string otp = Console.ReadLine();

            Console.WriteLine("\n[3] Logging in...");
            var loginRes = await _auth.LoginAndValidate(uid, pwd, otp, "SessionCheck");
            _report.Add(loginRes.test);

            if (!loginRes.ok)
            {
                Console.WriteLine(" Login failed");
                return false;
            }

            // check session multiple times
            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine($"\n[4.{i}] Session check #{i}...");
                var checkRes = await _auth.CheckSessionAndValidate($"SessionCheck_{i}");
                _report.Add(checkRes.test);

                if (!checkRes.ok)
                {
                    Console.WriteLine($" Check {i} failed");
                    allGood = false;
                }
                else
                {
                    Console.WriteLine($" Still active");
                }

                if (i < 3)
                    await Task.Delay(2000); // wait 2 sec
            }

            // cleanup
            Console.WriteLine("\n[5] Logout...");
            var logoutRes = await _auth.LogoutAndValidate("SessionCheck");
            _report.Add(logoutRes.test);

            Console.WriteLine($"\n>>> Scenario B: {(allGood ? "PASSED" : "FAILED")}");
            return allGood;
        }

        public async Task<bool> RunForgotPassword(string uid, string loginToken, string newPwd)
        {
            Console.WriteLine("\n=== SCENARIO C: Forgot Password ===\n");
           

            bool allGood = true;

     
            Console.WriteLine("\n[1] Requesting password reset OTP...");

            var forgotRes = await _auth.ForgotPwdAndValidate(uid, loginToken ?? "", "ForgotPwd");
            _report.Add(forgotRes.test);

            if (!forgotRes.ok)
            {
                Console.WriteLine($"Failed: {forgotRes.msg}");
                return false;
            }
            Console.WriteLine($"{forgotRes.msg}");

            
            Console.Write("\n[2] Enter OTP received: ");
            string otp = Console.ReadLine();

           
            Console.WriteLine("\n[3] Resetting password...");
            var resetRes = await _auth.ResetPwdAndValidate(uid, otp, newPwd, "ForgotPwd");
            _report.Add(resetRes.test);

            if (!resetRes.ok)
            {
                Console.WriteLine($"Failed: {resetRes.msg}");
                return false;
            }
            Console.WriteLine($"Password changed successfully!");

            
            Console.WriteLine("\n[4] Testing new password...");
            var testOtpRes = await _auth.SendOtpAndValidate(uid, newPwd, "ForgotPwd_Test");
            _report.Add(testOtpRes.test);

            if (!testOtpRes.ok)
            {
                Console.WriteLine("New password not working");
                allGood = false;
            }
            else
            {
                Console.WriteLine("New password works!");

                
                Console.Write("\n[5] Enter OTP to test login: ");
                string testOtp = Console.ReadLine();

                var testLoginRes = await _auth.LoginAndValidate(uid, newPwd, testOtp, "ForgotPwd_Test");
                _report.Add(testLoginRes.test);

                if (testLoginRes.ok)
                {
                    Console.WriteLine(" Login successful with new password!");
                }
            }

            Console.WriteLine($"\n>>> Scenario C: {(allGood ? "PASSED" : "FAILED")}");
            return allGood;
        }

    }
}
