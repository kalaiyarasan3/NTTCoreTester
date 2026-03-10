using NTTCoreTester.Core.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractSession : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractSession(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name =>nameof(ExtractSession);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "DataObject is null".FailWithLog();

            string? token = dataObject["susertoken"]?.Value<string>();
            string? userId = dataObject["uid"]?.Value<string>();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return "Invalid login response".FailWithLog();

            _cache.Set(Constants.SUserToken, token);
            _cache.Set(Constants.UId, userId);

            var log= string.Join(", ", new[] { $"Token: {token}", $"UserId: {userId}" });

            return ActivityResult.Success("Token Extracted "+log);
        }
    }

}
