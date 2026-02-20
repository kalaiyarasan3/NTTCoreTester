using CsvHelper;
using CsvHelper.Configuration;
using NTTCoreTester.Configuration;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Models;
using System.Globalization;
using System.Text;

namespace NTTCoreTester.Reporting
{

	public class CsvReport
	{
		private readonly ReportConfig _cfg;
		private readonly CsvService _csvService;
		private readonly List<CsvReportEntry> _entries;
		private readonly string _filename;
		private readonly string _fullPath;
		private const int PERFORMANCE_THRESHOLD_MS = 100;

		public CsvReport(ReportConfig cfg, CsvService csvService)
		{
			_cfg = cfg;
			_csvService = csvService;
			_entries = new List<CsvReportEntry>();

			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			_filename = $"{_cfg.FilePrefix}_{timestamp}.csv";

			if (!Directory.Exists(_cfg.OutputFolder))
				Directory.CreateDirectory(_cfg.OutputFolder);

			_fullPath = Path.Combine(_cfg.OutputFolder, _filename);
		}

		public void AddEntry(string endpoint, long responseMs, int httpCode, string businessStatus,
							string jsonResponse, bool schemaValid, string validationErrors, string? meaaage = null)
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

		 

		public async Task Save()
		{
			try
			{
				await _csvService.WriteCsv<CsvReportEntry>(_entries, _fullPath);
			}
			catch (Exception ex)
			{
				$"Error saving csv {ex}".Error();
				throw;
			}
			Console.WriteLine($"\n CSV Report saved: {_fullPath}");
			Console.WriteLine($"   Total Entries: {_entries.Count}");
			Console.WriteLine($"   Performance Threshold: {PERFORMANCE_THRESHOLD_MS}ms");
		}
		 
	}
}
