using NTTCoreTester.Models;
using System.Text.RegularExpressions;

namespace NTTCoreTester.Core
{
    public class PlaceholderCache
    {
        private readonly Dictionary<string, string> _cache;

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
            if (string.IsNullOrEmpty(text))
                return text.VariableReplace(true);

            var matches = Regex.Matches(text, @"\{\{(.*?)\}\}");

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;

                if (!_cache.ContainsKey(key))
                {


                    return text.VariableReplace(false, $"Variable '{key}' not found in cache");

                }

                text = text.Replace(match.Value, _cache[key]);
            }
            return text.VariableReplace(true);
        }


    }
}
