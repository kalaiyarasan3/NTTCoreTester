using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ResponseMessage:IActivityHandler
    {

        public ResponseMessage() { }

        public string Name=> nameof(ResponseMessage);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            
        }
    }
}
