using NTTCoreTester.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NTTCoreTester.Core
{
    public class PlaceholderCache
    {
        private readonly Dictionary<string, object> _cache;
        private static readonly Regex _variableRegex = new Regex(@"\{\{(.*?)\}\}", RegexOptions.Compiled);
      
        public PlaceholderCache()
        {
            _cache = new Dictionary<string, object>();
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
                var key = match.Groups[1].Value;

                if (_cache.TryGetValue(key, out var value))
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

                missingVariables.Add(key);
                return match.Value;  
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
    }
}
