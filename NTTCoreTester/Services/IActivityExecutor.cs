using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Services
{
    public interface IActivityExecutor
    {
        bool Execute(string methodName, string response, string endpoint);
    }
}
