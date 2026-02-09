using NTTCoreTester.Models.Common;

namespace NTTCoreTester.Validators.Common
{
    
    public interface IStatusCorrelationValidator
    {
        ValidationResult Validate<T>(ApiResponse<T> response) where T : ResponceDataObjectBase;
    }

    public class StatusCorrelationValidator : IStatusCorrelationValidator
    {
        public ValidationResult Validate<T>(ApiResponse<T> response) where T : ResponceDataObjectBase
        {
            var result = new ValidationResult();

            if (response == null || response.ResponceDataObject == null)
            {
                result.AddCorrelationError("Cannot validate - response or data is null");
                return result;
            }

            // StatusCode - must be int (number)
           
            if (response.StatusCode < 0)
            {
                result.AddCorrelationError("StatusCode is negative", "non-negative int", response.StatusCode);
            }

            // Status -->top - must be string
            if (response.Status == null)
            {
                result.AddCorrelationError("Status is null", "string", null);
            }
            else if (!(response.Status is string))
            {
                result.AddCorrelationError("Status is not a string", "string", response.Status.GetType().Name);
            }

            // status (ResponceDataObject) - must be string
            if (response.ResponceDataObject.status == null)
            {
                result.AddCorrelationError("ResponceDataObject.status is null", "string", null);
            }
            else if (!(response.ResponceDataObject.status is string))
            {
                result.AddCorrelationError("ResponceDataObject.status is not a string", "string",
                    response.ResponceDataObject.status.GetType().Name);
            }

            // Result - must be int (number)
            // (Already validated as int type by C#)

            return result;
        }
    }
}
