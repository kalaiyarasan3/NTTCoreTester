using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Enums
{
    public enum OrderEnumStatus
    {
        Pending = 1111,
        OrderReceived = 0000,
        TRANSACTIONNOTALLOWED = 1112,
        NotFound = 1116,
        Filled = 1118,
        Cancel = 1115,
        RmsRejection = 0001,
        RmsPending = 1120,
        NseAdaptorRejection = 1121
    }
    public static class OrderStatusHelper
    {
        public static bool TryParse(string? status, out OrderEnumStatus orderStatus)
        {
            orderStatus = default;

            if (string.IsNullOrWhiteSpace(status))
                return false;

            if (!int.TryParse(status, out int code))
                return false;

            if (!Enum.IsDefined(typeof(OrderEnumStatus), code))
                return false;

            orderStatus = (OrderEnumStatus)code;
            return true;
        }
    }
    /*
        if (OrderStatusHelper.TryParse(response.Status, out var orderStatus))
        {
            Console.WriteLine($"Order Status Code : {(int)orderStatus}");
            Console.WriteLine($"Order Status Name : {orderStatus}");

            if (orderStatus == OrderEnumStatus.Open)
            {
                Console.WriteLine("Order is currently OPEN");
            }
        }
        else
        {
            Console.WriteLine($"Unknown or Invalid Order Status Received: {response.Status}");
        }
     */

}
