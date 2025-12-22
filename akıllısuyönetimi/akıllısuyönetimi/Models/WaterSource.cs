// Models/WaterSource.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace akıllısuyönetimi.Models
{
    // Baraj ve Kuyu gibi su üretim kaynaklarının verilerini tutar.
    public class WaterSource
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Üretim Kaynağı")]
        public string SourceName { get; set; } // Örn: Tahtalı Barajı, Sarıkız Kuyuları

        [Display(Name = "Son Güncelleme Tarihi")]
        public DateTime LastUpdateDate { get; set; }

        // M3 Verileri
        [Display(Name = "Toplam Su Hacmi (m³)")]
        public decimal TotalVolume { get; set; } // Mevcut Toplam Su Hacmi

        [Display(Name = "Kullanılabilir Su Hacmi (m³)")]
        public decimal UsableVolume { get; set; } // Kullanılabilir Su Hacmi

        // Yüzde Verileri
        [Display(Name = "Aktif Doluluk Oranı (%)")]
        public decimal ActiveFillRate { get; set; }

        [Display(Name = "Göl Yükseltisi (m)")]
        public decimal LakeElevation { get; set; }
    }
}