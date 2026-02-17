using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
  
  
    // Generic response wrapper
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string RequestID { get; set; }
        public T ResponceDataObject { get; set; } 
    }

    

   
}
