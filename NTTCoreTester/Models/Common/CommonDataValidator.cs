using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Common
{
    /// <summary>
    /// Level 2 Validator: Common ResponceDataObject Fields
    /// Validates data types and structure of the 11 common fields
    /// </summary>
    public interface ICommonDataValidator
    {
        ValidationResult Validate(ResponceDataObjectBase data);
    }

    public class CommonDataValidator : ICommonDataValidator
    {
        public ValidationResult Validate(ResponceDataObjectBase data)
        {
            var result = new ValidationResult();

            if (data == null)
            {
                result.AddLevel2Error("ResponceDataObject", "ResponceDataObject is null", "ResponceDataObjectBase", null);
                return result;
            }

            // Validate data types and structure

            // request_time - must be string, not null
            ValidateStringType(data.request_time, "request_time", false, result);

            // status - must be string, not null
            ValidateStringType(data.status, "status", false, result);

            // Message - must be string, not null
            ValidateStringType(data.Message, "Message", false, result);

            // Result - must be number (int) - already enforced by C# type system
            // No additional validation needed

            // Data - must be object, not null
            if (data.Data == null)
            {
                result.AddLevel2Error("Data", "Data object is null", "object", null);
            }
            else
            {
                // Validate Data.TimeTaken - must be string
                ValidateStringType(data.Data.TimeTaken, "Data.TimeTaken", true, result);

                // Validate Data.APITimeTaken - can be null (for non-auth APIs)
                ValidateStringType(data.Data.APITimeTaken, "Data.APITimeTaken", true, result);
            }

            // OSId - nullable, can be any type
            // No validation needed

            // TypeID - nullable, can be any type
            // No validation needed

            // Info - nullable, can be any type
            // No validation needed

            // ModelId - nullable, can be any type
            // No validation needed

            // CTA - nullable, can be any type
            // No validation needed

            // Action - nullable, can be any type
            // No validation needed

            return result;
        }

        private void ValidateStringType(object value, string fieldName, bool nullable, ValidationResult result)
        {
            if (value == null)
            {
                if (!nullable)
                {
                    result.AddLevel2Error(fieldName, $"{fieldName} is null", "string", null);
                }
            }
            else if (!(value is string))
            {
                result.AddLevel2Error(fieldName, $"{fieldName} is not a string", "string", value.GetType().Name);
            }
        }
    }
}
