using System.ComponentModel.DataAnnotations;

namespace FileManagement.ViewModels;

public class EditUserViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Rol gereklidir.")]
    public string Role { get; set; }
}