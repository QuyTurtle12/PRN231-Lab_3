namespace DataAccess.DTO.Auth
{
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
