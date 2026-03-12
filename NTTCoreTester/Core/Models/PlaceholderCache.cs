using System.Globalization;
using System.Text.RegularExpressions;

namespace NTTCoreTester.Core.Models
{
    public class PlaceholderCache
    {
        private readonly Dictionary<string, object> _cache;
        private static readonly Regex _variableRegex = new Regex(@"\{\{(.*?)\}\}", RegexOptions.Compiled);
      
        public PlaceholderCache()
        {
            _cache = new Dictionary<string, object>();
        }
        public bool Remove(string key)
        {
            return _cache.Remove(key);
        }
        public void Set<T>(string key, T value)
        {
            _cache[key] = value!;
        }

        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value))
                return (T)value;

            return default;
        }

        public bool Has(string key)
        {
            return _cache.ContainsKey(key);
        }

        public void Clear()
        {
            _cache.Clear();
            Console.WriteLine("  Input cache cleared");
        }

        public VariableReplaceResult ReplaceVariables(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return VariableReplaceResult.Success(text);

            var missingVariables = new List<string>();

            string result = _variableRegex.Replace(text, match =>
            {
                var expr = match.Groups[1].Value;

                try
                {
                    if (Regex.IsMatch(expr, @"[\+\-\*/]"))
                    {
                        var value = EvaluateExpression(expr);
                        return value.ToString(CultureInfo.InvariantCulture);
                    }

                    if (_cache.TryGetValue(expr, out var val))
                        return Convert.ToString(val, CultureInfo.InvariantCulture) ?? "";

                    missingVariables.Add(expr);
                    return match.Value;
                }
                catch
                {
                    missingVariables.Add(expr);
                    return match.Value;
                }
            });

            if (missingVariables.Any())
            {
                return VariableReplaceResult.Failure(
                    result,
                    $"Missing variables: {string.Join(", ", missingVariables)}"
                );
            }

            return VariableReplaceResult.Success(result);
        }
        private decimal EvaluateExpression(string expression)
        {
            var parts = Regex.Split(expression, @"([\+\-\*/])");

            decimal value = GetVariable(parts[0]);

            for (int i = 1; i < parts.Length; i += 2)
            {
                string op = parts[i];
                decimal right = decimal.Parse(parts[i + 1], CultureInfo.InvariantCulture);

                switch (op)
                {
                    case "+":
                        value += right;
                        break;
                    case "-":
                        value -= right;
                        break;
                    case "*":
                        value *= right;
                        break;
                    case "/":
                        value /= right;
                        break;
                }
            }

            return value;
        }

        private decimal GetVariable(string key)
        {
            if (!_cache.TryGetValue(key.Trim(), out var val))
                throw new Exception($"Variable {key} not found");

            return Convert.ToDecimal(val, CultureInfo.InvariantCulture);
        }
    }
}
