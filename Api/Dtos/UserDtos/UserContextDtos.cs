namespace English.Website.Api.Dtos.UserDtos
{
    public class UserContextDtos
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        // có thể null vì khi mới đăng ký chưa đăng nhập lần nào
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
