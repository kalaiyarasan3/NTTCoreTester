using NTTCoreTester.Application.Features.Orders.Request;
using NTTCoreTester.Application.Features.Orders.ViewModels;

namespace NTTCoreTester.Application.Repositories
{
    public interface IOrderService
    {
        Task<OrderInfoResponse> GetLastOrderStatusAsync(LastOrderRequest request);
        Task<OrderMarginResponse> GetOrderMarginAsync(OrderMarginRequest request);
        Task<SecurityInfoResponse> GetSecurityInfoAsync(GetSecurityInfoRequest request);
    }
}