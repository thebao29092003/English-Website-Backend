using System.ComponentModel.DataAnnotations;

namespace English.Website.Api.Dtos.ContactDtos
{
    public class CreateContactDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nghề nghiệp")]
        public string Occupation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung liên hệ")]
        public string Content { get; set; } = string.Empty;

        public string? TurnstileToken { get; set; }
    }
}
