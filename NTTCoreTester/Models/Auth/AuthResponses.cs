using NTTCoreTester.Models.Common.NTTCoreTester.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models.Auth
{
    /// <summary>
    /// SendOTP Response Data
    /// Level 3: 3 additional fields (DealerUCC, DealerStatus, ClientType)
    /// Always validated regardless of StatusCode
    /// </summary>
    public class SendOtpData : ResponceDataObjectBase
    {
        public string DealerUCC { get; set; }
        public int DealerStatus { get; set; }
        public int ClientType { get; set; }
    }

    /// <summary>
    /// Login SUCCESS Response Data (StatusCode = 0)
    /// Level 3: 17 additional fields + complex objects (values, mws)
    /// Fully validated on success
    /// </summary>
    public class LoginSuccessData : ResponceDataObjectBase
    {
        // Required string fields
        public string susertoken { get; set; }
        public string uname { get; set; }
        public string uid { get; set; }

        // Required number fields
        public int TOTPEnabled { get; set; }
        public int IsTOTPSkip { get; set; }
        public int DealerStatus { get; set; }
        public int ClientType { get; set; }

        // Required array fields
        public List<string> prarr { get; set; }
        public List<string> access_type { get; set; }
        public List<string> orarr { get; set; }

        // Required complex object fields
        public Dictionary<string, string> values { get; set; }              // Dynamic keys, string values
        public Dictionary<string, List<MwsItem>> mws { get; set; }          // Dynamic keys, array of MwsItem

        // Optional/nullable fields
        public object AuthorizedActivity { get; set; }
        public string email { get; set; }
        public List<int> KraStatus { get; set; }
        public List<string> Clients { get; set; }
    }

    /// <summary>
    /// Market Watch Item structure (used in LoginSuccessData.mws)
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
    /// Login ERROR Response Data (StatusCode ≠ 0)
    /// Level 3: Same 17 fields but all nullable
    /// Record only - do not validate (they are null in error scenarios)
    /// </summary>
    public class LoginErrorData : ResponceDataObjectBase
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
    /// Used by: CheckLogin, ForgotPassword, ResetPassword, Logout
    /// Level 3: No additional fields beyond common ResponceDataObjectBase
    /// </summary>
    public class GeneralAuthData : ResponceDataObjectBase
    {
        // No additional fields - only inherits common fields from base
    }
}
