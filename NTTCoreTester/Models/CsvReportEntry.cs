using CsvHelper.Configuration;

namespace NTTCoreTester.Models
{
    public class CsvReportEntry
    {
        public DateTime Timestamp { get; set; }
        public string Endpoint { get; set; }
        public string ActivityName { get; set; }
        public long ResponseTimeMs { get; set; }
        public int HttpStatusCode { get; set; }
        public string BusinessStatus { get; set; }     // SUCCESS/FAILED based on StatusCode
        public string Remarks { get; set; }
        public string JsonResponse { get; set; }
        public bool SchemaValid { get; set; }
        public string ValidationErrors { get; set; }
        public string? Message { get; set; }

        public string? SyncFieldMismatches { get; set; }        
        public string? OrdenttmRaw { get; set; }
        public string? PlaceorderTime { get; set; }
        public string? OrderBookTime { get; set; }
        public string? ActivityOrderBookTime { get; set; }  
        public string? PlaceOrderToOrderBookMs { get; set; }    
        public string? PlaceOrderToActivityBookMs { get; set; }   
        public string? PlaceOrderToExchangeMs { get; set; }      
        public string? ExchangeStatus { get; set; }            
        public string? OrderActivityStatus { get; set; }



        public CsvReportEntry()
        {
            Timestamp = DateTime.Now;
            Endpoint = "";
            JsonResponse = "";
            ValidationErrors = "";
            BusinessStatus = "UNKNOWN";
            ActivityName = "";
            Remarks = "";
        }
    }

    public sealed class CsvReportEntryMap : ClassMap<CsvReportEntry>
    {
        public CsvReportEntryMap()
        {
            Map(m => m.Timestamp);
            Map(m => m.Endpoint);
            Map(m => m.ActivityName);
            Map(m => m.ResponseTimeMs);
            Map(m => m.HttpStatusCode);
            Map(m => m.Message);
            Map(m => m.BusinessStatus);
            Map(m => m.Remarks);
            Map(m => m.SchemaValid);
            Map(m => m.ValidationErrors);
            Map(m => m.SyncFieldMismatches);
            Map(m => m.OrdenttmRaw);
            Map(m => m.PlaceorderTime);
            Map(m => m.OrderBookTime);
            Map(m=>m.ActivityOrderBookTime);
            Map(m => m.PlaceOrderToOrderBookMs);
            Map(m => m.PlaceOrderToActivityBookMs);
            Map(m => m.PlaceOrderToExchangeMs);
            Map(m => m.ExchangeStatus);
            Map(m => m.OrderActivityStatus);
            Map(m => m.JsonResponse);
        }
    }
}
