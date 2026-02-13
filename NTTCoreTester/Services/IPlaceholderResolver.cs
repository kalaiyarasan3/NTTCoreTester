using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Services
{
    public interface IPlaceholderResolver
    {
        Task<string?> ResolvePlaceholders(string template, string endpoint);
        Task<string?> ResolvePlaceholderValue(string value, string endpoint);
    }
}
