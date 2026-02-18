using NTTCoreTester.Core;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    internal class ExtractSecurityInfoHandler: IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractSecurityInfoHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ExtractSecurityInfo";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return ActivityResult.HardFail("DataObject is null");

            string? exch = dataObject["exch"]?.ToString();
            string? tsym = dataObject["tsym"]?.ToString();
            string? lp = dataObject["lp"]?.ToString();
            _cache.Set("exch", exch);
            _cache.Set("tsym", tsym);
            _cache.Set("lp", lp);

            return ActivityResult.Success("Security info extracted and stored in cache");
        }

      
    }
}
