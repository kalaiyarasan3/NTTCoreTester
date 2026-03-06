namespace NTTCoreTester.Enums
{
    public enum OrderEnumStatus
    {
        ORDER_RECEIVED = 0000,
        RMS_ORDER_REJECTED = 0001,
        ORDER_PENDING = 1111,
        TRANSACTIONNOTALLOWED = 1112,
        ORDER_MODIFIED = 1113,
        ORDER_CANCELLED = 1115,
        NOT_FOUND = 1116,
        ORDER_TRADED = 1118,
        OPEN = 1119,     
        RMS_PENDING = 1120,
        NSE_ADAPTOR_REJECTION = 1121 
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
