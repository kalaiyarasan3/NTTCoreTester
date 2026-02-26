using Microsoft.Extensions.DependencyInjection;
using NTTCoreTester.Activities;
using NTTCoreTester.Core;
using NTTCoreTester.Reporting;
using NTTCoreTester.Services;
using NTTCoreTester.UI;
using NTTCoreTester.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester
{
    public static class DependencyContainer
    {
        public static ServiceCollection RegisterServices(this ServiceCollection services)
        {

            services.AddSingleton<PlaceholderCache, PlaceholderCache>();
            services.AddSingleton<CsvReport>();
            services.AddSingleton<ResponseChecker>();
            services.AddSingleton<ConfigRunner>();
            services.AddSingleton<ActivityExecutor>();

            services.AddTransient<IActivityHandler, ExtractSessionHandler>();
            services.AddTransient<IActivityHandler, ExtractOTPHandler>();
            services.AddTransient<IActivityHandler, ExtractClientOrdIdHandler>();
            services.AddTransient<IActivityHandler, GetLastOrderStatusHandler>();
            services.AddTransient<IActivityHandler, ExtractSecurityInfoHandler>();
            services.AddTransient<IActivityHandler, GetOrderMarginHandler>();
            services.AddTransient<IActivityHandler, ExtractPostLimitMarginHandler>();
            services.AddTransient<IActivityHandler, ExtractPreLimitMarginHandler>();
            services.AddTransient<IActivityHandler, ExtractPrePositionsHandler>();
            services.AddTransient<IActivityHandler, ExtractTradeFillHandler>();
            services.AddTransient<IActivityHandler, ValidatePostPositionsHandler>();
            services.AddTransient<IActivityHandler, ConfirmOrderStatusHandler>();
            services.AddTransient<IActivityHandler, MarketWatcListID>();
            services.AddTransient<IActivityHandler, ValidateActivityStatusHandler>();


            services.AddHttpClient<IApiService, ApiService>()
                  .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                  {
                      AutomaticDecompression = DecompressionMethods.GZip
                                             | DecompressionMethods.Deflate
                                             | DecompressionMethods.Brotli,
                      UseCookies = false
                  });

            services.AddSingleton<Menu>();

            return services;
        }
    }
}
