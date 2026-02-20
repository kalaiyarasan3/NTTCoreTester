using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Services;

namespace NTTCoreTester.UI
{
    public class Menu
    {
        private readonly ConfigRunner _configRunner;
        private readonly PlaceholderCache _cache;

        public Menu(ConfigRunner configRunner, PlaceholderCache cache)
        {
            _configRunner = configRunner;
            _cache = cache;
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
                    return;
                }

                var suites = _configRunner.GetAvailableSuites();

                if (int.TryParse(choice, out int index) && index > 0 && index <= suites.Count)
                {
                    string selectedSuite = suites[index - 1];
                    await _configRunner.RunSuite(selectedSuite);
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
                string? userName = _cache.Get<string>("userName");
                string? userId = _cache.Get<string>("uid");

                "\nLOGGED IN".Success();
                $"User: {userName} ({userId})".Info();
            }
            else
            {
                "\nNO ACTIVE SESSION".Warn();
            }

            Console.WriteLine(); // empty line for readability

            $"{new string('─', 64)}".Info();

            "AVAILABLE TEST SUITES:".Info();
            $"{new string('─', 64)}".Info();

            var suites = _configRunner.GetAvailableSuites();

            if (suites.Count == 0)
            {
                "No config files found in Configs/ folder".Warn();
            }
            else
            {
                for (int i = 0; i < suites.Count; i++)
                {
                    $" {i + 1}. {suites[i]}".Info();
                }
            }

            Console.WriteLine();

            " 0. Exit (Auto-save CSV Report)".Info();
            $"{new string('─', 64)}".Info();

            "\nChoose option: ".Info();   // note: no newline at the end → user types right after
        }
    }
}