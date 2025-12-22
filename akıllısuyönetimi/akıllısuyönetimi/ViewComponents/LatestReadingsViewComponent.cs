using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
// Model tanımlamalarınızı buraya ekleyin (Örn: akıllısuyönetimi.Models)

// Basit Sayaç Okuma Modeli (Dinamik veri gelene kadar mock data için)
public class ReadingViewModel
{
    public string SayacNo { get; set; }
    public string Lokasyon { get; set; }
    public string OkumaDegeri { get; set; }
    public string Zaman { get; set; }
    public string Durum { get; set; } // Normal, Anomali
}

public class LatestReadingsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Şimdilik Mock (Örnek) veriler
        var readings = new List<ReadingViewModel>
        {
            new ReadingViewModel { SayacNo = "SYC-4521", Lokasyon = "Konak - Alsancak Mah.", OkumaDegeri = "1.248 m³", Zaman = "30.11.2025 14:30", Durum = "Normal" },
            new ReadingViewModel { SayacNo = "SYC-8932", Lokasyon = "Bornova - Erzene Mah.", OkumaDegeri = "2.891 m³", Zaman = "30.11.2025 14:28", Durum = "Anomali" },
            new ReadingViewModel { SayacNo = "SYC-2341", Lokasyon = "Karşıyaka - Bostanlı", OkumaDegeri = "956 m³", Zaman = "30.11.2025 14:25", Durum = "Normal" }
        };

        return View(readings);
    }
}