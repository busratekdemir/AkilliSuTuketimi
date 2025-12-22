// ViewModels/ConsumptionRequestModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace akıllısuyönetimi.Models
{
    public class ConsumptionRequestModel
    {
        // [Required]
        // public string ApiKey { get; set; } // API Key doğrulamasını isterseniz ekleyebilirsiniz.

        [Required]
        [Display(Name = "Sayaç Seri Numarası")]
        public string MeterSerialNumber { get; set; }

        [Required]
        [Display(Name = "Tüketim Değeri (m³)")]
        public decimal UsageValue { get; set; }

        // İsteğe bağlı: Verinin okunduğu zaman
        public DateTime? ReadingTime { get; set; }
    }
}