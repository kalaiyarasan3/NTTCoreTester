using NTTCoreTester.Services;
using NTTCoreTester.Core;

namespace NTTCoreTester.UI
{
    public class Menu
    {
        private readonly IApiService _apiService;
        private readonly ISessionManager _sessionManager;

        public Menu(IApiService apiService, ISessionManager sessionManager)
        {
            _apiService = apiService;
            _sessionManager = sessionManager;
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

                var requests = _apiService.GetAvailableRequests();

                if (int.TryParse(choice, out int index) && index > 0 && index <= requests.Count)
                {
                    string selectedRequest = requests[index - 1];
                    await _apiService.ExecuteRequest(selectedRequest);
                }
                else
                {
                    Console.WriteLine("\n❌ Invalid option!");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("NTT Core Tester ");

            // Show session status
            if (_sessionManager.HasSession())
            {
                Console.WriteLine($"\n LOGGED IN");
                Console.WriteLine($"   User: {_sessionManager.GetUserName()} ({_sessionManager.GetUserId()})");
                Console.WriteLine($"   Token: {((SessionManager)_sessionManager)}");
            }
            else
            {
                Console.WriteLine("\n NO ACTIVE SESSION");
            }

            Console.WriteLine("\n" + new string('─', 64));
            Console.WriteLine("AVAILABLE API TESTS:");
            Console.WriteLine(new string('─', 64));

            var requests = _apiService.GetAvailableRequests();

            if (requests.Count == 0)
            {
                Console.WriteLine("    No request files found in Requests/ folder");
            }
            else
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {requests[i]}");
                }
            }

            Console.WriteLine("\n  0. Exit (Auto-save CSV Report)");
            Console.WriteLine(new string('─', 64));
            Console.Write("\nChoose option: ");
        }
    }
}
