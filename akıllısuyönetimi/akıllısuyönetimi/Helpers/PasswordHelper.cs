// Helpers/PasswordHelper.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace akıllısuyönetimi.Helpers
{
    // Güvenli şifre hash'leme ve doğrulama işlemleri için yardımcı sınıf
    public static class PasswordHelper
    {
        
        public static string HashPassword(string password)
        {
            // Şifreye özel, rastgele bir tuz (salt) oluştur
            byte[] salt = new byte[128 / 8]; // 16 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // PBKDF2 kullanarak şifreyi hash'le
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000, // Yüksek iterasyon sayısı güvenliği artırır
                numBytesRequested: 256 / 8));

            // Hash'lenmiş şifreyi ve tuzu tek bir string olarak döndür
            // Format: "tuz:hash"
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        
       
    }
}