using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractSessionHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractSessionHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ExtractSession";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return ActivityResult.HardFail("DataObject is null");

            string? token = dataObject["susertoken"]?.Value<string>();
            string? userId = dataObject["uid"]?.Value<string>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return ActivityResult.HardFail("Invalid login response");

            _cache.Set(Constants.SUserToken, token);
            _cache.Set(Constants.UId, userId);

            return ActivityResult.Success();
        }
    }

}
