using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Helper
{
    public static class Constants
    {
        public const string ClientOrdId = "cl_ord_id";
       // public const string ClientOrdIdTradeBook = "ClientOrdId";
        public const string OrderNumber = "ordno";
        public const string TotalQuantity = "Total_Quantity";
        public const string ShouldBlockMargin = "ShouldBlockMargin";
        public const string SUserToken = "token";
        public const string GetOrderMargin = "GetOrderMargin";
        public const string PreviousOrderMargin = "PreviousOrderMargin";
        public const string PreLimitMargin = "PreLimitMargin";
        public const string PostLimitMargin = "PostLimitMargin";
        public const string UId = "uid";
        public const string UName = "uname";
        public const string PrePositions = "PrePositions";
        public const string PostPositions = "PostPositions";
        public const string FilledQty = "FilledQty";
        public const string OrderSymbol = "OrderSymbol";
        public const string OrderProduct = "OrderProduct";
        public const string OrderSide = "OrderSide";

        public const string  PlaceOrderTime = "PlaceOrderTime";
        public const string OrderBookAddedOn = "OrderBookAddedOn";


        // Common response keys
        public const string StatusCode = "StatusCode";
        public const string Message = "Message";
        public const string AllMargins = "AllMargins";
        public const string TemplateId = "TemplateId";

        // Report related constants
        public const string SCHEMA_FAILED = "SCHEMA_FAILED";
        public const string NOT_JSON = "NOT_JSON";
        public const string HTTP_FAILED = "HTTP_FAILED";
        public const string INVALID_JSON = "INVALID_JSON";



    }
}
