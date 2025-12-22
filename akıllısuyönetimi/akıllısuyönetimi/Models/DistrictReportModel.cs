namespace akıllısuyönetimi.Models
{
    public class DistrictReportModel
    {
        public string District { get; set; }        // İlçe
        public string CurrentUsage { get; set; }   // Bu Ay
        public string PrevUsage { get; set; }      // Geçen Ay
        public string Change { get; set; }         // Değişim
        public string Subscribers { get; set; }    // Abone Sayısı
        public double Average { get; set; }        // Ortalama
    }
}