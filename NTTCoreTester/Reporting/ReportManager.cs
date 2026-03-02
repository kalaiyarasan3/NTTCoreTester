using NTTCoreTester.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Reporting
{
    public class ReportManager
    {
        private readonly CsvReport _csv;
        private readonly HtmlReport _html;

        public ReportManager(ReportConfig cfg)
        {
            _csv = new CsvReport(cfg);
            _html = new HtmlReport(cfg);
        }


        public void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
                             string jsonResponse, bool schemaValid, string validationErrors, string? message = null)
        {
            _csv.AddEntry(endpoint, responseMs, httpCode, businessStatus,
                          jsonResponse, schemaValid, validationErrors, message);
            _html.AddEntry(endpoint, responseMs, httpCode, businessStatus,
                           jsonResponse, schemaValid, validationErrors, message);
        }

        public async Task SaveAll()
        {
            await Task.WhenAll(
                _csv.Save(),
                _html.Save()
            );
        }

        public string GetCsvPath() => _csv.GetPath();
        public string GetHtmlPath() => _html.GetPath();

    }
}
