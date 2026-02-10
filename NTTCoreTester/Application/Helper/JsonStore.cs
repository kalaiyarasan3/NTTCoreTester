using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NTTCoreTester.Application.Helper
{
    public static class JsonStore
    {

        private static readonly Dictionary<string, JsonElement> _apis
            = new(StringComparer.OrdinalIgnoreCase);

        public static void LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);

            foreach (var api in doc.RootElement.EnumerateObject())
            {
                _apis[api.Name] = api.Value.Clone();
            }
        }

        public static JsonElement GetApi(string apiName)
        {
            if (!_apis.TryGetValue(apiName, out var api))
                throw new KeyNotFoundException($"API not found in JsonStore: {apiName}");

            return api;
        }
    }


}
