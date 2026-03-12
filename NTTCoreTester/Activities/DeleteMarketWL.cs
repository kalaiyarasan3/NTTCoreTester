using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Services;

namespace NTTCoreTester.Activities
{
    public class DeleteMarketWL : IActivityHandler
    {
        private readonly PlaceholderCache _cache;
        private readonly IApiService _apiService;
        public DeleteMarketWL(PlaceholderCache cache, IApiService apiService)
        {
            _cache = cache;
            _apiService= apiService;
        }

        public string Name => nameof(DeleteMarketWL);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var values = result.DataObject["values"]?.ToObject<Dictionary<string, string>>();

                if (values == null || values.Count == 0)
                    return ActivityResult.SoftFail("No market lists found in the response.");

                var match = values.FirstOrDefault(x => x.Value == "TestMWList");

                if (!string.IsNullOrEmpty(match.Key))
                {
                    _cache.Set("MarketWatchId", match.Key);
                }

                var id = values?
                    .Where(x => x.Value == "TestMWList")
                    .Select(x => x.Key)
                    .FirstOrDefault();

                if (id != null)
                {
                    var payload = new JObject
                    {
                        ["MarketWatchId"] = id,
                        ["IsDeleted"] = "1"
                    }.ToString();

                    var header = _apiService.ResolveHeaders("AuthorizedHeaders");
                    var response = await _apiService.SendRequest("AddMarketWatchName", payload,header);
                    var responseJson = JObject.Parse(response.ResponseBody);

                    var statusCode = responseJson["StatusCode"]?.Value<int>();
                    var Msg = responseJson["Message"]?.Value<string>();

                    if (statusCode != 0)
                        return ActivityResult.SoftFail("Failed to delete market watch list.");

                    $"{Msg}".Info();
                    return ActivityResult.Success(Msg);
                }

                return $"Market watch list with name 'TestMWList' not found.".FailWithLog(false);
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"An error occurred while deleting the market watch list: {ex.Message}");
            }
        }

    }

}

