using NTTCoreTester.Models;
using System.Text.RegularExpressions;

namespace NTTCoreTester.Core
{
    public class PlaceholderCache
    {
        private readonly Dictionary<string, string> _cache;
        private static readonly Regex _variableRegex = new Regex(@"\{\{(.*?)\}\}", RegexOptions.Compiled);
      
        public PlaceholderCache()
        {
            _cache = new Dictionary<string, string>();
        }

        public void Set(string key, string value)
        {
            _cache[key] = value;
        }

        public string Get(string key)
        {
            return _cache.ContainsKey(key) ? _cache[key] : null;
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
                    return value;

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
