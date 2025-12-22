// Models/DashboardViewModel.cs
using System.Collections.Generic;

namespace akıllısuyönetimi.Models
{
    // Bu model, Index View'a (Dashboard) gönderilecek tüm veriyi bir araya toplar.
    public class DashboardViewModel
    {
        // 1. BARAZ VE SU KAYNAKLARI VERİLERİ (Tüm Adminler ve Yöneticiler için kritik)
        // WaterSources modeliniz, doluluk oranı, hacim vb. verileri tutar.
        public List<WaterSource> WaterSources { get; set; } = new List<WaterSource>();

        // 2. Tüketim ve Sayaç Verileri
        // Consumption modeliniz, sayaçtan gelen tüketim miktarlarını tutar.
        public List<Consumption> ConsumptionRecords { get; set; } = new List<Consumption>();

        // 3. Uyarılar ve Anomaliler
        // Alerts modeliniz, sızıntı veya anormal kullanım gibi uyarıları tutar.
        public List<Alert> RecentAlerts { get; set; } = new List<Alert>();

        // 4. Kart İstatistikleri (Toplam Tüketim, Toplam Doluluk vb.)
        public decimal TotalVolumeInUse { get; set; } // Tüm barajların toplam hacmi
        public decimal AverageFillRate { get; set; } // Ortalama doluluk oranı (%)
        public decimal MonthlyTotalUsage { get; set; } // Bu ayki toplam tüketim (m³)
        public int ActiveAlertCount { get; set; } // Çözülmemiş anomali sayısı
    }
}