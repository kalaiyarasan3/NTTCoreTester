using CsvHelper;
using CsvHelper.Configuration;
using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System.Globalization;
using System.Text;

namespace NTTCoreTester.Reporting
{

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

        public void AddEntry(string endpoint, long responseTime, int statusCode, string businessStatus, string jsonResponse, bool schemaValid, string validationErrors, string? message, string? activityMessage)   
        {
            const int threshold = 100;

            _entries.Add(new CsvReportEntry
            {
                Endpoint = endpoint,
                ResponseTimeMs = responseTime,
                PerformanceStatus = responseTime <= threshold ? "PASS" : "FAIL",
                HttpStatusCode = statusCode,
                BusinessStatus = businessStatus,
                JsonResponse = jsonResponse,
                SchemaValid = schemaValid,
                ValidationErrors = validationErrors,
                Message = message,                
                ActivityMessage = activityMessage 
            });
        }


        //public async Task Save()
        //{
        //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        //    {
        //        HasHeaderRecord = true,
        //    };

        //    using (var writer = new StreamWriter(_fullPath))
        //    using (var csv = new CsvWriter(writer, config))
        //    {
        //        // Write records directly - CsvHelper handles all escaping
        //        await csv.WriteRecordsAsync(_entries);
        //    }

        //    Console.WriteLine($"\n CSV Report saved: {_fullPath}");
        //    Console.WriteLine($"   Total Entries: {_entries.Count}");
        //    Console.WriteLine($"   Performance Threshold: {PERFORMANCE_THRESHOLD_MS}ms");
        //}

        public async Task Save()
        {
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Timestamp,Endpoint,ResponseTimeMs,PerformanceStatus,HttpStatusCode,Message,ActivityMessage,BusinessStatus,SchemaValid,ValidationErrors,JsonResponse");
            foreach (var entry in _entries)
            {
                string escapedJson = EscapeForCsv(entry.JsonResponse);
                string escapedErrors = EscapeForCsv(entry.ValidationErrors);
                string escapedMessage = EscapeForCsv(entry.Message ?? "");
                string escapedactivityMessage = EscapeForCsv(entry.ActivityMessage ?? "");

                sb.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                             $"{entry.Endpoint}," +
                             $"{entry.ResponseTimeMs}," +
                             $"{entry.PerformanceStatus}," +
                             $"{entry.HttpStatusCode}," +
                             $"{escapedMessage}," +
                             $"{escapedactivityMessage}," +
                             $"{entry.BusinessStatus}," +
                             $"{(entry.SchemaValid ? "VALID" : "INVALID")}," +
                             $"{escapedErrors}," +
                             $"{escapedJson}");
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

            value = value.Replace("\r\n", " ")
                         .Replace("\n", " ")
                         .Replace("\r", " ")
                         .Replace("\t", " ");

            value = value.Replace("\"", "\"\"");

            return $"\"{value}\"";
        }



    }
}
