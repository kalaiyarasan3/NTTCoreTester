using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System.Text;

namespace NTTCoreTester.Reporting
{
    //public interface ICsvReport
    //{
    //    void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
    //                  string jsonResponse, bool schemaValid, string validationErrors);
    //    Task Save();
    //    string GetPath();
    //}

    public class CsvReport 
    {
        private readonly ReportConfig _cfg;
        private readonly List<CsvReportEntry> _entries;
        private readonly string _filename;
        private readonly string _fullPath;
        private const int PERFORMANCE_THRESHOLD_MS = 100;

        public CsvReport(ReportConfig cfg)
        {
            _cfg = cfg;
            _entries = new List<CsvReportEntry>();

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _filename = $"{_cfg.FilePrefix}_{timestamp}.csv";

            if (!Directory.Exists(_cfg.OutputFolder))
                Directory.CreateDirectory(_cfg.OutputFolder);

            _fullPath = Path.Combine(_cfg.OutputFolder, _filename);
        }

        public void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
                            string jsonResponse, bool schemaValid, string validationErrors)
        {
            var entry = new CsvReportEntry
            {
                Timestamp = DateTime.Now,
                Endpoint = endpoint,
                ResponseTimeMs = responseMs,
                PerformanceStatus = responseMs <= PERFORMANCE_THRESHOLD_MS ? "PASS" : "FAIL",
                HttpStatusCode = httpCode,
                BusinessStatus = businessStatus,
                JsonResponse = jsonResponse,
                SchemaValid = schemaValid,
                ValidationErrors = validationErrors ?? ""
            };

            _entries.Add(entry);
        }

        public async Task Save()
        {
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Timestamp,Endpoint,ResponseTimeMs,PerformanceStatus,HttpStatusCode,BusinessStatus,SchemaValid,ValidationErrors,JsonResponse");

            foreach (var entry in _entries)
            {
                string escapedJson = EscapeForCsv(entry.JsonResponse);
                string escapedErrors = EscapeForCsv(entry.ValidationErrors);

                sb.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                             $"{entry.Endpoint}," +
                             $"{entry.ResponseTimeMs}," +
                             $"{entry.PerformanceStatus}," +
                             $"{entry.HttpStatusCode}," +
                             $"{entry.BusinessStatus}," +
                             $"{(entry.SchemaValid ? "VALID" : "INVALID")}," +
                             $"\"{escapedErrors}\"," +
                             $"\"{escapedJson}\"");
            }

            await File.WriteAllTextAsync(_fullPath, sb.ToString());
            Console.WriteLine($"\n CSV Report saved: {_fullPath}");
            Console.WriteLine($"   Total Entries: {_entries.Count}");
            Console.WriteLine($"   Performance Threshold: {PERFORMANCE_THRESHOLD_MS}ms");
        }

        public string GetPath()
        {
            return _fullPath;
        }

        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("\"", "\"\"")
                       .Replace("\r\n", " ")
                       .Replace("\n", " ")
                       .Replace("\r", " ")
                       .Replace("\t", " ");
        }
    }
}
