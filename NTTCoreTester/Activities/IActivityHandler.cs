using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public interface IActivityHandler
    {
        string Name { get; }
        Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint,string payLoad);
    }

}
