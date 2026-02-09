using NTTCoreTester.Models.Auth;
using NTTCoreTester.Models.Common;
using NTTCoreTester.Validators.Auth;
using NTTCoreTester.Validators.Common;

namespace NTTCoreTester.Validators
{
    /// <summary>
    /// Response Validator - Runs all validations in order
    /// Step 1: Check envelope (Level 1)
    /// Step 2: Check common fields (Level 2)
    /// Step 3: Check status correlation
    /// Step 4: Check activity-specific fields (Level 3)
    /// </summary>
    public interface IResponseValidator
    {
        ValidationResult Validate(ApiResponse<SendOtpData> response);
        ValidationResult Validate(ApiResponse<LoginSuccessData> response);
        ValidationResult Validate(ApiResponse<LoginErrorData> response);
        ValidationResult Validate(ApiResponse<GeneralAuthData> response);
    }

    public class ResponseValidator : IResponseValidator
    {
        private readonly IEnvelopeValidator _envelopeValidator;
        private readonly ICommonDataValidator _commonDataValidator;
        private readonly IStatusCorrelationValidator _statusCorrelationValidator;
        private readonly IAuthActivityValidator _authActivityValidator;

        public ResponseValidator(
            IEnvelopeValidator envelopeValidator,
            ICommonDataValidator commonDataValidator,
            IStatusCorrelationValidator statusCorrelationValidator,
            IAuthActivityValidator authActivityValidator)
        {
            _envelopeValidator = envelopeValidator;
            _commonDataValidator = commonDataValidator;
            _statusCorrelationValidator = statusCorrelationValidator;
            _authActivityValidator = authActivityValidator;
        }

        /// Validate SendOTP response
        public ValidationResult Validate(ApiResponse<SendOtpData> response)
        {
            var result = new ValidationResult();

            // Step 1: Validate envelope
            var step1 = _envelopeValidator.Validate(response);
            result.Merge(step1);

            // If envelope failed, stop here
            if (response?.ResponceDataObject == null)
                return result;

            // Step 2: Validate common fields
            var step2 = _commonDataValidator.Validate(response.ResponceDataObject);
            result.Merge(step2);

            // Step 3: Check status correlation
            var step3 = _statusCorrelationValidator.Validate(response);
            result.Merge(step3);

            // Step 4: Validate SendOTP specific fields
            var step4 = _authActivityValidator.ValidateSendOtp(response.ResponceDataObject);
            result.Merge(step4);

            return result;
        }

        // Validate Login SUCCESS response
        public ValidationResult Validate(ApiResponse<LoginSuccessData> response)
        {
            var result = new ValidationResult();

            // Step 1: Validate envelope
            var step1 = _envelopeValidator.Validate(response);
            result.Merge(step1);

            if (response?.ResponceDataObject == null)
                return result;

            // Step 2: Validate common fields
            var step2 = _commonDataValidator.Validate(response.ResponceDataObject);
            result.Merge(step2);

            // Step 3: Check status correlation
            var step3 = _statusCorrelationValidator.Validate(response);
            result.Merge(step3);

            // Step 4: Validate Login success fields (only if StatusCode = 0)
            if (response.StatusCode == 0)
            {
                var step4 = _authActivityValidator.ValidateLoginSuccess(response.ResponceDataObject);
                result.Merge(step4);
            }

            return result;
        }

        // Validate Login ERROR response
        public ValidationResult Validate(ApiResponse<LoginErrorData> response)
        {
            var result = new ValidationResult();

            // Step 1: Validate envelope
            var step1 = _envelopeValidator.Validate(response);
            result.Merge(step1);

            if (response?.ResponceDataObject == null)
                return result;

            // Step 2: Validate common fields
            var step2 = _commonDataValidator.Validate(response.ResponceDataObject);
            result.Merge(step2);

            // Step 3: Check status correlation
            var step3 = _statusCorrelationValidator.Validate(response);
            result.Merge(step3);

            // Step 4: Just record error fields (don't validate values)
            var step4 = _authActivityValidator.ValidateLoginError(response.ResponceDataObject);
            result.Merge(step4);

            return result;
        }

        /// Validate General Auth responses (CheckLogin, Logout, ForgotPassword, ResetPassword)
        /// No Level 3 validation needed
        public ValidationResult Validate(ApiResponse<GeneralAuthData> response)
        {
            var result = new ValidationResult();

            // Step 1: Validate envelope
            var step1 = _envelopeValidator.Validate(response);
            result.Merge(step1);

            if (response?.ResponceDataObject == null)
                return result;

            // Step 2: Validate common fields
            var step2 = _commonDataValidator.Validate(response.ResponceDataObject);
            result.Merge(step2);

            // Step 3: Check status correlation
            var step3 = _statusCorrelationValidator.Validate(response);
            result.Merge(step3);

            // Step 4: No activity-specific validation for these endpoints

            return result;
        }
    }
}

