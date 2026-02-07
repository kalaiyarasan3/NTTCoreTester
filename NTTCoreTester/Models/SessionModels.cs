using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{

    //public class UserSession
    //{
    //    public string UserId { get; set; } = string.Empty;
    //    public string UserName { get; set; } = string.Empty;
    //    public string AuthToken { get; set; } = string.Empty;
    //    public string LoginToken { get; set; } = string.Empty;
    //    public DateTime SessionStartTime { get; set; }
    //    public bool IsAuthenticated { get; set; }
    //    public string MaskedToken => MaskToken(AuthToken);

    //    private string MaskToken(string token)
    //    {
    //        if (string.IsNullOrEmpty(token) || token.Length <= 10)
    //            return "***MASKED***";

    //        return $"{token[..5]}...{token[^5..]}";
    //    }
    //}

    //public enum TestStatus
    //{
    //    PASS,
    //    FAIL,
    //    SKIP
    //}

    //public class TestResult
    //{
    //    public DateTime Timestamp { get; set; } = DateTime.Now;
    //    public string Module { get; set; } = "AUTH";
    //    public string ScenarioName { get; set; } = string.Empty;
    //    public string ApiName { get; set; } = string.Empty;
    //    public TestStatus Status { get; set; }
    //    public long ResponseTimeMs { get; set; }
    //    public bool DataTypeValid { get; set; }
    //    public string ErrorMessage { get; set; } = string.Empty;
    //    public int HttpStatusCode { get; set; }
    //}
}
