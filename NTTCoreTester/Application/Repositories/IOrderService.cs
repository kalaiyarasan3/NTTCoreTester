
using NTTCoreTester.Application.Shared.Models;

namespace NTTCoreTester.Application.Repositories
{
    public interface IOrderService
    {
        Task<ApiResult> CancelOrderAsync();
        Task<ApiResult> GetLastOrderStatusAsync();
        Task<ApiResult> GetOrderMarginAsync();
        Task<ApiResult> GetSecurityInfoAsync();
        Task<ApiResult> GetUserInfoAsync();
        Task<ApiResult> ModifyOrderAsync();
        Task<ApiResult> PlaceOrderAsync();
    }
}