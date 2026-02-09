using System;
using System.Collections.Generic;

namespace NTTCoreTester.Models.Common
{
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string RequestID { get; set; }
        public string Activity { get; set; }
        public T ResponceDataObject { get; set; }
        public object Responce { get; set; }
        public object TypeID { get; set; }
        public object Info { get; set; }
    }

    public class ResponceDataObjectBase
    {
        public string request_time { get; set; }
        public string status { get; set; }
        public string Message { get; set; }
        public int Result { get; set; }
        public DataObject Data { get; set; }
        public object OSId { get; set; }
        public object TypeID { get; set; }
        public object Info { get; set; }
        public object ModelId { get; set; }
        public object CTA { get; set; }
        public object Action { get; set; }
    }

    public class DataObject
    {
        public string TimeTaken { get; set; }
        public string APITimeTaken { get; set; }
    }
}
