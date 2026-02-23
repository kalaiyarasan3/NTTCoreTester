using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;
using ValidationResult = NTTCoreTester.Core.Models.ValidationResult;

namespace NTTCoreTester.Validators
{
    public class ResponseChecker
    {
        private readonly string _expectedFolder = "ExpectedResponses";

        public ResponseChecker()
        {
        }
            

        public ValidationResult Validate(ApiExecutionResult result)
        {
            var validation = new ValidationResult();

            try
            {
                if (!IsHttpSuccess(result, validation)) return validation;
                if (!IsJsonResponse(result, validation)) return validation;
                if (!TryParseJson(result, validation)) return validation;
                if (!IsBusinessSuccess(result, validation)) return validation;

                validation.IsSchemaValid = Check(result.Endpoint, result.ResponseBody, out var schemaErrors);
                validation.Errors.AddRange(schemaErrors);
                validation.IsSuccess = true;
            }
            catch (Exception ex)
            {
                validation.IsSuccess = false;
                validation.BusinessStatus = "EXCEPTION";
                validation.Errors.Add($"Unhandled exception: {ex.Message}");
            }

            return validation;
        }

        // ─── Validate() Helpers

        private bool IsHttpSuccess(ApiExecutionResult result, ValidationResult validation)
        {
            if (result.StatusCode == 200) return true;

            validation.IsSuccess = false;
            validation.BusinessStatus = Constants.HTTP_FAILED;
            $"HTTP Status not 200: {result.StatusCode}".Error();
            validation.Errors.Add($"HTTP Status not 200: {result.StatusCode}");
            return false;
        }

        private bool IsJsonResponse(ApiExecutionResult result, ValidationResult validation)
        {
            if (result.ResponseBody.TrimStart().StartsWith("{")) return true;

            validation.IsSuccess = false;
            validation.BusinessStatus = Constants.NOT_JSON;
            validation.Errors.Add("Response is not JSON");
            return false;
        }

        private bool TryParseJson(ApiExecutionResult result, ValidationResult validation)
        {
            try
            {
                result.Json = JObject.Parse(result.ResponseBody);
                return true;
            }
            catch
            {
                validation.IsSuccess = false;
                validation.BusinessStatus = Constants.INVALID_JSON;
                validation.Errors.Add("Response is not valid JSON");
                return false;
            }
        }

        private bool IsBusinessSuccess(ApiExecutionResult result, ValidationResult validation)
        {
            var json = result.Json;
            int businessCode = json[Constants.StatusCode]?.Value<int>() ?? -1;
            string message = json[Constants.Message]?.Value<string>() ?? "";

            var status = businessCode.ToBusinessStatus();
            validation.Message = message;
            validation.BusinessStatus = status.GetDisplayName();

            if (status == HTTPEnumStatus.Success) return true;

            validation.IsSuccess = false;
            validation.Errors.Add($"Business StatusCode: {businessCode}");
            $"Business StatusCode: {businessCode}, Meassage: {message}".Error();
            return false;
        }

        // ─── Schema Check ────────────────────────────────────────────────────────

        public bool Check(string endpoint, string responseJson, out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            try
            {
                string filePath = Path.Combine(_expectedFolder, $"{endpoint}_response.json");

                if (!File.Exists(filePath))
                {
                    validationErrors.Add($"NO FILE: {filePath}");
                    return false;
                }

                using var expectedDoc = JsonDocument.Parse(File.ReadAllText(filePath));
                using var actualDoc = JsonDocument.Parse(responseJson);

                ValidateElement(expectedDoc.RootElement, actualDoc.RootElement, endpoint, validationErrors);

                bool passed = validationErrors.Count == 0;

                if (passed) $"{endpoint} — VALIDATION OK".Debug();
                else $"{endpoint} — VALIDATION FAILED ({validationErrors.Count} error(s))".Error();


                return passed;
            }
            catch (Exception ex)
            {
                validationErrors.Add($"ERROR: {ex.Message}");
                return false;
            }
        }

        // ─── Core Recursive Validator ─────────────────────────────────────────────

        private static void ValidateElement(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            // nothing to validate
            if (IsNullOrUndefined(expected)) return;

            // Actual is null — check if template allows it
            if (IsNullOrUndefined(actual))
            {
                if (!IsNullAllowedByTemplate(expected))
                    errors.Add($"NULL value at {path} (expected non-null)");
                return;
            }

            // Both booleans — just presence check
            if (IsBooleanKind(expected) && IsBooleanKind(actual)) return;

            // Type must match
            if (expected.ValueKind != actual.ValueKind)
            {
                errors.Add($"Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}");
                return;
            }

            switch (expected.ValueKind)
            {
                case JsonValueKind.String: ValidateString(expected, actual, path, errors); break;
                case JsonValueKind.Number: break; // presence is enough
                case JsonValueKind.Object: ValidateObject(expected, actual, path, errors); break;
                case JsonValueKind.Array: ValidateArray(expected, actual, path, errors); break;
            }
        }

