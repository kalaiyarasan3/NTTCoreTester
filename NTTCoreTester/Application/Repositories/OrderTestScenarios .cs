using NTTCoreTester.Application.Helper;

namespace NTTCoreTester.Application.Repositories
{
    public class OrderTestScenarios(
        IOrderService orderService,
        VariableManager variableManager) : IOrderTestScenarios
    {
        private readonly IOrderService _orderService = orderService;
        private readonly VariableManager _variableManager = variableManager;

        

        
    }
}