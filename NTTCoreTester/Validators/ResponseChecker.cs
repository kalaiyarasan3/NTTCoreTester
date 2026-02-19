using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
            if (!Directory.Exists(_expectedFolder))
                Directory.CreateDirectory(_expectedFolder);
        }
        public ValidationResult Validate(ApiExecutionResult result)
        {
            var validation = new ValidationResult();
            try
            {

                if (result.StatusCode != 200)
                {
                    validation.IsSuccess = false;
                    validation.Errors.Add($"HTTP Status not 200: {result.StatusCode}");
                    validation.BusinessStatus = Constants.HTTP_FAILED;
                    Console.WriteLine($"HTTP request failed with status code: {result.StatusCode}");
                    return validation;
                }

                if (!result.ResponseBody.TrimStart().StartsWith("{"))
                {
                    validation.IsSuccess = false;
                    validation.BusinessStatus = Constants.NOT_JSON;
                    validation.Errors.Add("Response is not JSON");
                    return validation;
                }

                int businessCode = -1;
                JObject json;

                try
                {
                    json = JObject.Parse(result.ResponseBody);
                }
                catch (JsonReaderException)
                {
                    validation.IsSuccess = false;
                    validation.BusinessStatus = Constants.INVALID_JSON;
                    validation.Errors.Add("Response is not valid JSON");
                    return validation;
                }
                result.Json = json;

                businessCode = json[Constants.StatusCode]?.Value<int>() ?? -1;
                var message = json[Constants.Message]?.Value<string>() ?? "";
                validation.Message = message;

                var status = businessCode.ToBusinessStatus();
                validation.BusinessStatus = status.GetDisplayName();

                if (status != HTTPEnumStatus.Success)
                {
                    validation.IsSuccess = false;
                    validation.Errors.Add($"Business StatusCode: {businessCode}");
                    Console.WriteLine($"Business status: {status} : {message}");
                    return validation;
                }

                validation.IsSchemaValid = Check(result.Endpoint, result.ResponseBody, out var schemaErrors);
                 
                validation.Errors.AddRange(schemaErrors);

                validation.IsSuccess = true;

                return validation;
            }

            catch (Exception ex)
            {
                validation.IsSuccess = false;
                validation.Errors.Add($"Unhandled exception: {ex.Message}");
                validation.BusinessStatus = "EXCEPTION";
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                return validation;
            }
        }


        public bool Check(string endpoint, string responseJson, out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            try
            {
                string filePath = Path.Combine(_expectedFolder, $"{endpoint}_response.json");
                if (!File.Exists(filePath))
                {
                    string error = $"NO FILE: {filePath}";
                    Console.WriteLine($" {error}");
                    validationErrors.Add(error);
                    return false;
                }

                Console.WriteLine(Directory.GetCurrentDirectory() + filePath);

                string expectedJson = File.ReadAllText(filePath);
                using var expectedDoc = JsonDocument.Parse(expectedJson);
                using var actualDoc = JsonDocument.Parse(responseJson);

                ValidateElement(expectedDoc.RootElement, actualDoc.RootElement, endpoint, validationErrors);

                bool hasErrors = validationErrors.Count > 0;

                if (!hasErrors)
                {
                    Console.WriteLine($" {endpoint} STRUCTURE & PATTERN VALIDATION OK ");
                }
                else
                {
                    Console.WriteLine($" {endpoint} VALIDATION FAILED ({validationErrors.Count} error(s))");
                }

                return !hasErrors;
            }
            catch (Exception ex)
            {
                string error = $"ERROR: {ex.Message}";
                Console.WriteLine($" {endpoint} {error}");
                validationErrors.Add(error);
                return false;
            }
        }

        private static void ValidateElement(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            bool expectedIsNull = expected.ValueKind == JsonValueKind.Null || expected.ValueKind == JsonValueKind.Undefined;
            bool actualIsNull = actual.ValueKind == JsonValueKind.Null || actual.ValueKind == JsonValueKind.Undefined;

            if (expectedIsNull) return;

            if (actualIsNull)
            {
                if (expected.ValueKind == JsonValueKind.String)
                {
                    var expStr=expected.GetString() ?? "";

                    //Empty schema string = field is optional / nullable
                    if (string.IsNullOrEmpty(expStr)) 
                        return; // Allow null if template string is empty

                    if (expStr.Contains("||"))
                    {
                        var parts = expStr.Split(new[] { "||" }, 2, StringSplitOptions.None);
                        if (parts.Length == 2 && (parts[1].EndsWith("|null") || parts[1] == "null"))
                            return;
                    }
                }

                string error = $"NULL value at {path} (expected non-null)";
                Console.WriteLine($" {error}");
                errors.Add(error);
                return;
            }
              
            if ((expected.ValueKind == JsonValueKind.True || expected.ValueKind == JsonValueKind.False) &&
                (actual.ValueKind == JsonValueKind.True || actual.ValueKind == JsonValueKind.False))
            {
                return;
            }

            if (expected.ValueKind != actual.ValueKind)
            {
                string error = $"Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}";
                Console.WriteLine($" {error}");
                errors.Add(error);
                return;
            }

            // String validation and  pattern checking
            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedValue = expected.GetString() ?? "";
                var actualValue = actual.GetString() ?? "";

                // Check for pattern syntax
                if (!string.IsNullOrEmpty(expectedValue) && expectedValue.Contains("||"))
                {
                    var parts = expectedValue.Split(new[] { "||" }, 2, StringSplitOptions.None);

                    if (parts.Length == 2)
                    {
                        string pattern = parts[1];

                        // Check if pattern allows null: "pattern|null"
                        bool allowNull = pattern.EndsWith("|null");
                        if (allowNull)
                        {
                            pattern = pattern.Substring(0, pattern.Length - 5);
                            if (string.IsNullOrEmpty(actualValue))
                                return;
                        }

                        try
                        {
                            if (!Regex.IsMatch(actualValue, pattern))
                            {
                                string error = $"Pattern mismatch at {path}: Expected pattern \"{pattern}\", got \"{actualValue}\"";
                                Console.WriteLine($" {error}");
                                errors.Add(error);
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            string error = $"Invalid regex pattern at {path}: {ex.Message}";
                            Console.WriteLine($" {error}");
                            errors.Add(error);
                        }
                    }
                    return;
                }

                // No pattern - just check non-empty
                if (!string.IsNullOrEmpty(expectedValue) && string.IsNullOrEmpty(actualValue))
                {
                    string error = $"Empty string at {path}";
                    Console.WriteLine($" {error}");
                    errors.Add(error);
                }
                return;
            }

            // Number 
            if (expected.ValueKind == JsonValueKind.Number)
            {
                return;
            }

            // Object 
            if (expected.ValueKind == JsonValueKind.Object)
            {
                // Check for __any__ wildcard template
                JsonElement wildcardTemplate = default;
                bool hasWildcard = expected.TryGetProperty("__any__", out wildcardTemplate);

                // First, validate all explicitly defined properties
                foreach (var prop in expected.EnumerateObject())
                {
                    // Skip special markers
                    if (prop.Name == "__any__" || prop.Name == "__pattern__" || prop.Name.EndsWith("||pattern"))
                        continue;

                    if (!actual.TryGetProperty(prop.Name, out var actualProp))
                    {
                        string error = $"Missing field at {path}.{prop.Name}";
                        Console.WriteLine($"{error}");
                        errors.Add(error);
                        continue;
                    }
                    ValidateElement(prop.Value, actualProp, $"{path}.{prop.Name}", errors);
                }

                // If __any__ exists, validate all actual properties 
                if (hasWildcard)
                {
                    foreach (var actualProp in actual.EnumerateObject())
                    {
                        // Skip if this property was already validated as an explicit property
                        if (expected.TryGetProperty(actualProp.Name, out _) &&
                            actualProp.Name != "__any__" &&
                            actualProp.Name != "__pattern__")
                        {
                            continue;
                        }

                        // Validate against wildcard template
                        ValidateElement(wildcardTemplate, actualProp.Value, $"{path}.{actualProp.Name}", errors);
                    }
                }

                return;
            }

            // Array validation 
            if (expected.ValueKind == JsonValueKind.Array)
            {
                if (actual.ValueKind != JsonValueKind.Array)
                {
                    string error = $"Expected array at {path}";
                    Console.WriteLine($" {error}");
                    errors.Add(error);
                    return;
                }

                int expectedLength = expected.GetArrayLength();
                int actualLength = actual.GetArrayLength();

                if (expectedLength == 0)
                    return; // Empty array in template - skip validation

                // Use first element as template
                var template = expected[0];

                if (template.ValueKind == JsonValueKind.String)
                {
                    string templateStr = template.GetString() ?? "";

                    if (templateStr.Contains("||"))
                    {
                        var parts = templateStr.Split(new[] { "||" }, 2, StringSplitOptions.None);

                        if (parts.Length == 2)
                        {
                            string pattern = parts[1];

                            // Validate ALL elements against this pattern
                            int index = 0;
                            foreach (var actualItem in actual.EnumerateArray())
                            {
                                if (actualItem.ValueKind == JsonValueKind.String)
                                {
                                    string actualValue = actualItem.GetString() ?? "";

                                    try
                                    {
                                        if (!Regex.IsMatch(actualValue, pattern))
                                        {
                                            string error = $"Array element pattern mismatch at {path}[{index}]: Expected pattern \"{pattern}\", got \"{actualValue}\"";
                                            Console.WriteLine($" {error}");
                                            errors.Add(error);
                                        }
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        string error = $"Invalid regex pattern at {path}[{index}]: {ex.Message}";
                                        Console.WriteLine($" {error}");
                                        errors.Add(error);
                                    }
                                }
                                index++;
                            }
                            return;
                        }
                    }
                }

                // Validate ALL elements against template structure
                int elementIndex = 0;
                foreach (var actualItem in actual.EnumerateArray())
                {
                    ValidateElement(template, actualItem, $"{path}[{elementIndex}]", errors);
                    elementIndex++;
                }
            }
        }


    }
}