        // ─── String Validation 

        private static void ValidateString(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            string expectedValue = expected.GetString() ?? "";
            string actualValue = actual.GetString() ?? "";

            if (!expectedValue.Contains("||"))
            {
                // No pattern — just check actual is not empty when expected is not
                if (!string.IsNullOrEmpty(expectedValue) && string.IsNullOrEmpty(actualValue))
                    errors.Add($"Empty string at {path}");
                return;
            }

            // Has pattern: "example||<regex>" or "example||<regex>|null"
            string pattern = expectedValue.Split(new[] { "||" }, 2, StringSplitOptions.None)[1];

            if (pattern.EndsWith("|null"))
            {
                pattern = pattern[..^5]; // strip "|null"
                if (string.IsNullOrEmpty(actualValue)) return; // null is allowed
            }

            MatchPattern(actualValue, pattern, path, errors);
        }

        private static void MatchPattern(string actualValue, string pattern, string path, List<string> errors)
        {
            try
            {
                if (!Regex.IsMatch(actualValue, pattern))
                    errors.Add($"Pattern mismatch at {path}: Expected \"{pattern}\", got \"{actualValue}\"");
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Invalid regex at {path}: {ex.Message}");
            }
        }

        // Object Validation 
        private static void ValidateObject(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            bool hasWildcard = expected.TryGetProperty("__any__", out var wildcardTemplate);

            // Validate all explicitly defined fields in template
            foreach (var prop in expected.EnumerateObject())
            {
                if (IsSpecialMarker(prop.Name)) continue;

                if (!actual.TryGetProperty(prop.Name, out var actualProp))
                {
                    errors.Add($"Missing field at {path}.{prop.Name}");
                    continue;
                }

                ValidateElement(prop.Value, actualProp, $"{path}.{prop.Name}", errors);
            }

            // Wildcard: validate all actual fields not already covered
            if (!hasWildcard) return;

            foreach (var actualProp in actual.EnumerateObject())
            {
                bool alreadyValidated = expected.TryGetProperty(actualProp.Name, out _)
                                        && !IsSpecialMarker(actualProp.Name);
                if (alreadyValidated) continue;

                ValidateElement(wildcardTemplate, actualProp.Value, $"{path}.{actualProp.Name}", errors);
            }
        }

        // Array Validation 

        private static void ValidateArray(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            if (actual.ValueKind != JsonValueKind.Array)
            {
                errors.Add($"Expected array at {path}");
                return;
            }

            if (expected.GetArrayLength() == 0) return; // empty template = skip

            var template = expected[0];

            // String template with pattern — validate every element against regex
            if (template.ValueKind == JsonValueKind.String)
            {
                string templateStr = template.GetString() ?? "";
                if (templateStr.Contains("||"))
                {
                    string pattern = templateStr.Split(new[] { "||" }, 2, StringSplitOptions.None)[1];
                    int index = 0;
                    foreach (var item in actual.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                            MatchPattern(item.GetString() ?? "", pattern, $"{path}[{index}]", errors);
                        index++;
                    }
                    return;
                }
            }

            // Object template — validate every element against first element's shape
            int elementIndex = 0;
            foreach (var item in actual.EnumerateArray())
            {
                ValidateElement(template, item, $"{path}[{elementIndex}]", errors);
                elementIndex++;
            }
        }

        //  Small Helpers 

        private static bool IsNullOrUndefined(JsonElement el) =>
            el.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;

        private static bool IsBooleanKind(JsonElement el) =>
            el.ValueKind is JsonValueKind.True or JsonValueKind.False;

        private static bool IsSpecialMarker(string name) =>
            name is "__any__" or "__pattern__" || name.EndsWith("||pattern");

        private static bool IsNullAllowedByTemplate(JsonElement expected)
        {
            if (expected.ValueKind != JsonValueKind.String) return false;

            string val = expected.GetString() ?? "";
            if (string.IsNullOrEmpty(val)) return true; // empty string = optional

            if (val.Contains("||"))
            {
                string pattern = val.Split(new[] { "||" }, 2, StringSplitOptions.None)[1];
                return pattern.EndsWith("|null") || pattern == "null";
            }

            return false;
        }
    }
}