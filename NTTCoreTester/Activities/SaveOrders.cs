using Newtonsoft.Json;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;


namespace NTTCoreTester.Activities
{
    public class SaveOrders : IActivityHandler
    {
        private readonly PlaceholderCache _cache;

        public SaveOrders(PlaceholderCache cache)
        {
            _cache = cache;
        }

        public string Name => nameof(SaveOrders);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {

            if (string.IsNullOrEmpty(result.ResponseBody))
                return "SaveOrders: ResponseBody is null".FailWithLog();

            var response = JsonConvert.DeserializeObject<Response>(result.ResponseBody);

            if (response?.ResponceDataObject?.AllOrders == null)
                return "SaveOrders: AllOrders not found".FailWithLog();

            var key = _cache.Get<string>(Constants.ClientOrdId);

            var order = response.ResponceDataObject.AllOrders
                .Where(o => o.ClientOrderId == key
                         || o.NewClientOrderId == key
                         || o.OriginalClientOrderId == key)
                .OrderByDescending(o => o.OrderId)
                .FirstOrDefault();

            if (order == null)
                return $"SaveOrders: cl_ord_id={key} not found in OrderBook".FailWithLog();

            Console.WriteLine($"AddedOn from deserialized response: {order.AddedOn}");

            _cache.Set(Constants.OrderSymbol, order.TypeSymbol);
            _cache.Set(Constants.OrderProduct, order.Product);
            _cache.Set(Constants.OrderSide, order.TransactionType);
            _cache.Set(Constants.TotalQuantity, order.Quantity);
            _cache.Set(Constants.OrderBookAddedOn, order.AddedOn);

            var log = $"SaveOrders: tsym={order.TypeSymbol} | qty={order.Quantity} | prd={order.Product} | trantype={order.TransactionType}";
            log.Info();

            return ActivityResult.Success(log);
        }
    }
}
