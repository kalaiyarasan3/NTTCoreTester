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
                var Tests = _configRunner.GetAvailableTests();

                int totalOptions = masterTests.Count + Tests.Count;

                if(int.TryParse(choice, out int index) && index > 0 && index <= totalOptions)
                {
                    if(index <= Tests.Count)
                    {
                        string selectedTest = Tests[index - 1];
                        await _configRunner.RunTest(selectedTest);
                    }
                    else
                    {
                        int masterIndex = index - Tests.Count;
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
                //Console.Clear();
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

            "AVAILABLE TEST TestS:".Info();
            $"{new string('─', 64)}".Info();

            var masterTests = _configRunner.GetAvailableMasterTest();
            var Tests = _configRunner.GetAvailableTests();

            if (Tests.Count == 0 && masterTests.Count == 0)
            {
                "No config files found in Configs/ folder".Warn();
            }
            else
            {
                int index = 1;

                foreach (var Test in Tests)
                {
                    $"{index++}. {Test}".Info();
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