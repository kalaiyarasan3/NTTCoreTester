using Newtonsoft.Json.Linq;
using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Common
{
    /// <summary>
    /// Level 2 Validator: Common ResponceDataObject Fields
    /// Validates data types and structure of the 11 common fields
    /// </summary>
    public interface ICommonDataValidator
    {
        ValidationResult Validate(JObject dataObject);
    }

    public class CommonDataValidator : ICommonDataValidator
    {
        public ValidationResult Validate(JObject dataObject)
        {
            var result = new ValidationResult();

            if (dataObject == null)
            {
                result.AddLevel2Error("ResponceDataObject", "Data object is null");
                return result;
            }

            // Validate 11 common fields
            CheckStringField(dataObject, "request_time", false, result);
            CheckStringField(dataObject, "status", false, result);
            CheckStringField(dataObject, "Message", false, result);
            CheckIntegerField(dataObject, "Result", false, result);

            // Data object validation
            if (!dataObject.ContainsKey("Data"))
            {
                result.AddLevel2Error("Data", "Data object is missing");
            }
            else if (dataObject["Data"] != null && dataObject["Data"].Type == JTokenType.Object)
            {
                JObject data = (JObject)dataObject["Data"];
                CheckStringField(data, "TimeTaken", true, result, "Data.TimeTaken");
            }
            else if (dataObject["Data"] != null && dataObject["Data"].Type != JTokenType.Null)
            {
                result.AddLevel2Error("Data", "Data is not an object", "object", dataObject["Data"].Type.ToString());
            }

            // Optional fields (OSId, TypeID, Info, ModelId, CTA, Action) - no validation needed

            return result;
        }

        private void CheckStringField(JObject json, string fieldName, bool nullable,
            ValidationResult result, string displayName = null)
        {
            string display = displayName ?? fieldName;

            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel2Error(display, $"{display} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel2Error(display, $"{display} is null", "string", "null");
                }
                return;
            }

            if (field.Type != JTokenType.String)
            {
                result.AddLevel2Error(display, $"{display} is not a string", "string", field.Type.ToString());
                return;
            }

            string value = field.Value<string>();
            if (!nullable && string.IsNullOrWhiteSpace(value))
            {
                result.AddLevel2Error(display, $"{display} is empty", "non-empty string", "empty");
            }
        }

        private void CheckIntegerField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel2Error(fieldName, $"{fieldName} is missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel2Error(fieldName, $"{fieldName} is null", "integer", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Integer)
            {
                result.AddLevel2Error(fieldName, $"{fieldName} is not an integer", "integer", field.Type.ToString());
            }
        }
    }
}
