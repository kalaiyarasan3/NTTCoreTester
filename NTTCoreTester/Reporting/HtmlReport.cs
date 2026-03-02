using NTTCoreTester.Configuration;
using NTTCoreTester.Models;
using System.Net;
using System.Text;

namespace NTTCoreTester.Reporting
{
    public class HtmlReport
    {
        private readonly ReportConfig _cfg;
        private readonly List<CsvReportEntry> _entries;
        private readonly string _filename;
        private readonly string _fullPath;
        private const int PERFORMANCE_THRESHOLD_MS = 100;

        public HtmlReport(ReportConfig cfg)
        {
            _cfg = cfg;
            _entries = new List<CsvReportEntry>();

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _filename = $"{_cfg.FilePrefix}_{timestamp}.html";

            if (!Directory.Exists(_cfg.OutputFolder))
                Directory.CreateDirectory(_cfg.OutputFolder);

            _fullPath = Path.Combine(_cfg.OutputFolder, _filename);
        }

        public void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
                             string jsonResponse, bool schemaValid, string validationErrors, string? message = null)
        {
            _entries.Add(new CsvReportEntry
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
                Message = message
            });
        }

        public async Task Save()
        {
            await File.WriteAllTextAsync(_fullPath, BuildHtml(), Encoding.UTF8);
            Console.WriteLine($"\n HTML Report saved: {_fullPath}");
            Console.WriteLine($"   Total Entries   : {_entries.Count}");
            Console.WriteLine($"   Perf Threshold  : {PERFORMANCE_THRESHOLD_MS}ms");
        }

        public string GetPath() => _fullPath;

        private string PrettyJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "";

            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return json; 
            }
        }


        private string BuildHtml()
        {
            int total = _entries.Count;
            int schemaFail = _entries.Count(e => !e.SchemaValid);
            int perfFail = _entries.Count(e => e.PerformanceStatus == "FAIL");
            int fullPass = _entries.Count(e => e.SchemaValid && e.PerformanceStatus == "PASS");

            var rows = new StringBuilder();
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];

                string rowClass = (!e.SchemaValid || e.PerformanceStatus == "FAIL") ? "row-fail" : "row-pass";
                string schemaTag = e.SchemaValid ? "<span class='badge b-valid'>VALID</span>"
                                                 : "<span class='badge b-invalid'>INVALID</span>";
                string perfTag = e.PerformanceStatus == "PASS"
                                 ? "<span class='badge b-pass'>PASS</span>"
                                 : "<span class='badge b-fail'>FAIL</span>";
                string httpTag = e.HttpStatusCode == 200
                                 ? $"<span class='badge b-pass'>{e.HttpStatusCode}</span>"
                                 : $"<span class='badge b-fail'>{e.HttpStatusCode}</span>";

                string errorsHtml = string.IsNullOrWhiteSpace(e.ValidationErrors)
                    ? "<span class='muted'>None</span>"
                    : $"<div class='err-box'>{WebUtility.HtmlEncode(e.ValidationErrors)}</div>";

                rows.Append($@"
        <tr class='{rowClass}'
            data-endpoint='{WebUtility.HtmlEncode(e.Endpoint?.ToLower() ?? "")}'
            data-schema='{(e.SchemaValid ? "VALID" : "INVALID")}'
            data-perf='{e.PerformanceStatus}'
            data-http='{e.HttpStatusCode}'>
          <td class='mono'>{e.Timestamp:yyyy-MM-dd HH:mm:ss}</td>
          <td class='ep'>{WebUtility.HtmlEncode(e.Endpoint ?? "")}</td>
          <td class='mono'>{e.ResponseTimeMs} ms</td>
          <td>{perfTag}</td>
          <td>{httpTag}</td>
          <td>{WebUtility.HtmlEncode(e.Message ?? "")}</td>
          <td>{WebUtility.HtmlEncode(e.BusinessStatus ?? "")}</td>
          <td>{schemaTag}</td>
          <td>{errorsHtml}</td>
          <td>
            <button class='btn-view' onclick='toggle(this,""json{i}"")'>▶ View</button>
            <div id='json{i}' class='json-box' style='display:none'>
              <pre>{WebUtility.HtmlEncode(PrettyJson(e.JsonResponse ?? ""))}</pre>
            </div>
          </td>
        </tr>");
            }

            // Load template and replace placeholders
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                               "Reporting", "ReportTemplate.html");
            string template = File.ReadAllText(templatePath);

            return template
                .Replace("{{FILE_PREFIX}}", WebUtility.HtmlEncode(_cfg.FilePrefix))
                .Replace("{{GENERATED_TIME}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{PERF_THRESHOLD}}", PERFORMANCE_THRESHOLD_MS.ToString())
                .Replace("{{TOTAL}}", total.ToString())
                .Replace("{{FULL_PASS}}", fullPass.ToString())
                .Replace("{{SCHEMA_FAIL}}", schemaFail.ToString())
                .Replace("{{PERF_FAIL}}", perfFail.ToString())
                .Replace("{{TABLE_ROWS}}", rows.ToString());
        }

    }
}
