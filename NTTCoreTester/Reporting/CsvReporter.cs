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

        public void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
                            string jsonResponse, bool schemaValid, string validationErrors,string? meaaage = null)
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
                ValidationErrors = validationErrors ?? "",
                Message = meaaage
            };

            _entries.Add(entry);
        }

        public void AddSyncEntry(
            string endpoint, long responseMs, int httpCode, string businessStatus,
            string jsonResponse, bool schemaValid, string validationErrors, string? message,
            string? syncFieldMismatches, string? ordenttmRaw,
            long? placeOrderToOrderBookMs, long? placeOrderToActivityBookMs, long? placeOrderToExchangeMs,
            string? exchangeStatus, string? orderActivityStatus)
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
                ValidationErrors = validationErrors ?? "",
                Message = message,
                SyncFieldMismatches = syncFieldMismatches,
                OrdenttmRaw = ordenttmRaw,
                PlaceOrderToOrderBookMs = placeOrderToOrderBookMs,
                PlaceOrderToActivityBookMs = placeOrderToActivityBookMs,
                PlaceOrderToExchangeMs = placeOrderToExchangeMs,
                ExchangeStatus = exchangeStatus,
                OrderActivityStatus = orderActivityStatus
            };

            _entries.Add(entry);
        }

        public async Task Save()
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                "Timestamp,Endpoint,ResponseTimeMs,PerformanceStatus,HttpStatusCode,Message," +
                "BusinessStatus,SchemaValid,ValidationErrors," +
                "SyncFieldMismatches,OrdenttmRaw," +
                "PlaceOrderToOrderBookMs,PlaceOrderToActivityBookMs,PlaceOrderToExchangeMs," +
                "ExchangeStatus,OrderActivityStatus," +
                "JsonResponse");

            foreach (var entry in _entries)
            {
                sb.AppendLine(
                    $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                    $"{entry.Endpoint}," +
                    $"{entry.ResponseTimeMs}," +
                    $"{entry.PerformanceStatus}," +
                    $"{entry.HttpStatusCode}," +
                    $"{EscapeForCsv(entry.Message ?? "")}," +
                    $"{entry.BusinessStatus}," +
                    $"{(entry.SchemaValid ? "VALID" : "INVALID")}," +
                    $"{EscapeForCsv(entry.ValidationErrors)}," +
                    $"{EscapeForCsv(entry.SyncFieldMismatches ?? "")}," +
                    $"{EscapeForCsv(entry.OrdenttmRaw ?? "")}," +
                    $"{entry.PlaceOrderToOrderBookMs?.ToString() ?? ""}," +
                    $"{entry.PlaceOrderToActivityBookMs?.ToString() ?? ""}," +
                    $"{entry.PlaceOrderToExchangeMs?.ToString() ?? ""}," +
                    $"{entry.ExchangeStatus ?? ""}," +
                    $"{entry.OrderActivityStatus ?? ""}," +
                    $"{EscapeForCsv(entry.JsonResponse)}");
            }

            await File.WriteAllTextAsync(_fullPath, sb.ToString());
            Console.WriteLine($"\n CSV Report saved: {_fullPath}");
            Console.WriteLine($"   Total Entries: {_entries.Count}");
            Console.WriteLine($"   Performance Threshold: {PERFORMANCE_THRESHOLD_MS}ms");
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
