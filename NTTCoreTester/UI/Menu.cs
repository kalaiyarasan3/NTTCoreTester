using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Reporting;
using NTTCoreTester.Services;

namespace NTTCoreTester.UI
{
    public class Menu
    {
        private readonly ConfigRunner _configRunner;
        private readonly PlaceholderCache _cache;
        private readonly CsvReport _csvReport;

        public Menu(ConfigRunner configRunner, PlaceholderCache cache, CsvReport csvReport)
        {
            _configRunner = configRunner;
            _cache = cache;
            _csvReport = csvReport;
        }

        public async Task Start()
        {
            while (true)
            {
                ShowMenu();
                string? choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    "\nExiting... CSV will be saved automatically.".Info();
                    await _csvReport.Save();
                    continue;
                }
                if (choice == "00")
                {
                    "\nExiting... CSV will be saved automatically.".Info();
                    _cache.Clear();
                    return;
                }

                

                var masterTests = _configRunner.GetAvailableMasterTest();
                var suites = _configRunner.GetAvailableSuites();

                int totalOptions = masterTests.Count + suites.Count;

                if(int.TryParse(choice, out int index) && index > 0 && index <= totalOptions)
                {
                    if(index <= suites.Count)
                    {
                        string selectedSuite = suites[index - 1];
                        await _configRunner.RunSuite(selectedSuite);
                    }
                    else
                    {
                        int masterIndex = index - suites.Count;
                        string selectedMaster = masterTests[masterIndex - 1];
                        await _configRunner.RunMasterTest(selectedMaster);
                    }
                }

                else
                {
                    "\nInvalid option!".Error();
                }

                "\nPress any key to continue...".Info();
                Console.ReadKey(true);
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            "NTT Core Tester".Success();  

            // Show session status using cache
            string? token = _cache.Get<string>("token");

            if (!string.IsNullOrEmpty(token))
            {
                _ = _cache.Get<string>("token");
                string? userName = _cache.Get<string>("userName");
                string? userId = _cache.Get<string>("uid");

                "\nLOGGED IN".Success();
                $"Session token: {token}".Info();
                $"User: {userName} ({userId})".Info();
            }
            else
            {
                "\nNO ACTIVE SESSION".Warn();
            }

            Console.WriteLine(); 

            $"{new string('─', 64)}".Info();

            "AVAILABLE TEST SUITES:".Info();
            $"{new string('─', 64)}".Info();

            var masterTests = _configRunner.GetAvailableMasterTest();
            var suites = _configRunner.GetAvailableSuites();

            if (suites.Count == 0 && masterTests.Count == 0)
            {
                "No config files found in Configs/ folder".Warn();
            }
            else
            {
                int index = 1;

                foreach (var suite in suites)
                {
                    $"{index++}. {suite}".Info();
                }

                foreach (var master in masterTests)
                {
                    $"{index++}. {master}".Info();
                }
            }

            Console.WriteLine();

            "0. Save".Info();
            "00. Save and Exit".Info();
            $"{new string('─', 64)}".Info();

            "\nChoose option: ".Info();   // note: no newline at the end → user types right after
        }
    }
}