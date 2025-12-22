// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace akıllısuyönetimi.Models
{
    public class User
    {
        // Temel Alanlar
        public int Id { get; set; }

        // 👇 YENİ EKLENEN AD ve SOYAD ALANLARI 👇
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }
        // 👆 YENİ EKLENEN ALANLAR SONU 👆

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "Client"; // Varsayılan olarak Client

        public bool IsTemporaryPassword { get; set; } = false;

        // -------------------------------------------------------------------
        // SQL ŞEMASINA UYUMLULUK İÇİN EKLENEN ALANLAR
        // -------------------------------------------------------------------

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpire { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}