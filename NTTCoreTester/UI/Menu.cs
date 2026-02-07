using NTTCoreTester.BusinessLogic;
using NTTCoreTester.Reporting;
using NTTCoreTester.Scenarios;

namespace NTTCoreTester.UI
{
    public class Menu
    {
        private readonly ITestScenarios _scenarios;
        private readonly ICsvReport _report;
        private readonly IAuthManager _authManager;

        public Menu(ITestScenarios scenarios, ICsvReport report, IAuthManager authManager)
        {
            _scenarios = scenarios;
            _report = report;
            _authManager = authManager;
        }

        public async Task Start()
        {
            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RunScenarioA();
                        break;
                    case "2":
                        await RunScenarioB();
                        break;
                    case "3":
                        await RunScenarioC();
                        break;
                    case "4":
                        await _report.Save();
                        break;
                    case "5":
                        Console.WriteLine($"\nReport location: {_report.GetPath()}");
                        break;
                    case "6":
                        await _report.Save();
                        Console.WriteLine("\nSaved");
                        return;
                    default:
                        Console.WriteLine("\nInvalid option");
                        break;
                }

                Console.WriteLine("\nPress any key...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("Auth Testing Framework");

            // Show session status
            var session = _authManager.GetSession();
            if (session != null && session.IsActive)
            {
                Console.WriteLine($"\n✓ Logged in: {session.UserName} ({session.UserId})");
                Console.WriteLine($"  Session: {session.GetMaskedToken()}");
                Console.WriteLine($"  Login Time: {session.LoginTime:HH:mm:ss}");
            }
            else
            {
                Console.WriteLine("\n○ Not logged in");
            }

            Console.WriteLine();
            Console.WriteLine("1. Normal Login Flow");
            Console.WriteLine("2. Session Validation");
            Console.WriteLine("3. Forgot Password");
            Console.WriteLine("4. Save Report");
            Console.WriteLine("5. Show Report Path");
            Console.WriteLine("6. Exit & Save Report");
            Console.WriteLine();
            Console.Write("Choose: ");
        }

        private async Task RunScenarioA()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();
            //string uid = "47054457";

            Console.Write("Password: ");
            string pwd = Console.ReadLine();
            //string pwd = "Uat@47054457";

            await _scenarios.RunNormalLogin(uid, pwd);
        }

        private async Task RunScenarioB()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();

            Console.Write("Password: ");
            string pwd = Console.ReadLine();

            await _scenarios.RunSessionValidation(uid, pwd);
        }

        private async Task RunScenarioC()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();
            //string uid = "47054457";

            Console.Write("New Password: ");
            string newPwd = Console.ReadLine();

            await _scenarios.RunForgotPassword(uid, "", newPwd);
        }
    }
}
