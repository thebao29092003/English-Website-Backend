namespace English.Website.Api.Dtos.AuthDtos
{
    public class TokenResponseDto
    {
        /// <summary>
        /// required: bắt buộc phải gán giá trị cho thuộc tính này khi khởi tạo đối tượng,
        /// nếu không
        /// </summary>
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
