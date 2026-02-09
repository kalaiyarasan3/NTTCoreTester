using System.Collections.Generic;
using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Models.Auth
{
    /// <summary>
    /// SendOTP Response Data
    /// </summary>
    public class SendOtpData : ResponceDataObjectBase  // ✅ Must inherit
    {
        public string DealerUCC { get; set; }
        public int DealerStatus { get; set; }
        public int ClientType { get; set; }
    }

    /// <summary>
    /// Login SUCCESS Response Data
    /// </summary>
    public class LoginSuccessData : ResponceDataObjectBase  // ✅ Must inherit
    {
        public string susertoken { get; set; }
        public string uname { get; set; }
        public string uid { get; set; }
        public int TOTPEnabled { get; set; }
        public int IsTOTPSkip { get; set; }
        public int DealerStatus { get; set; }
        public int ClientType { get; set; }

        public List<string> prarr { get; set; }
        public List<string> access_type { get; set; }
        public List<string> orarr { get; set; }

        public Dictionary<string, string> values { get; set; }
        public Dictionary<string, List<MwsItem>> mws { get; set; }

        public object AuthorizedActivity { get; set; }
        public string email { get; set; }
        public List<int> KraStatus { get; set; }
        public List<string> Clients { get; set; }
    }

    /// <summary>
    /// Market Watch Item
    /// </summary>
    public class MwsItem
    {
        public int MarketWatchId { get; set; }
        public string MarketWatchName { get; set; }
        public string exch { get; set; }
        public int token { get; set; }
        public string tsym { get; set; }
        public string Segment { get; set; }
        public string instname { get; set; }
        public string pp { get; set; }
        public string ls { get; set; }
        public string ti { get; set; }
        public object SymbolInfos { get; set; }
    }

    /// <summary>
    /// Login ERROR Response Data
    /// </summary>
    public class LoginErrorData : ResponceDataObjectBase  // ✅ Must inherit
    {
        public string susertoken { get; set; }
        public object AuthorizedActivity { get; set; }
        public object values { get; set; }
        public object mws { get; set; }
        public string uname { get; set; }
        public object prarr { get; set; }
        public object access_type { get; set; }
        public string uid { get; set; }
        public object orarr { get; set; }
        public string email { get; set; }
        public int? TOTPEnabled { get; set; }
        public int? IsTOTPSkip { get; set; }
        public int? DealerStatus { get; set; }
        public int? ClientType { get; set; }
        public object KraStatus { get; set; }
        public object Clients { get; set; }
    }

    /// <summary>
    /// General Auth Response Data
    /// </summary>
    public class GeneralAuthData : ResponceDataObjectBase  // ✅ Must inherit
    {
        // No additional fields
    }
}
