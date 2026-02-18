using NTTCoreTester.Core;
using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractOTPHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public ExtractOTPHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "ExtractOTP";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            Console.Write("Enter OTP: ");
            var otp = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(otp))
                return ActivityResult.HardFail("OTP cannot be empty");

            _cache.Set("otp", otp);

            return ActivityResult.Success();
        }
    }

}
