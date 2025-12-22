// ViewModels/WaterSourceRequestModel.cs
using System.ComponentModel.DataAnnotations;

namespace akıllısuyönetimi.Models
{
    public class WaterSourceRequestModel
    {
        [Required]
        [Display(Name = "Kaynak Adı")]
        public string SourceName { get; set; }

        [Required]
        [Display(Name = "Mevcut Hacim (m³)")]
        public decimal UsableVolume { get; set; }

        [Required]
        [Display(Name = "Doluluk Oranı (%)")]
        public decimal ActiveFillRate { get; set; }

        // TotalVolume (Toplam kapasite) bilgisi de opsiyonel eklenebilir.
    }
}