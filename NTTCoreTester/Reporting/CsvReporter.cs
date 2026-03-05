using CsvHelper;
using CsvHelper.Configuration;
using NTTCoreTester.Core.Models;
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

        public void AddEntry(string configName,string endpoint,string description, string activityName, long responseMs, int httpCode, string businessStatus,string remarks,
                            string jsonResponse, bool schemaValid, string validationErrors,string? meaaage = null)
        {
            var entry = new CsvReportEntry
            {
                Timestamp = DateTime.Now,
                ConfigName = configName,
                Endpoint = endpoint,
                Description = description,
                ActivityName = activityName,
                ResponseTimeMs = responseMs,
                HttpStatusCode = httpCode,
                BusinessStatus = businessStatus,
                Remarks = remarks,
                JsonResponse = jsonResponse,
                SchemaValid = schemaValid,
                ValidationErrors = validationErrors ?? "",
                Message = meaaage
            };

            _entries.Add(entry);
        }

        public async Task Save()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            await using var writer = new StreamWriter(_fullPath, false, Encoding.UTF8);
            await using var csv = new CsvWriter(writer, config);

            csv.Context.RegisterClassMap<CsvReportEntryMap>();

            await csv.WriteRecordsAsync(_entries);

            Console.WriteLine($"\nCSV Report saved: {_fullPath}");
            Console.WriteLine($"Total Entries: {_entries.Count}");
            Console.WriteLine($"Performance Threshold: {PERFORMANCE_THRESHOLD_MS}ms");
        }

        public string GetPath()
         {
             return _fullPath;
         }

      
    }
}
