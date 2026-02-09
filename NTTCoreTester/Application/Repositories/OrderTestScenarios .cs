using NTTCoreTester.Application.Features.Orders.Request;
using NTTCoreTester.Application.Helper;
using NTTCoreTester.Application.Repositories;

public class OrderTestScenarios : IOrderTestScenarios
{
    private readonly IOrderService _orderService;
    private readonly VariableManager _variableManager;

    public OrderTestScenarios(
        IOrderService orderService,
        VariableManager variableManager)
    {
        _orderService = orderService;
        _variableManager = variableManager;
    }

    public async Task PlaceModifyCancelAsync()
    {

        Console.Write("Enter qty to PLACE: ");
        var qty = Console.ReadLine();

        _variableManager.SetOrder("qty", qty!);

        var placeResponse = await _orderService.PlaceOrderAsync();

        // Capture order identity
        _variableManager.ClearOrderContext();
        _variableManager.SetOrder("cl_ord_id", placeResponse.ClOrdId);

        // cleanup temp
        _variableManager.RemoveOrder("qty");

        Console.WriteLine($"Order placed: {placeResponse.ClOrdId}");

        // ===== Modify Order =====
        Console.Write("Enter new qty to MODIFY: ");
        var newQty = Console.ReadLine();

        _variableManager.SetOrder("qty", newQty!);
        await _orderService.ModifyOrderAsync();
        _variableManager.RemoveOrder("qty");

        Console.WriteLine("Order modified");

        // ===== Cancel Order =====
        await _orderService.CancelOrderAsync();
        Console.WriteLine("Order cancelled");
    }

    // -------------------------------
    // Scenario 2: Select → Modify
    // -------------------------------
    public async Task SelectAndModifyAsync()
    {
        var orders = await _orderService.GetLastOrderStatusAsync(new LastOrderRequest
        {
            Symbol = "RELIANCE",
            Exchange = "NSE"
        });

        // show list (simplified)
        for (int i = 0; i < orders.Orders.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {orders.Orders[i].ClOrdId}");
        }

        Console.Write("Select order: ");
        var index = int.Parse(Console.ReadLine()!) - 1;

        var selected = orders.Orders[index];

        _variableManager.ClearOrderContext();
        _variableManager.SetOrder("cl_ord_id", selected.ClOrdId);

        Console.Write("Enter new qty: ");
        var qty = Console.ReadLine();

        _variableManager.SetOrder("qty", qty!);
        await _orderService.ModifyOrderAsync();
        _variableManager.RemoveOrder("qty");

        Console.WriteLine("Selected order modified");
    }
}
