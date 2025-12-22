using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace akıllısuyönetimi.Models
{
    [Table("Meters", Schema = "dbo")] // Veritabanı şemasıyla tam eşleşme
    public class Meter
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Seri numarası gereklidir.")]
        public string SerialNumber { get; set; } // SQL'deki gerçek sütun

        // 🚨 KRİTİK DEĞİŞİKLİK: Veritabanında MeterCode yok. 
        // [NotMapped] kullanarak hatayı engelliyoruz.
        [NotMapped]
        public string MeterCode
        {
            get => SerialNumber;
            set => SerialNumber = value;
        }

        public string Location { get; set; }

        public string Status { get; set; } // Örn: Aktif, Bakımda, Arızalı
        public string Type { get; set; }   // Örn: Dijital, Smart, Analog

        public int UserID { get; set; } // SQL'deki sütun adı

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}