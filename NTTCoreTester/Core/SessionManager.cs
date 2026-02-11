namespace NTTCoreTester.Core
{
    public interface ISessionManager
    {
        void SetSession(string token, string userId, string userName);
        string GetToken();
        string GetUserId();
        string GetUserName();
        bool HasSession();
        void ClearSession();
    }

    public class SessionManager : ISessionManager
    {
        private string _token;
        private string _userId;
        private string _userName;

        public void SetSession(string token, string userId, string userName)
        {
            _token = token;
            _userId = userId;
            _userName = userName;
            Console.WriteLine($"✅ Session saved: {_userName} ({_userId})");
        }

        public string GetToken()
        {
            return _token;
        }

        public string GetUserId()
        {
            return _userId;
        }

        public string GetUserName()
        {
            return _userName;
        }

        public bool HasSession()
        {
            return !string.IsNullOrEmpty(_token);
        }

        public void ClearSession()
        {
            _token = null;
            _userId = null;
            _userName = null;
            Console.WriteLine("🗑️  Session cleared");
        }

        public string GetMaskedToken()
        {
            if (string.IsNullOrEmpty(_token))
                return "";

            if (_token.Length <= 6)
                return "***";

            return _token.Substring(0, 3) + "***" + _token.Substring(_token.Length - 3);
        }
    }
}
