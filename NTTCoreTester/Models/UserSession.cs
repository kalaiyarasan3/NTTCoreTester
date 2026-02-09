namespace NTTCoreTester.Models
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsActive { get; set; }

        
        public string GetMaskedToken()
        {
            return Token; 
        }
    }
}
