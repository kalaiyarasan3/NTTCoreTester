namespace NTTCoreTester.Models
{
    public class CsvReportEntry
    {
        public DateTime Timestamp { get; set; }
        public string Endpoint { get; set; }
        public long ResponseTimeMs { get; set; }
        public string PerformanceStatus { get; set; }  // PASS/FAIL based on 100ms threshold
        public int HttpStatusCode { get; set; }
        public string BusinessStatus { get; set; }     // SUCCESS/FAILED based on StatusCode
        public string JsonResponse { get; set; }
        public bool SchemaValid { get; set; }
        public string ValidationErrors { get; set; }

        public CsvReportEntry()
        {
            Timestamp = DateTime.Now;
            Endpoint = "";
            JsonResponse = "";
            ValidationErrors = "";
            PerformanceStatus = "FAIL";
            BusinessStatus = "UNKNOWN";
        }
    }
}
