using System.Text.Json;

namespace NTTCoreTester.Application.Helper
{
    public static class JsonContractValidator
    {
        public static bool Validate(string expectedJson, JsonElement actualJson)
        {
            using var expectedDoc = JsonDocument.Parse(expectedJson);
           

            bool hasErrors = false;

            ValidateElement(
                expectedDoc.RootElement,
                actualJson,
                "$",
                ref hasErrors
            );

            return !hasErrors;
        }

        private static void ValidateElement(
            JsonElement expected,
            JsonElement actual,
            string path,
            ref bool hasErrors)
        {
            // Null / undefined check
            if (actual.ValueKind == JsonValueKind.Null ||
                actual.ValueKind == JsonValueKind.Undefined)
            {
                Console.WriteLine($"NULL value at {path}");
                hasErrors = true;
                return;
            }

            if (expected.ValueKind != actual.ValueKind)
            {
                Console.WriteLine(
                    $"Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}");
                hasErrors = true;
                return;
            }

            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedValue = expected.GetString();
                var actualValue = actual.GetString();

                if (!string.IsNullOrEmpty(expectedValue) &&
                    string.IsNullOrEmpty(actualValue))
                {
                    Console.WriteLine($"Empty string at {path}");
                    hasErrors = true;
                }
                return;
            }

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