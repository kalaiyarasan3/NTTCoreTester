using Newtonsoft.Json.Linq;
using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Common
{
    public interface IEnvelopeValidator
    {
        ValidationResult Validate(JObject json);
    }

    public class EnvelopeValidator : IEnvelopeValidator
    {
        public ValidationResult Validate(JObject json)
        {
            var result = new ValidationResult();

            if (json == null)
            {
                result.AddLevel1Error("Response:", "JSON object is null ");
                return result;
            }

            //validate 9 envelope fields

            CheckStringField(json, "Status", false, result);
            CheckStringField(json, "Message", false, result);
            CheckIntegerField(json, "StatusCode", false, result);
            CheckStringField(json, "RequestID", false, result);
            CheckStringField(json, "Activity", false, result);
            CheckObjectField(json, "ResponceDataObject", false, result);

            // Optional fields (Responce, TypeID, Info) - no validation needed

            // Validate StatusCode range (0-9)
            if (json.ContainsKey("StatusCode") && json["StatusCode"] != null && json["StatusCode"].Type == JTokenType.Integer)
            {
                int code = json["StatusCode"].Value<int>();
                if (code < 0 || code > 9)
                {
                    result.AddLevel1Error("StatusCode", "StatusCode must be 0-9", "0-9", code);
                }
            }

            return result;
        }

        private void CheckStringField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is missing", "present", "missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel1Error(fieldName, $"{fieldName} is null", "string", "null");
                }
                return;
            }

            if (field.Type != JTokenType.String)
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is not a string", "string", field.Type.ToString());
                return;
            }

            string value = field.Value<string>();
            if (!nullable && string.IsNullOrWhiteSpace(value))
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is empty", "non-empty string", "empty");
            }
        }

        private void CheckIntegerField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is missing", "present", "missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel1Error(fieldName, $"{fieldName} is null", "integer", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Integer)
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is not an integer", "integer", field.Type.ToString());
            }
        }

        private void CheckObjectField(JObject json, string fieldName, bool nullable, ValidationResult result)
        {
            if (!json.ContainsKey(fieldName))
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is missing", "present", "missing");
                return;
            }

            JToken field = json[fieldName];

            if (field == null || field.Type == JTokenType.Null)
            {
                if (!nullable)
                {
                    result.AddLevel1Error(fieldName, $"{fieldName} is null", "object", "null");
                }
                return;
            }

            if (field.Type != JTokenType.Object)
            {
                result.AddLevel1Error(fieldName, $"{fieldName} is not an object", "object", field.Type.ToString());
            }
        }
    }
}