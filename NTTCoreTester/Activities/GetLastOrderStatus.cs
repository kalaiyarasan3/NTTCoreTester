using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Enums;
using NTTCoreTester.Models;


namespace NTTCoreTester.Activities
{
    public class GetLastOrderStatus : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public GetLastOrderStatus(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => nameof(GetLastOrderStatus);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "ResponseDataObject not found".FailWithLog(true);

            var ordersToken = dataObject["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog(true);

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var key = _cache.Get<string>(Constants.ClientOrdId);

            var relatedOrders = orders?
                .Where(x => x.ClientOrderId == key)
                .ToList();

            if (relatedOrders == null || !relatedOrders.Any())
                return $"Order {key} not found".FailWithLog(true);

            var pendingOrder = relatedOrders.FirstOrDefault(x => x.OrderStatus is OrderEnumStatus.Pending);

            var orderToUse = pendingOrder ?? relatedOrders.First();

            _cache.Set(Constants.OrderNumber, pendingOrder?.OrderNumber ?? orderToUse.OrderNumber);
            _cache.Set(Constants.TotalQuantity, pendingOrder?.Quantity ?? orderToUse.Quantity);
            _cache.Set(Constants.OrderSymbol, orderToUse.TypeSymbol);
            _cache.Set(Constants.OrderProduct, orderToUse.Product);
            _cache.Set(Constants.OrderSide, orderToUse.TransactionType);


            if (pendingOrder == null)
            {
                if (relatedOrders.Any(x => x.Status == "1118" || x.Status == "0000"))
                {
                    $"Set block margin true".Warn();
                    _cache.Set(Constants.ShouldBlockMargin, true);
                }
                var status = string.Join(" | ",
                    relatedOrders
                        .Select(x => $"[{x.Status}] {x.Remarks ?? "No Remarks"}")
                        .Distinct());

                return $"1111 not found. Current status: {status}"
                    .FailWithLog(false);
            }

            var log = $"Product: {orderToUse.Product} | type: {orderToUse.TransactionType}, symbol: {orderToUse.TypeSymbol} | qty: {pendingOrder?.Quantity} | ordno: {pendingOrder?.OrderNumber} | remarks: {pendingOrder?.Remarks}"; log.Warn();

            _cache.Set(Constants.ShouldBlockMargin, true);
            return ActivityResult.Success(log);
        }
    }

}

/*

Login 
Check Login
Get Security Info
Limits
Place Order
Check Order Status
Modify Order
place order with new client order id
cancel order
Square off order
i already have this handlers 
prelimits 
prepositions
place order
post limits
post positions book (here save the post positions and  in cache and use it after square off)
pre limits 
square off
post limits
post positions (validate the position is updated as expected after square off)
*************

so place buy order
check trade fill
check positions
check margin
now place sell order with same symbol
check trade fill
check positions
check margin

i found one  thing if i square an order it will 
next call activity book to get find the clientordid
then call TradeBook ExtractTradeFill acivity to find the trade fill for the square off order
then call PositionBook  ValidatePostPositions activity to validate the position is updated as expected after square off
then call ExtractPostLimitMargin activity to validate the margin is updated as expected after square off
here is a one caught 
positon net qty: 20 (placed square off but in pending status)
place order to buy 5 qty
position net qty should be 25 now but if i call square off all 
5 qty send to squareoff or 25 qty send to square off because order is in pending 
status and not filled yet so position is not updated 
yet so it will send 25 qty to 
square off instead of 5 qty which is wrong because only 5 qty is 
pending to fill rest 20 qty is already there in position book so we need to 
consider the pending order qty also while calculating the square off qty

 */