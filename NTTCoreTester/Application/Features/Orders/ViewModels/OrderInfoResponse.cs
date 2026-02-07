namespace NTTCoreTester.Application.Features.Orders.ViewModels
{
    // Use this as T in ApiResponse<T> for order status/info endpoints
    public class OrderInfoResponse
    {
        public string OrderRequestId { get; set; }
        public bool reconcilation { get; set; }
        public int ActivityId { get; set; }
        public long OrderId { get; set; }              
        public string cl_ord_id { get; set; }           
        public string OriginalClientOrderId { get; set; }
        public string NewClientOrderId { get; set; }
        public string uid { get; set; }
        public string actid { get; set; }
        public bool pro { get; set; }
        public string exch { get; set; }               
        public string Segment { get; set; }            
        public string TerminalInfo { get; set; }
        public string ParticipantCode { get; set; }
        public string CTCLId { get; set; }
        public string ClientType { get; set; }
        public string RoleName { get; set; }
        public string tsym { get; set; }
        public decimal prc { get; set; }
        public int orgqty { get; set; }
        public int qty { get; set; }
        public int tqty { get; set; }
        public string mkt_protection { get; set; }
        public string prd { get; set; }                  
        public string s_prdt_ali { get; set; }
        public string status { get; set; }             
        public string ExchangeOrderStatus { get; set; }
        public string exchsts { get; set; }              
        public string trantype { get; set; }          
        public string prctyp { get; set; }            
        public int fillshares { get; set; }
        public decimal avgprc { get; set; }
        public string rejreason { get; set; }
        public string ordno { get; set; }
        public int cancelqty { get; set; }
        public string remarks { get; set; }
        public int dscqty { get; set; }
        public decimal trgprc { get; set; }
        public string ret { get; set; }                
        public decimal bpprc { get; set; }
        public decimal blprc { get; set; }
        public decimal trailprc { get; set; }
        public string amo { get; set; }
        public decimal pp { get; set; }
        public decimal ti { get; set; }
        public decimal ls { get; set; }
        public string token { get; set; }
        public string tm { get; set; }
        public string ordenttm { get; set; }
        public string exch_tm { get; set; } 
        public ExtraOrderInfos ExtraOrderInfos { get; set; }
        public object OrderTradeInfos { get; set; }
        public object ReportFilter { get; set; }
    }

    public class ExtraOrderInfos
    {
        public string ExchangeSegment { get; set; }
        public string token { get; set; }
        public int SegmentId { get; set; }
        public decimal OpenIntrestClose { get; set; }
        public decimal OpenIntrest { get; set; }
        public decimal StrikePrice { get; set; }
        public string ExpiryDateString { get; set; }
        public string ExpiryDate { get; set; }
        public decimal DPRFrom { get; set; }
        public decimal DPRTo { get; set; }
        public decimal Volume { get; set; }
        public decimal LTP { get; set; }
        public int Lot { get; set; }
        public int Multiplier { get; set; }
        public string BaseSymbolId { get; set; }
        public string BaseSymbol { get; set; }
        public string SCStage { get; set; }
        public string OptionType { get; set; }
        public string InstrumentName { get; set; }
        public decimal OrderValue { get; set; }
        public string ISIN { get; set; }
        public string PAN { get; set; }
        public string StockName { get; set; }
        // ... other fields if needed
    }
}