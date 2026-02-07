using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsActive { get; set; }

        
        public string GetMaskedToken()
        {
            return Token; 
        }
    }

    public enum Status
    {
        PASS,
        FAIL,
        SKIP
    }

    public class TestResult
    {
        public DateTime Time { get; set; }
        public string Module { get; set; } = "AUTH";
        public string Scenario { get; set; }
        public string Api { get; set; }
        public Status Result { get; set; }
        public long ResponseMs { get; set; }
        public bool ValidJson { get; set; }
        public string Error { get; set; }
        public int HttpCode { get; set; }
    }
}
