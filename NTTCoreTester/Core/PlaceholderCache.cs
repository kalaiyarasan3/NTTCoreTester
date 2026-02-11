namespace NTTCoreTester.Core
{
    public interface IPlaceholderCache
    {
        void Set(string key, string value);
        string Get(string key);
        bool Has(string key);
        void Clear();
    }

    public class PlaceholderCache : IPlaceholderCache
    {
        private readonly Dictionary<string, string> _cache;

        public PlaceholderCache()
        {
            _cache = new Dictionary<string, string>();
        }

        public void Set(string key, string value)
        {
            _cache[key] = value;
        }

        public string Get(string key)
        {
            return _cache.ContainsKey(key) ? _cache[key] : null;
        }

        public bool Has(string key)
        {
            return _cache.ContainsKey(key);
        }

        public void Clear()
        {
            _cache.Clear();
            Console.WriteLine("  Input cache cleared");
        }
    }
}
