using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NTTCoreTester.Configuration;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Reporting;
using NTTCoreTester.UI;

namespace NTTCoreTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
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
                    "ERROR: ApiConfiguration is null!".Info();
                    "Press any key to exit...".Info();
                    Console.ReadKey();
                    return;
                }
                 
                var services = new ServiceCollection();

                // Register configurations
                services.AddSingleton(apiCfg);
                services.AddSingleton(reportCfg);
                services.RegisterServices();

                var provider = services.BuildServiceProvider();
                 
                Console.Clear();
                " NTT Core Tester - File-Driven Testing".Info();
                 
                $"Server: {apiCfg.BaseUrl}".Info();
                $"Performance Threshold: 100ms".Info();
                $"Report Folder: {reportCfg.OutputFolder}".Info();
                $"Request Files: Requests\n".Info();
                 
                "Press any key to start testing...".Info();
                Console.ReadKey();
                Console.Clear();

                // Run tests
                var menu = provider.GetRequiredService<Menu>();
                await menu.Start();

                // Save CSV report at the end
                $"{ new string('=', 80)}".Info();
                "Saving CSV Report...".Info();
                $"{new string('=', 80)}".Info();

                //var csvReport = provider.GetRequiredService<CsvReport>();
                var csvReport = provider.GetRequiredService<CsvReport>();
                //await csvReport.Save();
                await csvReport.Save();

                "\nTesting completed successfully!".Info();
                "Press any key to exit...".Info();
                Console.ReadKey();
            }
            catch (Exception ex)
            {                
                $"\nError: {ex.Message}".Error();
                $"\nStack Trace:\n{ex.StackTrace}".Error();                
                Console.ReadKey();
            }
        }
    }
}
