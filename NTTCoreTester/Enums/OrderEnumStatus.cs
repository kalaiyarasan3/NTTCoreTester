namespace NTTCoreTester.Enums
{
    public enum OrderEnumStatus
    {
        // ACTIVE / MARGIN BLOCKING
        ORDER_RECEIVED = 0000,
        ORDER_PENDING = 1111,        
        ORDER_TRADED = 1118,
        ORDER_MODIFIED = 1113,

        // FINAL STATES (NO BLOCK)
        ORDER_CANCELLED = 1115,
        ORDER_REJECTED = 1119,
        RMS_PENDING = 1120,
        RMS_ORDER_REJECTED = 0001,
        NSE_ADAPTOR_REJECTION = 1121,
        NOT_FOUND = 1116,
        TRANSACTION_NOT_ALLOWED = 1112
    }
    public static class OrderStatusHelper
    { 
        public static OrderEnumStatus ToOrderStatus(this string? statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
                return OrderEnumStatus.NOT_FOUND; 

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
            return OrderEnumStatus.NOT_FOUND; // or throw new ArgumentException(...)
        }
    }    

}
