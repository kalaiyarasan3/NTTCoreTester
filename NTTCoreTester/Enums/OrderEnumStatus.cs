using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Enums
{
    public enum OrderEnumStatus
    {
        OrderReceived = 0000,
        RmsRejection = 0001,
        Pending = 1111,
        TRANSACTIONNOTALLOWED = 1112,
        Cancel = 1115,
        NotFound = 1116,
        Filled = 1118,
        Open = 1119,     
        RmsPending = 1120,
        NseAdaptorRejection = 1121 
    }
    public static class OrderStatusHelper
    { 
        public static OrderEnumStatus ToOrderStatus(this string? statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return OrderEnumStatus.NotFound; 

            // Remove leading zeros if any (e.g. "0000" → "0", "0001" → "1")
            var cleaned = statusCode.TrimStart('0');
            if (string.IsNullOrEmpty(cleaned))
                cleaned = "0"; // "0000" becomes 0 → OrderReceived

            if (int.TryParse(cleaned, out int code))
            {
                if (Enum.IsDefined(typeof(OrderEnumStatus), code))
                {
                    return (OrderEnumStatus)code;
                }
            }

            // Fallback for unknown codes like "1119", "1100", etc.
            // Log it if needed: _logger.LogWarning("Unknown order status: {Status}", statusCode);
            return OrderEnumStatus.NotFound; // or throw new ArgumentException(...)
        }
    }    

}
