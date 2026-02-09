using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTTCoreTester.Validators
{
    public interface IValidator
    {
        bool CheckTechnical(int httpCode, long responseMs, bool validJson, out string msg);
        bool CheckCriticalErrors(int httpCode, bool validJson, out string msg);
    }

    public class Validator : IValidator
    {
        private readonly ApiConfiguration _cfg;

        public Validator(ApiConfiguration cfg)
        {
            _cfg = cfg;
        }

        /// <summary>
        /// Check all technical issues (HTTP, JSON, timing) - for recording
        /// </summary>
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

        /// <summary>
        /// Check ONLY critical errors that should stop execution
        /// </summary>
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
    }
}
