// Models/ForgotPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; }
}