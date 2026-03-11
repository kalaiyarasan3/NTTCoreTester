using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    /// <summary>
    /// Extracting security info using exch and token
    /// it stores exch, tsym and lp
    /// </summary>
    public class ExtractSecurityInfo : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractSecurityInfo(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ExtractSecurityInfo";

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "DataObject is null".FailWithLog();

            string? exch = dataObject["exch"]?.ToString();
            string? tsym = dataObject["tsym"]?.ToString();
            string? lp = dataObject["lp"]?.ToString();
            _cache.Set("exch", exch);
            _cache.Set("tsym", tsym);
            _cache.Set("lp", lp);

            var log = string.Join(" | ", $"exch: {exch}", $"tsym: {tsym}", $"lp: {lp}");

            //Console.Write($"Enter qty: ");
            //int? qty = Convert.ToInt32(Console.ReadLine());
            //_cache.Set("qty", qty);
            log.Info();

            return ActivityResult.Success($"Etracted Sysmbol: {log}");
        }


    }
}
