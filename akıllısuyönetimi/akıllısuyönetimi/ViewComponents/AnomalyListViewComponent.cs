using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic; // List<T> için gereklidir

namespace akıllısuyönetimi.ViewComponents
{
    // Model Sınıfı: Anomali verilerini taşımak için
    public class AnomalyItemViewModel
    {
        public string Id { get; set; }        // Örn: SYC-4521
        public string Description { get; set; } // Örn: Yüksek Tüketim
        public string TimeAgo { get; set; }    // Örn: 2 saat önce
        public string SeverityClass { get; set; } // Örn: warning (sarı) veya danger (kırmızı)
    }

    // View Component Sınıfı: Microsoft.AspNetCore.Mvc.ViewComponent'ten miras almalıdır.
    public class AnomalyListViewComponent : ViewComponent
    {
        // Not: İleride buraya ApplicationDbContext enjekte edilecektir.

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 👇 EKSİK OLAN 3 ÖĞE DAHA EKLENDİ 👇
            var anomalies = new List<AnomalyItemViewModel>
            {
                new AnomalyItemViewModel
                {
                    Id = "SYC-4521",
                    Description = "Yüksek Tüketim",
                    TimeAgo = "2 saat önce",
                    SeverityClass = "warning" // Sarı ikon ve metin
                },
                new AnomalyItemViewModel
                {
                    Id = "SYC-8932",
                    Description = "Kaçak Şüphesi",
                    TimeAgo = "4 saat önce",
                    SeverityClass = "danger" // Kırmızı ikon ve metin
                },
                new AnomalyItemViewModel
                {
                    Id = "SYC-2341",
                    Description = "Anormal Tüketim Modeli",
                    TimeAgo = "6 saat önce",
                    SeverityClass = "warning" // Sarı ikon ve metin
                },
                new AnomalyItemViewModel
                {
                    Id = "SYC-7654",
                    Description = "Gece Kullanımı",
                    TimeAgo = "8 saat önce",
                    SeverityClass = "info" // Mavi/Gri ikon ve metin (Yeni sınıf)
                },
                new AnomalyItemViewModel
                {
                    Id = "SYC-1123",
                    Description = "Ani Artış",
                    TimeAgo = "12 saat önce",
                    SeverityClass = "danger" // Kırmızı ikon ve metin
                }
            };

            // Veriyi Views/Shared/Components/AnomalyList/Default.cshtml'e gönder
            return View(anomalies);
        }
    }
}