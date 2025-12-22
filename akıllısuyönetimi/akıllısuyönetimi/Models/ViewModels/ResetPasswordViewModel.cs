// Models/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    // E-posta, formda gizli alan olarak taşınacak veya TempData'dan alınacak.
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Şifreyi Onayla")]
    [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
    public string ConfirmPassword { get; set; }
}