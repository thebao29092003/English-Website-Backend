namespace English.Website.Domain.Entities
{
    public class User 
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string  SecurityStamp { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        // có thể null vì khi mới đăng ký chưa đăng nhập lần nào
        public DateTime? LastLoginAt { get; set; }
    }
}
