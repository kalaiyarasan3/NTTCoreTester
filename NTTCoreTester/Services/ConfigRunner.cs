using CsvHelper;
using Newtonsoft.Json;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Models;
using System.Globalization;

namespace NTTCoreTester.Services
{
  
    public class ConfigRunner 
    {
        private readonly IApiService _apiService;
        private const string CONFIG_FOLDER = "Configs";
        private readonly PlaceholderCache _cache;

        public ConfigRunner(IApiService apiService, PlaceholderCache cache)
        {
            _apiService = apiService;

            if (!Directory.Exists(CONFIG_FOLDER))
                Directory.CreateDirectory(CONFIG_FOLDER);
            _cache = cache;
        }


        
        public List<string> GetAvailableSuites()
        {
            if (!Directory.Exists(CONFIG_FOLDER))
                return new List<string>();

            var files = Directory.GetFiles(CONFIG_FOLDER, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
        

        //public List<string> GetAvailableSuites()
        //{
        //    var files =Directory.GetFiles(CONFIG_FOLDER, "*.json");

        //    return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        //}

        //public async Task RunSuiteCSV(string configFileName)
        //{
        //    string filePath= Path.Combine(CONFIG_FOLDER, $"{configFileName}.csv");

        //    using var reader=new StreamReader(filePath);
        //    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        //}

        public async Task RunSuite(string configFileName)
        {
            string filePath = Path.Combine(CONFIG_FOLDER, $"{configFileName}.json");

            if (!File.Exists(filePath))
            {
                $"\n Config file not found: {filePath}".Error();
                return;
            }

            // Load config
            string configContent = await File.ReadAllTextAsync(filePath);
            var suiteConfig = JsonConvert.DeserializeObject<TestSuiteConfig>(configContent);

            if (suiteConfig == null || suiteConfig.Requests == null || suiteConfig.Requests.Count == 0)
            {
                $"\n Invalid config or no requests found".Error();
                return;
            }

            $"\n{new string('═', 80)}".Success();
            $"  TEST SUITE: {suiteConfig.SuiteName}".Info();
            $"  Description: {suiteConfig.Description}".Info();
            $"  Total Requests: {suiteConfig.Requests.Count}".Info();
            $"  Stop on Failure: {suiteConfig.StopOnFailure}".Info();
            $"{new string('═', 80)}\n".Success();

            int passed = 0, failed = 0;
           
            //if (suiteConfig.Requests.Any(x=>x.Endpoint.Contains("SendOTP".Info()))
            //{
            //    Console.Write("Enter Uid: ".Info();
            //    var uid = Console.ReadLine();
            //    Console.Write("Enter pwd: ".Info();
            //    var pwd = Console.ReadLine();
                
            //    _cache.Set("uid", uid);
            //    _cache.Set("pwd", pwd);
                
            //}

           

            for (int i = 0; i < suiteConfig.Requests.Count; i++)
            {
                var request = suiteConfig.Requests[i];
                
                $"\n[{i + 1}/{suiteConfig.Requests.Count}] {request.Endpoint}".Info();

                bool success = await _apiService.ExecuteRequestFromConfig(request);
                    
                if (success)
                {
                    passed++;
                    $" {request.Endpoint} PASSED\n".Debug();
                }
                else
                {
                    failed++;
                    $" {request.Endpoint} FAILED\n".Error();

                    if (suiteConfig.StopOnFailure)
                    {
                        $" Stopping suite execution due to failure (stopOnFailure=true)".Error();
                        break;
                    }
                }

                // Small delay between requests
                if (i < suiteConfig.Requests.Count - 1)
                {
                    await Task.Delay(1500);
                }
            }

            // Summary
            $"\n{new string('═', 80)}".Success();
            $"  SUITE COMPLETE: {suiteConfig.SuiteName}".Info();
            $"   Passed: {passed}".Info();
            $"   Failed: {failed}".Info();
            _cache.Clear();
            $"{new string('═', 80)}\n".Success();
        }
    }
}
