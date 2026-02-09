using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models.Common
{
    using System;
    using System.Collections.Generic;

    namespace NTTCoreTester.Models.Common
    {
        /// <summary>
        /// Level 1: Common Response Envelope
        /// Present in ALL APIs across ALL modules (Auth, Trading, Reports, etc.)
        /// </summary>
        public class ApiResponse<T>
        {
            // Required fields (9 total)
            public string Status { get; set; }
            public string Message { get; set; }
            public int StatusCode { get; set; }
            public string RequestID { get; set; }
            public string Activity { get; set; }

            // Main data container - generic type T allows different modules to use their own models
            public T ResponceDataObject { get; set; }

            // Optional fields
            public object Responce { get; set; }
            public object TypeID { get; set; }
            public object Info { get; set; }
        }

        /// <summary>
        /// Level 2: Common fields inside ResponceDataObject
        /// Present in ALL ResponceDataObject across ALL modules
        /// Base class that all module-specific response models inherit from
        /// </summary>
        public class ResponceDataObjectBase
        {
            // Required common fields (11 total)
            public string request_time { get; set; }
            public string status { get; set; }          // "Ok" or "Not_Ok"
            public string Message { get; set; }
            public int Result { get; set; }
            public DataObject Data { get; set; }

            // Optional common fields
            public object OSId { get; set; }
            public object TypeID { get; set; }
            public object Info { get; set; }
            public object ModelId { get; set; }
            public object CTA { get; set; }
            public object Action { get; set; }
        }

        /// <summary>
        /// Common Data object containing timing information
        /// Present in all APIs
        /// </summary>
        public class DataObject
        {
            public string TimeTaken { get; set; }
            public string APITimeTaken { get; set; }    // Present in auth APIs
        }
    }

}
