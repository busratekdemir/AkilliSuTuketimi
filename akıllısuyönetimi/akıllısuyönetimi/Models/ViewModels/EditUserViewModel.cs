// ViewModels/EditUserViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace akıllısuyönetimi.ViewModels
{
    // Bu ViewModel, Kullanıcı Düzenleme (Edit) View'ında veriyi Controller'a taşımak için kullanılır.
    public class EditUserViewModel
    {
        public int Id { get; set; } // int olarak düzelttik (User modelinizdeki gibi)

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public string Role { get; set; }

        // Bu alanlar şimdilik statik rol yönetimi için yeterli olacaktır.
    }
}