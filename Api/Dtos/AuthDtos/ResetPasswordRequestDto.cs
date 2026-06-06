using System.ComponentModel.DataAnnotations;

namespace English.Website.Api.Dtos.AuthDtos
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Mật khẩu mới phải từ 8 ký tự, gồm ít nhất 1 chữ hoa, 1 chữ thường và 1 chữ số.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu nhập lại không được để trống.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string RepeatNewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP không được để trống.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số.")]
        public string Otp { get; set; } = string.Empty;
    }
}
