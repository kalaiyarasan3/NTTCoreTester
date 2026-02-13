using Newtonsoft.Json;
using NTTCoreTester.Core;
using NTTCoreTester.Models;

namespace NTTCoreTester.Services
{
  
    public class ConfigRunner 
    {
        private readonly IApiService _apiService;
        private const string CONFIG_FOLDER = "Configs";

        public ConfigRunner(IApiService apiService)
        {
            _apiService = apiService;

            if (!Directory.Exists(CONFIG_FOLDER))
                Directory.CreateDirectory(CONFIG_FOLDER);
        }

        public List<string> GetAvailableSuites()
        {
            if (!Directory.Exists(CONFIG_FOLDER))
                return new List<string>();

            var files = Directory.GetFiles(CONFIG_FOLDER, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }

        public async Task RunSuite(string configFileName)
        {
            string filePath = Path.Combine(CONFIG_FOLDER, $"{configFileName}.json");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"\n Config file not found: {filePath}");
                return;
            }

            // Load config
            string configContent = await File.ReadAllTextAsync(filePath);
            var suiteConfig = JsonConvert.DeserializeObject<TestSuiteConfig>(configContent);

            if (suiteConfig == null || suiteConfig.Requests == null || suiteConfig.Requests.Count == 0)
            {
                Console.WriteLine($"\n Invalid config or no requests found");
                return;
            }

            Console.WriteLine($"\n{new string('═', 80)}");
            Console.WriteLine($"  TEST SUITE: {suiteConfig.SuiteName}");
            Console.WriteLine($"  Description: {suiteConfig.Description}");
            Console.WriteLine($"  Total Requests: {suiteConfig.Requests.Count}");
            Console.WriteLine($"  Stop on Failure: {suiteConfig.StopOnFailure}");
            Console.WriteLine($"{new string('═', 80)}\n");

            int passed = 0, failed = 0;

            for (int i = 0; i < suiteConfig.Requests.Count; i++)
            {
                var request = suiteConfig.Requests[i];

                Console.WriteLine($"\n[{i + 1}/{suiteConfig.Requests.Count}] {request.Endpoint}");

                bool success = await _apiService.ExecuteRequestFromConfig(request);

                if (success)
                {
                    passed++;
                    Console.WriteLine($" {request.Endpoint} PASSED\n");
                }
                else
                {
                    failed++;
                    Console.WriteLine($" {request.Endpoint} FAILED\n");

                    if (suiteConfig.StopOnFailure)
                    {
                        Console.WriteLine($" Stopping suite execution due to failure (stopOnFailure=true)");
                        break;
                    }
                }

                // Small delay between requests
                if (i < suiteConfig.Requests.Count - 1)
                {
                    await Task.Delay(500);
                }
            }

            // Summary
            Console.WriteLine($"\n{new string('═', 80)}");
            Console.WriteLine($"  SUITE COMPLETE: {suiteConfig.SuiteName}");
            Console.WriteLine($"   Passed: {passed}");
            Console.WriteLine($"   Failed: {failed}");
            Console.WriteLine($"{new string('═', 80)}\n");
        }
    }
}
