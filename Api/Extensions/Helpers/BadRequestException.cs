namespace English.Website.Api.Extensions.Helpers
{
    public class BadRequestException : Exception
    {
        // Khi khởi tạo BadRequestException với tham số message, hãy chuyển tiếp (pass)
        // tham số message này lên constructor của lớp cha (Exception)
        // để nó tự thiết lập thuộc tính Message
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
