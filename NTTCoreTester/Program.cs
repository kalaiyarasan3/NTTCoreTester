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
                    .AddJsonFile("appsettings.json")
                    .Build();

                var apiCfg = config.GetSection("ApiConfiguration").Get<ApiConfiguration>();
                var reportCfg = config.GetSection("ReportConfig").Get<ReportConfig>();

                // setup DI
                var services = new ServiceCollection();

                services.AddSingleton(apiCfg);
                services.AddSingleton(reportCfg);
                // If you want the BusinessLogic version:
                services.AddSingleton<NTTCoreTester.BusinessLogic.AuthManager,
                                      NTTCoreTester.BusinessLogic.AuthManager>();

                //// OR if you want the Services version:
                //services.AddSingleton<NTTCoreTester.Services.AuthManager,
                //                      NTTCoreTester.Services.AuthManager>();

                services.AddSingleton<IValidator, Validator>();
                services.AddSingleton<ICsvReport, CsvReport>();
                
                services.AddSingleton<ITestScenarios, TestScenarios>();
                services.AddSingleton<Menu>();

                var provider = services.BuildServiceProvider();

                // run
                var menu = provider.GetRequiredService<Menu>();

                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════╗");
                Console.WriteLine("║  Fintech Auth Test v1.0              ║");
                Console.WriteLine("╚══════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("Ready to test authentication APIs");
                Console.WriteLine("\nPress any key to start...");
                Console.ReadKey();
                Console.Clear();

                await menu.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n!! ERROR: {ex.Message}");
                Console.WriteLine($"\n{ex.StackTrace}");
                Console.ReadKey();
            }
        }
    }
}
