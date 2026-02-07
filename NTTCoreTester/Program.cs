using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NTTCoreTester.BusinessLogic;
using NTTCoreTester.Configuration;
using NTTCoreTester.Reporting;
using NTTCoreTester.Scenarios;
using NTTCoreTester.Services;
using NTTCoreTester.UI;
using NTTCoreTester.Validators;

namespace NTTCoreTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // load config
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Get configurations
                var apiCfg = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
                var reportCfg = config.GetSection("ReportConfig").Get<ReportConfig>();

                // Use defaults if null
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

                // setup DI
                var services = new ServiceCollection();

                // Register configurations
                services.AddSingleton(apiCfg);
                services.AddSingleton(reportCfg);

                // Register HttpClient and API Service - THIS IS CRITICAL
                services.AddHttpClient<IApiService, ApiService>();

                // Register other services
                services.AddSingleton<IValidator, Validator>();
                services.AddSingleton<ICsvReport, CsvReport>();

                // Register Business Logic - THIS WAS THE PROBLEM
                services.AddSingleton<IAuthManager, AuthManager>();

                // Register Scenarios and UI
                services.AddSingleton<ITestScenarios, TestScenarios>();
                services.AddSingleton<Menu>();

                var provider = services.BuildServiceProvider();

                // run
                var menu = provider.GetRequiredService<Menu>();

                Console.Clear();
                Console.WriteLine("  Auth Test              ");
                Console.WriteLine();
                Console.WriteLine($"Server: {apiCfg.BaseUrl}");
                Console.WriteLine("\nPress any key to start...");
                Console.ReadKey();
                Console.Clear();

                await menu.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n!! ERROR: {ex.Message}");
                Console.WriteLine($"\n{ex.StackTrace}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
