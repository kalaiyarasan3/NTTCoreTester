using NTTCoreTester.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public interface IActivityHandler
    {
        string Name { get; }
        ActivityResult Execute(ApiExecutionResult result, string endpoint);
    }

}
