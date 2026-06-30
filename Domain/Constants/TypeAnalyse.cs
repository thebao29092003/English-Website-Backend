namespace English.Website.Domain.Constants
{
    public enum TypeAnalyse
    {
        NOT = 0,     // Không gọi DeepSeek
        QUICK = 1,   // Gọi DeepSeek với prompt ngắn gọn
        FULL = 2     // Gọi DeepSeek với prompt chi tiết đầy đủ
    }
}
