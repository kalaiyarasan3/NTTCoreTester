using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using NTTCoreTester.Models.Auth;
using NTTCoreTester.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTTCoreTester.Validators
{
    public interface IValidator
    {
        bool CheckTechnical(int httpCode, long responseMs, bool validJson, out string msg);
        bool CheckCriticalErrors(int httpCode, bool validJson, out string msg);  // NEW - only critical errors
        bool CheckLogin(ApiResponse<LoginSuccessData> resp, string expectedUid, out string msg);
        bool CheckSession(ApiResponse<GeneralAuthData> resp, out string msg);
    }

    public class Validator : IValidator
    {
        private readonly ApiConfiguration _cfg;

        public Validator(ApiConfiguration cfg)
        {
            _cfg = cfg;
        }


        public bool CheckTechnical(int httpCode, long responseMs, bool validJson, out string msg)
        {
            var errors = new List<string>();

            if (httpCode != 200)
                errors.Add($"HTTP {httpCode}");

            if (!validJson)
                errors.Add("Invalid JSON");

            if (responseMs > _cfg.MaxResponseTime)
                errors.Add($"Slow response: {responseMs}ms");

            msg = errors.Any() ? string.Join(", ", errors) : "";
            return !errors.Any();
        }

        // Check ONLY critical errors that should stop execution. when ms> max, it's a warning, not critical.
        // Same for non-200 codes that are expected in some scenarios
        public bool CheckCriticalErrors(int httpCode, bool validJson, out string msg)
        {
            var errors = new List<string>();

            if (httpCode != 200)
                errors.Add($"HTTP {httpCode}");

            if (!validJson)
                errors.Add("Invalid JSON");

            msg = errors.Any() ? string.Join(", ", errors) : "";
            return !errors.Any();
        }

        public bool CheckLogin(ApiResponse<LoginSuccessData> resp, string expectedUid, out string msg)
        {
            var errors = new List<string>();

            if (resp.Status != "OK" && resp.Status != "Ok")
                errors.Add($"Bad status: {resp.Status}");

            if (resp.ResponceDataObject == null)
            {
                msg = "No data in response";
                return false;
            }

            if (string.IsNullOrEmpty(resp.ResponceDataObject.susertoken))
                errors.Add("Token missing");

            if (resp.ResponceDataObject.uid != expectedUid)
                errors.Add($"UID mismatch");

            msg = errors.Any() ? string.Join(", ", errors) : "";
            return !errors.Any();
        }

        public bool CheckSession(ApiResponse<GeneralAuthData> resp, out string msg)
        {
            if (resp.Message != "LoggedIn")
            {
                msg = $"Not logged in: {resp.Message}";
                return false;
            }

            msg = "";
            return true;
        }
    }
}
