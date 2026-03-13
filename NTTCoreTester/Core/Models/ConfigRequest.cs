namespace NTTCoreTester.Core.Models
{
    public class ConfigRequest
    {
        public string Endpoint { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public string HeaderProfileName { get; set; }

        public string? Activity { get; set; }
        public string? Description { get; set; }

    }
}
