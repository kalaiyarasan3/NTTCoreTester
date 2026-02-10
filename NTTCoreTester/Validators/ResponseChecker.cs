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

        public bool Check(string endpoint, string responseJson)
        {
            try
            {
                string filePath = Path.Combine(_expectedFolder, $"{endpoint}_response.json");
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ NO FILE: {filePath}");
                    return false;
                }

                string expectedJson = File.ReadAllText(filePath);

                using var expectedDoc = JsonDocument.Parse(expectedJson);
                using var actualDoc = JsonDocument.Parse(responseJson);

                bool hasErrors = false;

                ValidateElement(
                    expectedDoc.RootElement,
                    actualDoc.RootElement,
                    endpoint,
                    ref hasErrors
                );

                if (!hasErrors)
                    Console.WriteLine($"✅ {endpoint} STRUCTURE OK ✓");

                return !hasErrors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {endpoint} ERROR: {ex.Message}");
                return false;
            }
        }

        private static void ValidateElement(
            JsonElement expected,
            JsonElement actual,
            string path,
            ref bool hasErrors)
        {
            bool expectedIsNull = expected.ValueKind == JsonValueKind.Null;
            bool actualIsNull = actual.ValueKind == JsonValueKind.Null;

            if (expectedIsNull && actualIsNull)
                return; 

            if (actualIsNull && !expectedIsNull)
            {
                Console.WriteLine($"❌ NULL value at {path} (expected non-null)");
                hasErrors = true;
                return;
            }

            // Type mismatch
            if (expected.ValueKind != actual.ValueKind)
            {
                Console.WriteLine(
                    $"❌ Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}");
                hasErrors = true;
                return;
            }

            // String empty check (only if expected is non-empty)
            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedValue = expected.GetString();
                var actualValue = actual.GetString();

                if (!string.IsNullOrEmpty(expectedValue) &&
                    string.IsNullOrEmpty(actualValue))
                {
                    Console.WriteLine($"❌ Empty string at {path}");
                    hasErrors = true;
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
                        Console.WriteLine($"❌ Missing field at {path}.{prop.Name}");
                        hasErrors = true;
                        continue;
                    }

                    ValidateElement(
                        prop.Value,
                        actualProp,
                        $"{path}.{prop.Name}",
                        ref hasErrors);
                }
            }

            // Array validation (structure only)
            if (expected.ValueKind == JsonValueKind.Array &&
                expected.GetArrayLength() > 0 &&
                actual.GetArrayLength() > 0)
            {
                ValidateElement(
                    expected[0],
                    actual[0],
                    $"{path}[0]",
                    ref hasErrors);
            }
        }
    }
}
