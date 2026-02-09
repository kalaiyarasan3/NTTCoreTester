using Newtonsoft.Json.Linq;
using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Auth
{
    /// <summary>
    /// Level 3 Validator: Auth Activity-Specific Fields
    /// Validates fields unique to each Auth endpoint
    /// </summary>
    public interface IAuthActivityValidator
    {
        ValidationResult ValidateSendOtp(JObject dataObject);
        ValidationResult ValidateLogin(JObject dataObject, int statusCode);
    }

    public class AuthActivityValidator : IAuthActivityValidator
    {
        /// <summary>
        /// Validate SendOTP (3 fields)
        /// </summary>
        public ValidationResult ValidateSendOtp(JObject dataObject)
        {
            var result = new ValidationResult();

            if (dataObject == null)
            {
                result.AddLevel3Error("ResponceDataObject", "Data object is null");
                return result;
            }

            CheckStringField(dataObject, "DealerUCC", false, result);
            CheckIntegerField(dataObject, "DealerStatus", false, result);
            CheckIntegerField(dataObject, "ClientType", false, result);

            return result;
        }

        /// <summary>
        /// Validate Login (17 fields if StatusCode = 0)
        /// </summary>
        public ValidationResult ValidateLogin(JObject dataObject, int statusCode)
        {
            var result = new ValidationResult();

            if (dataObject == null)
            {
                result.AddLevel3Error("ResponceDataObject", "Data object is null");
                return result;
            }

            // Only validate success fields if StatusCode = 0
            if (statusCode != 0)
            {
                return result; // Skip for error responses
            }

            // String fields (required)
            CheckStringField(dataObject, "susertoken", false, result);
            CheckStringField(dataObject, "uname", false, result);
            CheckStringField(dataObject, "uid", false, result);

            // Integer fields (required)
            CheckIntegerField(dataObject, "TOTPEnabled", false, result);
            CheckIntegerField(dataObject, "IsTOTPSkip", false, result);
            CheckIntegerField(dataObject, "DealerStatus", false, result);
            CheckIntegerField(dataObject, "ClientType", false, result);

            // Array fields (required)
            CheckArrayField(dataObject, "prarr", false, result);
            CheckArrayField(dataObject, "access_type", false, result);
            CheckArrayField(dataObject, "orarr", false, result);

            // Object fields (required)
            CheckObjectField(dataObject, "values", false, result);
            CheckObjectField(dataObject, "mws", false, result);

            // Nullable fields
            CheckStringField(dataObject, "email", true, result);
            CheckArrayField(dataObject, "KraStatus", true, result);
            CheckArrayField(dataObject, "Clients", true, result);

            // AuthorizedActivity can be any type - no validation

            return result;
        }

        private void CheckStringField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "string", "null");
                }
                return;
            }

            if (field.Type != JTokenType.String)
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is not a string", "string", field.Type.ToString());
                return;
            }

            string value = field.Value<string>();
            if (!nullable && string.IsNullOrWhiteSpace(value))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is empty", "non-empty string", "empty");
            }
        }

        private void CheckIntegerField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "integer", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Integer)
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is not an integer", "integer", field.Type.ToString());
            }
        }

        private void CheckArrayField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "array", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Array)
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is not an array", "array", field.Type.ToString());
            }
        }

        private void CheckObjectField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel3Error(fieldName, $"{fieldName} is null", "object", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Object)
            {
                result.AddLevel3Error(fieldName, $"{fieldName} is not an object", "object", field.Type.ToString());
            }
        }
    }
}
