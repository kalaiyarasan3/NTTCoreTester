using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Common
{
    //Level 1 Validator: Response Envelope
    // Validates data types and structure of the 9 envelope fields
    public interface IEnvelopeValidator
    {
        ValidationResult Validate<T>(ApiResponse<T> response);
    }

    public class EnvelopeValidator : IEnvelopeValidator
    {
        public ValidationResult Validate<T>(ApiResponse<T> response)
        {
            var result = new ValidationResult();

            if (response == null)
            {
                result.AddLevel1Error("Response", "Response object is null", "ApiResponse<T>", null);
                return result;
            }


            
            ValidateStringType(response.Status, "Status", false, result); // Status - must be string, not null

            
            ValidateStringType(response.Message, "Message", false, result);// Message - must be string, not null

            
            
            ValidateStringType(response.RequestID, "RequestID", false, result);// RequestID - must be string, not null

           
            ValidateStringType(response.Activity, "Activity", false, result); // Activity - must be string, not null

            // ResponceDataObject - must be object, not null
            if (response.ResponceDataObject == null)
            {
                result.AddLevel1Error("ResponceDataObject", "ResponceDataObject is null", "object", null);
            }

            // Responce - can be null (nullable)
            // No validation needed

            // TypeID - can be null (nullable)
            // No validation needed

            // Info - can be null (nullable)
            // No validation needed

            return result;
        }

        private void ValidateStringType(object value, string fieldName, bool nullable, ValidationResult result)
        {
            if (value == null)
            {
                if (!nullable)
                {
                    result.AddLevel1Error(fieldName, $"{fieldName} is null", "string", null);
                }
            }
            else if (!(value is string))
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is not a string", "string", value.GetType().Name);
            }
        }
    }
}
