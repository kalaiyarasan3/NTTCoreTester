using Newtonsoft.Json;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Services
{

    public class ConfigRunner
    {
        private readonly IApiService _apiService;
        private const string CONFIG_FOLDER = "Configs";
        private const string MASTER_CONFIG_FOLDER = "ConfigMaster";
        private readonly PlaceholderCache _cache;

        public ConfigRunner(IApiService apiService, PlaceholderCache cache)
        {
            _apiService = apiService;

            if (!Directory.Exists(CONFIG_FOLDER))
                Directory.CreateDirectory(CONFIG_FOLDER);
            if (!Directory.Exists(MASTER_CONFIG_FOLDER))
                Directory.CreateDirectory(MASTER_CONFIG_FOLDER);
            _cache = cache;
        }

        public List<string> GetAvailableMasterTest()
        {

            if (!Directory.Exists(MASTER_CONFIG_FOLDER))
                return new List<string>();


            var files = Directory.GetFiles(MASTER_CONFIG_FOLDER, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }

        public async Task RunMasterTest(string masterConfigFileName)
        {
            string filePath = Path.Combine(MASTER_CONFIG_FOLDER, $"{masterConfigFileName}.json");

            if (!File.Exists(filePath))
            {
                $"\n Master config file not found: {filePath}".Error();
                return;
            }

            string configContent = await File.ReadAllTextAsync(filePath);
            var mastersuiteConfig = JsonConvert.DeserializeObject<MasterSuite>(configContent);

            if (mastersuiteConfig == null || mastersuiteConfig.Suites == null || mastersuiteConfig.Suites.Count == 0)
            {
                $"\n Invalid master config or no suites found".Error();
                return;
            }

            foreach (var suite in mastersuiteConfig.Suites)
            {
                if (!suite.Enabled)
                    continue;

                $"\n Running suite: {suite.TestName}".Info();
                bool result = await RunSuite(suite.Path);
                if (!result && mastersuiteConfig.StopOnFailure)
                {
                    "\nStopping master execution".Error();
                    break;
                }
            }

        }

        public List<string> GetAvailableSuites()
        {
            if (!Directory.Exists(CONFIG_FOLDER))
                return new List<string>();

            var files = Directory.GetFiles(CONFIG_FOLDER, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }

        public async Task<bool> RunSuite(string configFileName)
        {
            try
            {
                string filePath = Path.Combine(CONFIG_FOLDER, $"{configFileName}.json");

                if (!File.Exists(filePath))
                {
                    $"\n Config file not found: {filePath}".Error();
                    return false;
                }

                // Load config

                string configContent = await File.ReadAllTextAsync(filePath);
                var testConfig = JsonConvert.DeserializeObject<TestSuiteConfig>(configContent);

                if (testConfig == null || testConfig.Requests == null || testConfig.Requests.Count == 0)
                {
                    $"\n Invalid config or no requests found".Error();
                    return false;
                }

                $"\n{new string('═', 80)}".Success();
                $"  TEST SUITE: {testConfig.TestName}".Info();
                $"  Description: {testConfig.Description}".Info();
                $"  Total Requests: {testConfig.Requests.Count}".Info();
                $"  Stop on Failure: {testConfig.StopOnFailure}".Info();
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



                for (int i = 0; i < testConfig.Requests.Count; i++)
                {
                    var request = testConfig.Requests[i];

                    $"\n[{i + 1}/{testConfig.Requests.Count}] {request.Endpoint}".Info();

                    bool success = await _apiService.ExecuteRequestFromConfig(request, testConfig);

                    if (success)
                    {
                        passed++;
                        $" {request.Endpoint} PASSED\n".Debug();
                    }
                    else
                    {
                        failed++;
                        $" {request.Endpoint} FAILED\n".Error();

                        if (testConfig.StopOnFailure)
                        {
                            $" Stopping suite execution due to failure (stopOnFailure=true)".Error();
                            break;
                        }
                    }

                    if (i < testConfig.Requests.Count - 1)
                    {
                        await Task.Delay(500);
                    }
                }

                // Summary
                $"\n{new string('═', 80)}".Success();
                $"  SUITE COMPLETE: {testConfig.TestName}".Info();
                $"   Passed: {passed}".Info();
                $"   Failed: {failed}".Info();
                $"{new string('═', 80)}\n".Success();

                return failed == 0;
            }
            catch (JsonReaderException ex)
            {
                $"\n JSON Error in {configFileName}".Error();
                $"\n Line: {ex.LineNumber} Position: {ex.LinePosition}".Error();
                $"\n Path: {ex.Path}".Error();
                $"\n Message: {ex.Message}".Error();
                return false;
            }
            catch (Exception ex)
            {
                $"\n Suite Failed: {configFileName}".Error();
                $"\n {ex}".Error();
                return false;
            }
        }
    }
}
