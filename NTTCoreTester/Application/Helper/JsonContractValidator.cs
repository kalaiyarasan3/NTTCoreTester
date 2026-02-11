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
            // ---------- NULL Handling ----------
            if (expected.ValueKind == JsonValueKind.Null)
            {
                if (actual.ValueKind != JsonValueKind.Null)
                {
                    Console.WriteLine($"Expected null at {path}");
                    hasErrors = true;
                }
                return;
            }

            if (actual.ValueKind == JsonValueKind.Null ||
                actual.ValueKind == JsonValueKind.Undefined)
            {
                Console.WriteLine($"❌ NULL value at {path}");
                hasErrors = true;
                return;
            }

            // ---------- TYPE CHECK ----------
            if (expected.ValueKind != actual.ValueKind &&
                !(expected.ValueKind == JsonValueKind.Number &&
                  actual.ValueKind == JsonValueKind.Number))
            {
                Console.WriteLine(
                    $"Type mismatch at {path}. Expected {expected.ValueKind}, got {actual.ValueKind}");
                hasErrors = true;
                return;
            }

            // ---------- STRING VALIDATION ----------
            if (expected.ValueKind == JsonValueKind.String)
            {
                var expectedValue = expected.GetString();
                var actualValue = actual.GetString() ?? "";

                if (!string.IsNullOrEmpty(expectedValue) &&
                    expectedValue.Contains("||"))
                {
                    var parts = expectedValue.Split(
                        new[] { "||" },
                        2,
                        StringSplitOptions.None);

                    var pattern = parts[1];

                    if (!System.Text.RegularExpressions.Regex
                        .IsMatch(actualValue, pattern))
                    {
                        Console.WriteLine(
                            $"Regex mismatch at {path}. Value: '{actualValue}'");
                        hasErrors = true;
                    }

                    return;
                }

                // Exact match (if expected not empty)
                if (!string.IsNullOrEmpty(expectedValue) &&
                    expectedValue != actualValue)
                {
                    Console.WriteLine(
                        $"Value mismatch at {path}. Expected '{expectedValue}', got '{actualValue}'");
                    hasErrors = true;
                }

                return;
            }

            // ---------- NUMBER (TYPE ONLY) ----------
            if (expected.ValueKind == JsonValueKind.Number)
            {
                if (actual.ValueKind != JsonValueKind.Number)
                {
                    Console.WriteLine($"Expected number at {path}");
                    hasErrors = true;
                }
                return;
            }

            // ---------- BOOLEAN ----------
            if (expected.ValueKind == JsonValueKind.True ||
                expected.ValueKind == JsonValueKind.False)
            {
                if (expected.GetBoolean() != actual.GetBoolean())
                {
                    Console.WriteLine($"Boolean mismatch at {path}");
                    hasErrors = true;
                }
                return;
            }

            // ---------- OBJECT ----------
            if (expected.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in expected.EnumerateObject())
                {
                    if (!actual.TryGetProperty(prop.Name, out var actualProp))
                    {
                        Console.WriteLine($"Missing field at {path}.{prop.Name}");
                        hasErrors = true;
                        continue;
                    }

                    ValidateElement(
                        prop.Value,
                        actualProp,
                        $"{path}.{prop.Name}",
                        ref hasErrors);
                }

                return;
            }

            // ---------- ARRAY ----------
            if (expected.ValueKind == JsonValueKind.Array)
            {
                if (actual.ValueKind != JsonValueKind.Array)
                {
                    Console.WriteLine($" Expected array at {path}");
                    hasErrors = true;
                    return;
                }

                if (expected.GetArrayLength() == 0)
                    return; 

                var template = expected[0];

                int index = 0;
                foreach (var actualItem in actual.EnumerateArray())
                {
                    ValidateElement(
                        template,
                        actualItem,
                        $"{path}[{index}]",
                        ref hasErrors);

                    index++;
                }

                return;
            }
        }

    }
}