using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Helper
{
    public class VariableManager
    {
        private readonly Dictionary<string, string> _sessionVars =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _orderVars =
            new(StringComparer.OrdinalIgnoreCase);

        // ======================
        // SESSION VARIABLES
        // ======================

        public void SetSession(string name, string value)
            => _sessionVars[name] = value;

        public string? GetSession(string name)
            => _sessionVars.TryGetValue(name, out var v) ? v : null;

        // ======================
        // ORDER VARIABLES
        // ======================

        public void SetOrder(string name, string value)
            => _orderVars[name] = value;

        public string? GetOrder(string name)
            => _orderVars.TryGetValue(name, out var v) ? v : null;
        public void RemoveOrder(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            _orderVars.Remove(name);
        }


        public void ClearOrderContext()
            => _orderVars.Clear();

        // ======================
        // TEMPLATE RESOLUTION
        // ======================

        public string ReplaceVariables(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Order vars override session vars
            foreach (var kv in _sessionVars)
                text = text.Replace($"{{{{{kv.Key}}}}}", kv.Value);

            foreach (var kv in _orderVars)
                text = text.Replace($"{{{{{kv.Key}}}}}", kv.Value);

            if (text.Contains("{{"))
                throw new Exception("Unresolved variables found in JSON");

            return text;
        }
    }

}
