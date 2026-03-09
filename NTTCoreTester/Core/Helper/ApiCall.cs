using System.Diagnostics;
using System.Text;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Core.Helper
{
    public class ApiCall
    {
        private readonly HttpClient _client;
        private readonly PlaceholderCache _cache;
        private readonly ApiConfiguration _cfg;

        public ApiCall(ApiConfiguration cfg, PlaceholderCache cache)
        {
            _cfg = cfg;
            _cache = cache;

            _client = new HttpClient
            {
                BaseAddress = new Uri(cfg.BaseUrl)
            };
        }

        private void ApplyHeaders()
        {
            _client.DefaultRequestHeaders.Clear();

            if (_cfg.HeaderProfiles == null)
                return;

            if (!_cfg.HeaderProfiles.TryGetValue("AuthorizedHeaders", out var headers))
                return;

            foreach (var h in headers)
            {
                var value = _cache.ReplaceVariables(h.Value);

                if (!value.IsSuccess)
                {
                    $"Failed to resolve placeholder in header: {h.Key}".Error();
                    continue;
                }

                _client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, value.Text);
            }
        }

        public async Task<string> PostAsync(string endpoint, string json)
        {
            ApplyHeaders();

            var stopwatch = Stopwatch.StartNew();
            string fullUrl = $"{_cfg.BaseUrl.TrimEnd('/')}{endpoint}";
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            $"POST {endpoint}".Info();
            $"Request: {json}".Info();

            var response = await _client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();

            $"Response ({stopwatch.ElapsedMilliseconds} ms):".Info();
            responseBody.Info();

            response.EnsureSuccessStatusCode();

            return responseBody;
        }

    }
}