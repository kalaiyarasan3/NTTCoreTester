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
                choice = string.IsNullOrWhiteSpace(choice) ? "b" : choice;

                if (choice == "0")
                {
                    "\nCSV saved.".Info();
                    await _csvReport.Save();
                    continue;
                }

                if (choice == "00")
                {
                    "\nSaving and exiting...".Info();
                    await _csvReport.Save();
                    _cache.Clear();
                    return;
                }

                // NEW: browse scenarios
                if (choice == "b")
                {
                    await ShowScenarioBrowser();
                    continue;
                }

                var masterTests = _configRunner.GetAvailableMasterTest();
                var Tests = _configRunner.GetAvailableTests();

                int totalOptions = masterTests.Count + Tests.Count;

                if (int.TryParse(choice, out int index) && index > 0 && index <= totalOptions)
                {
                    if (index <= masterTests.Count)
                    {
                        string selectedMaster = masterTests[index - 1];
                        await _configRunner.RunMasterTest(selectedMaster);
                    }
                    else
                    {
                        int testIndex = index - masterTests.Count;
                        string selectedTest = Tests[testIndex - 1];
                        await _configRunner.RunTest(selectedTest);
                    }
                }
                else
                {
                    "\nInvalid option!".Error();
                }

                "\nPress any key to continue...".Info();
                Console.ReadKey(true);
            }
        }

        private void ShowMenu()
        {
            //Console.Clear();

            "NTT Core Tester".Success();

            string? token = _cache.Get<string>("token");

            if (!string.IsNullOrEmpty(token))
            {
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
            "AVAILABLE TESTS".Info();
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
                "MasterFiles".Info();
                foreach (var master in masterTests)
                {
                    $"{index++}. {master}".Info();
                }

                foreach (var Test in Tests)
                {
                    $"{index++}. {Test}".Info();
                }

            
            }

            Console.WriteLine();

            "B. Browse Scenarios// Press Enter".Warn();

            Console.WriteLine();

            "0. Save".Info();
            "00. Save and Exit".Info();
            $"{new string('─', 64)}".Info();

            "\nChoose option: ".Info();
        }

        private async Task ShowScenarioBrowser()
        {
            Console.Clear();

            "CONFIG SCENARIOS".Success();
            $"{new string('─', 64)}".Info();

            var scenarios = _configRunner.GetAllScenarios();

            var grouped = scenarios
                .OrderBy(s => s.Folder)
                .ThenBy(s => s.Name)
                .GroupBy(s => s.Folder);

            int index = 1;
            var map = new Dictionary<int, ScenarioInfo>();

            foreach (var group in grouped)
            {
                $"\n[{group.Key.ToUpper()}]".Warn();

                foreach (var scenario in group)
                {
                    $"{index}. {scenario.Name}".Info();
                    map[index] = scenario;
                    index++;
                }
            }

            "\n0. Back".Info();
            "\nChoose option: ".Info();

            var input = Console.ReadLine();

            if (input == "0")
                return;

            if (int.TryParse(input, out int selected) && map.ContainsKey(selected))
            {
                var scenario = map[selected];
                await _configRunner.RunTest(scenario.Name);
            }
            else
            {
                "\nInvalid option!".Error();
            }

            "\nPress any key to continue...".Info();
            Console.ReadKey(true);
        }
    }
}