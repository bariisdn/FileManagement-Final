using System.ComponentModel.DataAnnotations;

namespace FileManagement.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifre doğrulama gereklidir.")]
    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
    public string ConfirmPassword { get; set; }
    
    public string Role { get; set; } = "User";
}