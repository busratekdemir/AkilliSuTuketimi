using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

// Uyarı Detay Modeli
public class AlertDetail
{
    public string Type { get; set; } // Olası Kaçak, Yüksek Tüketim
    public string Severity { get; set; } // Kritik, Yüksek, Orta
    public string SayacNo { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string DetectionTime { get; set; }
    public string Consumption { get; set; }
    public string Status { get; set; } // İnceleniyor, Bekliyor, Çözüldü
    public string ColorClass { get; set; } // danger, warning, success
    public string IconClass { get; set; } // bi-x-octagon, bi-exclamation-triangle
}

public class AlertDetailListViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var alerts = new List<AlertDetail>
        {
            new AlertDetail { Type = "Olası Kaçak", Severity = "Kritik", SayacNo = "SYC-8932", Description = "Sürekli su akışı tespit edildi. Normal tüketim paterninin dışında.", Location = "Bornova - Erzene Mah.", DetectionTime = "30.11.2025 10:45", Consumption = "2891 m³/gün", Status = "İnceleniyor", ColorClass = "danger", IconClass = "bi-x-octagon-fill" },
            new AlertDetail { Type = "Yüksek Tüketim", Severity = "Yüksek", SayacNo = "SYC-4521", Description = "Aylık ortalamanın %45 üzerinde tüketim.", Location = "Konak - Alsancak Mah.", DetectionTime = "30.11.2025 08:30", Consumption = "1248 m³/gün", Status = "Bekliyor", ColorClass = "warning", IconClass = "bi-exclamation-triangle-fill" },
            new AlertDetail { Type = "Ani Artış", Severity = "Yüksek", SayacNo = "SYC-1123", Description = "Son 24 saatte beklenmeyen tüketim artışı.", Location = "Bayraklı - Cennetoğlu", DetectionTime = "30.11.2025 07:15", Consumption = "3245 m³/gün", Status = "Bekliyor", ColorClass = "warning", IconClass = "bi-exclamation-triangle-fill" },
            new AlertDetail { Type = "Gece Tüketimi", Severity = "Orta", SayacNo = "SYC-7654", Description = "Gece saatlerinde normalin üstünde kullanım.", Location = "Çiğli - Balatçık Mah.", DetectionTime = "30.11.2025 02:20", Consumption = "1782 m³/gün", Status = "Çözüldü", ColorClass = "warning", IconClass = "bi-exclamation-triangle-fill" },
            new AlertDetail { Type = "Düzensiz Kullanım", Severity = "Orta", SayacNo = "SYC-2341", Description = "Tüketim paterni düzensiz, kontrol edilmeli.", Location = "Karşıyaka - Bostanlı", DetectionTime = "29.11.2025 23:50", Consumption = "956 m³/gün", Status = "Çözüldü", ColorClass = "warning", IconClass = "bi-exclamation-triangle-fill" },
        };
        return View(alerts);
    }
}