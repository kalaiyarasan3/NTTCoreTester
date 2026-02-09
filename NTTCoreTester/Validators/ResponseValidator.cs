using Newtonsoft.Json.Linq;
using NTTCoreTester.Models.Common;
using NTTCoreTester.Validators.Auth;
using NTTCoreTester.Validators.Common;
using System;

namespace NTTCoreTester.Validators
{
    /// <summary>
    /// Main Response Validator
    /// Orchestrates 3-level validation using JObject (no model classes)
    /// </summary>
    public interface IResponseValidator
    {
        ValidationResult Validate(string jsonResponse, string activity);
    }

    public class ResponseValidator : IResponseValidator
    {
        private readonly IEnvelopeValidator _envelopeValidator;
        private readonly ICommonDataValidator _commonDataValidator;
        private readonly IAuthActivityValidator _authActivityValidator;

        public ResponseValidator(
            IEnvelopeValidator envelopeValidator,
            ICommonDataValidator commonDataValidator,
            IAuthActivityValidator authActivityValidator)
        {
            _envelopeValidator = envelopeValidator;
            _commonDataValidator = commonDataValidator;
            _authActivityValidator = authActivityValidator;
        }

        public ValidationResult Validate(string jsonResponse, string activity)
        {
            var result = new ValidationResult();

            // Parse JSON
            JObject json;
            try
            {
                json = JObject.Parse(jsonResponse);
            }
            catch (Exception ex)
            {
                result.AddLevel1Error("Response", $"Invalid JSON: {ex.Message}");
                return result;
            }

            // Step 1: Validate envelope (Level 1)
            var step1 = _envelopeValidator.Validate(json);
            result.Merge(step1);

            // Get ResponceDataObject
            if (!json.ContainsKey("ResponceDataObject") || json["ResponceDataObject"] == null)
            {
                return result; // Can't continue without data object
            }

            JObject dataObject = json["ResponceDataObject"] as JObject;
            if (dataObject == null)
            {
                result.AddLevel1Error("ResponceDataObject", "ResponceDataObject is not a valid object");
                return result;
            }

            // Step 2: Validate common fields (Level 2)
            var step2 = _commonDataValidator.Validate(dataObject);
            result.Merge(step2);

            // Step 3: Activity-specific validation (Level 3)
            int statusCode = json["StatusCode"]?.Value<int>() ?? -1;

            ValidationResult step3;
            switch (activity)
            {
                case "SendOTP":
                    step3 = _authActivityValidator.ValidateSendOtp(dataObject);
                    result.Merge(step3);
                    break;

                case "Login":
                    step3 = _authActivityValidator.ValidateLogin(dataObject, statusCode);
                    result.Merge(step3);
                    break;

                case "CheckLogin":
                case "Logout":
                case "FgtPwdOTP":
                case "ValOTPStPwd":
                    // No Level 3 validation for these endpoints
                    break;

                default:
                    // Unknown activity - skip Level 3 validation
                    break;
            }

            return result;
        }
    }
}
