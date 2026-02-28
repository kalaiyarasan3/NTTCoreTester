using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;


namespace NTTCoreTester.Activities
{
    public class GetLastOrderStatusHandler : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public GetLastOrderStatusHandler(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => "GetLastOrderStatus";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            var dataObject = result.DataObject;

            if (dataObject == null)
                return "ResponseDataObject not found".FailWithLog();

            var ordersToken = dataObject["AllOrders"];

            if (ordersToken == null || ordersToken.Type != JTokenType.Array)
                return "AllOrders not found".FailWithLog();

            var orders = ordersToken.ToObject<List<OrderDetails>>();

            var key = _cache.Get<string>(Constants.ClientOrdId);
             
            var relatedOrders = orders?
                .Where(x => x.ClientOrderId == key)
                .ToList();

            if (relatedOrders == null || !relatedOrders.Any())
                return $"Order {key} not found".FailWithLog();

            var pendingOrder = relatedOrders
                .FirstOrDefault(x => x.Status == "1111");

            if (pendingOrder == null)
            {
                var statuses = string.Join(" | ",
                    relatedOrders
                        .Select(x => $"[{x.Status}] {x.Remarks ?? "No Remarks"}")
                        .Distinct());

                return $"1111 not found. Current states: {statuses}"
                    .FailWithLog(false);
            }

            _cache.Set(Constants.OrderNumber, pendingOrder.OrderNumber);
            _cache.Set(Constants.TotalQuantity, pendingOrder.Quantity);
            _cache.Set(Constants.OrderSymbol, pendingOrder.TypeSymbol);
            _cache.Set(Constants.OrderProduct, pendingOrder.Product);
            _cache.Set(Constants.OrderSide, pendingOrder.TransactionType);

            $"ordno: {pendingOrder.OrderNumber} qty: {pendingOrder.Quantity} symbol: {pendingOrder.TypeSymbol} product: {pendingOrder.Product} type: {pendingOrder.TransactionType}".Info();

            return ActivityResult.Success(pendingOrder.Remarks ?? "");
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