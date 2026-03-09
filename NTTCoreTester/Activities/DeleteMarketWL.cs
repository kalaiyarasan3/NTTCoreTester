using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class DeleteMarketWL : IActivityHandler
    {
        private readonly PlaceholderCache _cache;
        private readonly ApiCall _api;

        public DeleteMarketWL(PlaceholderCache cache, ApiCall api)
        {
            _cache = cache;
            _api = api;
        }

        public string Name => nameof(DeleteMarketWL);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
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

                    var response = _api.PostAsync("AddMarketWatchName", payload).GetAwaiter().GetResult();
                    var responseJson = JObject.Parse(response);

                    var statusCode = responseJson["StatusCode"]?.Value<int>();

                    if (statusCode != 0)
                        return ActivityResult.SoftFail("Failed to delete market watch list.");

                    return ActivityResult.Success("Market watch list deleted successfully.");
                }
                
                return $"Market watch list with name 'TestMWList' not found.".FailWithLog(false);
            }
            catch(Exception ex)
            {
                return ActivityResult.HardFail($"An error occurred while deleting the market watch list: {ex.Message}");
            }
        }

        
    }

}

