using NTTCoreTester.Services;
using NTTCoreTester.Core;

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
                string choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    Console.WriteLine("\n Exiting... CSV will be saved automatically.");
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
                    Console.WriteLine("\n Invalid option!");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("NTT Core Tester");

            // Show session status using cache
            string? token = _cache.Get<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                string? userName = _cache.Get<string>("userName");
                string? userId = _cache.Get<string>("uid");

                Console.WriteLine($"\n LOGGED IN");
                Console.WriteLine($"   User: {userName} ({userId})");
            }
            else
            {
                Console.WriteLine("\n NO ACTIVE SESSION");
            }

            Console.WriteLine($"\n{new string('─', 64)}");
            Console.WriteLine("AVAILABLE TEST SUITES:");
            Console.WriteLine(new string('─', 64));

            var suites = _configRunner.GetAvailableSuites();

            if (suites.Count == 0)
            {
                Console.WriteLine("No config files found in Configs/ folder");
            }
            else
            {
                for (int i = 0; i < suites.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {suites[i]}");
                }
            }

            Console.WriteLine($"\n  0. Exit (Auto-save CSV Report)");
            Console.WriteLine(new string('─', 64));
            Console.Write("\nChoose option: ");
        }
    }
}
