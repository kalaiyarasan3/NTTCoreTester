using NTTCoreTester.Application.Features.Orders.Request;
using NTTCoreTester.Application.Features.Orders.ViewModels;

namespace NTTCoreTester.Application.Repositories
{
    public interface IOrderService
    {
        Task<CancelOrderResponse> CancelOrderAsync();
        Task<OrderInfoResponse> GetLastOrderStatusAsync(LastOrderRequest request);
        Task<OrderMarginResponse> GetOrderMarginAsync(OrderMarginRequest request);
        Task<SecurityInfoResponse> GetSecurityInfoAsync(GetSecurityInfoRequest request);
        Task<ModifyOrderResponse> ModifyOrderAsync();
        Task<PlaceOrderResponse> PlaceOrderAsync();
        Task<SingleOrdHistResponse> SingleOrdHistAsync();
    }
}