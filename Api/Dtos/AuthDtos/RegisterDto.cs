using System.ComponentModel.DataAnnotations;

namespace English.Website.Api.Dtos.AuthDtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        // 👇 Regex bắt buộc: Ít nhất 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 chữ số
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 8 ký tự, bao gồm ít nhất 1 chữ hoa, 1 chữ thường và 1 chữ số.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập lại mật khẩu không được để trống.")]
        // 👇 Tự động so sánh khớp với trường Password ở trên
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string RepeatPassword { get; set; } = string.Empty;
    }
}
