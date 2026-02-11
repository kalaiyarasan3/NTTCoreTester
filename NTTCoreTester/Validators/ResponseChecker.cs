using System.IO;
using System.Text.Json;

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

        public bool Check(string endpoint, string responseJson, out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            try
            {
                string filePath = Path.Combine(_expectedFolder, $"{endpoint}_response.json");
                if (!File.Exists(filePath))
                {
                    string error = $"NO FILE: {filePath}";
                    Console.WriteLine($"❌ {error}");
                    validationErrors.Add(error);
                    return false;
                }

                string expectedJson = File.ReadAllText(filePath);
                using var expectedDoc = JsonDocument.Parse(expectedJson);
                using var actualDoc = JsonDocument.Parse(responseJson);

                ValidateElement(expectedDoc.RootElement, actualDoc.RootElement, endpoint, validationErrors);

                bool hasErrors = validationErrors.Count > 0;
                if (!hasErrors)
                    Console.WriteLine($"✅ {endpoint} STRUCTURE OK ✓");

                return !hasErrors;
            }
            catch (Exception ex)
            {
                string error = $"ERROR: {ex.Message}";
                Console.WriteLine($"❌ {endpoint} {error}");
                validationErrors.Add(error);
                return false;
            }
        }

        private static void ValidateElement(JsonElement expected, JsonElement actual, string path, List<string> errors)
        {
            // Allow nulls if both are null
            bool expectedIsNull = expected.ValueKind == JsonValueKind.Null || expected.ValueKind == JsonValueKind.Undefined;
            bool actualIsNull = actual.ValueKind == JsonValueKind.Null || actual.ValueKind == JsonValueKind.Undefined;

            if (expectedIsNull && actualIsNull) return;

            if (actualIsNull && !expectedIsNull)
            {
                string error = $"NULL value at {path} (expected non-null)";
                Console.WriteLine($"❌ {error}");
                errors.Add(error);
                return;
            }

            // Type mismatch
            if (expected.ValueKind != actual.ValueKind)
            {
                string error = $"Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}";
                Console.WriteLine($"❌ {error}");
                errors.Add(error);
                return;
            }

            // String validation
            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedValue = expected.GetString();
                var actualValue = actual.GetString();

                if (!string.IsNullOrEmpty(expectedValue) && string.IsNullOrEmpty(actualValue))
                {
                    string error = $"Empty string at {path}";
                    Console.WriteLine($"❌ {error}");
                    errors.Add(error);
                }
                return;
            }

            // Object recursion
            if (expected.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in expected.EnumerateObject())
                {
                    if (!actual.TryGetProperty(prop.Name, out var actualProp))
                    {
                        string error = $"Missing field at {path}.{prop.Name}";
                        Console.WriteLine($"❌ {error}");
                        errors.Add(error);
                        continue;
                    }
                    ValidateElement(prop.Value, actualProp, $"{path}.{prop.Name}", errors);
                }
            }

            // Array validation
            if (expected.ValueKind == JsonValueKind.Array &&
                expected.GetArrayLength() > 0 &&
                actual.GetArrayLength() > 0)
            {
                ValidateElement(expected[0], actual[0], $"{path}[0]", errors);
            }
        }
    }
}
