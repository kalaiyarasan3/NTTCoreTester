namespace NTTCoreTester.Core
{
    public class ConfigRequest
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
