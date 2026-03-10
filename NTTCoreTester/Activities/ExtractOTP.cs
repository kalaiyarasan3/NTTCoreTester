using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractOTP : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractOTP(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => nameof(ExtractOTP);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            Console.Write("Enter OTP: ");
            var otp = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(otp))
                return  "OTP cannot be empty".FailWithLog();

            _cache.Set("otp", otp);

            return ActivityResult.Success();
        }
    }

}
