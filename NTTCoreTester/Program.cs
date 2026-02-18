using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NTTCoreTester.Activities;
using NTTCoreTester.Configuration;
using NTTCoreTester.Core;
using NTTCoreTester.Reporting;
using NTTCoreTester.Services;
using NTTCoreTester.UI;
using NTTCoreTester.Validators;
using System.Net;

namespace NTTCoreTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Load config
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var apiCfg = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
                var reportCfg = config.GetSection("ReportConfig").Get<ReportConfig>();

                if (reportCfg == null)
                {
                    reportCfg = new ReportConfig
                    {
                        OutputFolder = "Reports",
                        FilePrefix = "auth_test"
                    };
                }

                if (apiCfg == null)
                {
                    Console.WriteLine("ERROR: ApiConfiguration is null!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Setup DI
                var services = new ServiceCollection();

                // Register configurations
                services.AddSingleton(apiCfg);
                services.AddSingleton(reportCfg);

                // Register core services
                services.AddSingleton<PlaceholderCache, PlaceholderCache>();
                services.AddSingleton<CsvReport>();
                services.AddSingleton<ResponseChecker>();
                services.AddSingleton<ConfigRunner>();
                services.AddSingleton<ActivityExecutor>();
                services.AddSingleton<PlaceholderResolver>();

                services.AddSingleton<IActivityHandler, ExtractSessionHandler>();
                services.AddSingleton<IActivityHandler, ExtractOTPHandler>();
                services.AddSingleton<IActivityHandler, ExtractClientOrdIdHandler>();
                services.AddSingleton<IActivityHandler, GetLastOrderStatusHandler>();

                // HttpClient with proper decompression
                services.AddHttpClient<IApiService, ApiService>()
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                                               | DecompressionMethods.Deflate
                                               | DecompressionMethods.Brotli,
                        UseCookies = false
                    });

                // Register UI
                services.AddSingleton<Menu>();

                var provider = services.BuildServiceProvider();

                // Display startup info
                Console.Clear();
                Console.WriteLine(" NTT Core Tester - File-Driven Testing");
                Console.WriteLine();
                Console.WriteLine($"Server: {apiCfg.BaseUrl}");
                Console.WriteLine($"Performance Threshold: 100ms");
                Console.WriteLine($"Report Folder: {reportCfg.OutputFolder}");
                Console.WriteLine($"Request Files: Requests/");
                Console.WriteLine();
                Console.WriteLine("Press any key to start testing...");
                Console.ReadKey();
                Console.Clear();

                // Run tests
                var menu = provider.GetRequiredService<Menu>();
                await menu.Start();

                // Save CSV report at the end
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("Saving CSV Report...");
                Console.WriteLine(new string('=', 80));

                var csvReport = provider.GetRequiredService<CsvReport>();
                await csvReport.Save();

                Console.WriteLine("\nTesting completed successfully!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + new string('═', 80));
                Console.WriteLine("FATAL ERROR");
                Console.WriteLine(new string('═', 80));
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                Console.WriteLine("\n" + new string('═', 80));
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
