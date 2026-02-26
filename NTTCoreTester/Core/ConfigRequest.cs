namespace NTTCoreTester.Core
{
    public class ConfigRequest
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, object> Payload { get; set; }

        public string Activity { get; set; }

        public int DelayBeforeMs { get; set; } = 0;

        //public int RetryCount { get; set; } = 0;
        //public int RetryDelayMs { get; set; } = 2000;
    }
}
