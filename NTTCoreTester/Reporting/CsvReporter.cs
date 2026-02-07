using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Reporting
{
    public interface ICsvReport
    {
        void Add(TestResult result);
        Task Save();
        string GetPath();
    }

    public class CsvReport : ICsvReport
    {
        private readonly ReportConfig _cfg;
        private readonly List<TestResult> _results;
        private readonly string _filename;
        private readonly string _fullPath;

        public CsvReport(ReportConfig cfg)
        {
            _cfg = cfg;
            _results = new List<TestResult>();

            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _filename = $"{_cfg.FilePrefix}_{timestamp}.csv";

            if (!Directory.Exists(_cfg.OutputFolder))
                Directory.CreateDirectory(_cfg.OutputFolder);

            _fullPath = Path.Combine(_cfg.OutputFolder, _filename);
        }

        public void Add(TestResult result)
        {
            _results.Add(result);
        }

        public async Task Save()
        {
            var sb = new StringBuilder();

            
            sb.AppendLine("Timestamp,Module,Scenario,API,Status,ResponseMs,ValidJson,Error,HttpCode");

            
            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Time:yyyy-MM-dd HH:mm:ss}," +
                             $"{r.Module}," +
                             $"{r.Scenario}," +
                             $"{r.Api}," +
                             $"{r.Result}," +
                             $"{r.ResponseMs}," +
                             $"{(r.ValidJson ? "YES" : "NO")}," +
                             $"\"{r.Error.Replace("\"", "\"\"")}\"," +
                             $"{r.HttpCode}");
            }

            await File.WriteAllTextAsync(_fullPath, sb.ToString());
            Console.WriteLine($"\n✓ Report saved: {_fullPath}");
        }

        public string GetPath()
        {
            return _fullPath;
        }
    }
}
