using NTTCoreTester.Reporting;
using NTTCoreTester.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.UI
{
    public class Menu
    {
        private readonly ITestScenarios _scenarios;
        private readonly ICsvReport _report;

        public Menu(ITestScenarios scenarios, ICsvReport report)
        {
            _scenarios = scenarios;
            _report = report;
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
                        Console.WriteLine("\nBye!");
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
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  Auth Testing Framework              ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("1. Normal Login Flow");
            Console.WriteLine("2. Session Validation");
            Console.WriteLine("3. Forgot Password");
            Console.WriteLine("4. Save Report");
            Console.WriteLine("5. Show Report Path");
            Console.WriteLine("6. Exit");
            Console.WriteLine();
            Console.Write("Choose: ");
        }

        private async Task RunScenarioA()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();

            Console.Write("Password: ");
            string pwd = GetPassword();

            await _scenarios.RunNormalLogin(uid, pwd);
        }

        private async Task RunScenarioB()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();

            Console.Write("Password: ");
            string pwd = GetPassword();

            await _scenarios.RunSessionValidation(uid, pwd);
        }

        private async Task RunScenarioC()
        {
            Console.Write("\nUser ID: ");
            string uid = Console.ReadLine();

            Console.Write("Login Token: ");
            string token = Console.ReadLine();

            Console.Write("New Password: ");
            string newPwd = GetPassword();

            await _scenarios.RunForgotPassword(uid, token, newPwd);
        }

        // simple password masking
        private string GetPassword()
        {
            string pwd = "";
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pwd.Length > 0)
                {
                    Console.Write("\b \b");
                    pwd = pwd.Substring(0, pwd.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pwd += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return pwd;
        }
    }
}
