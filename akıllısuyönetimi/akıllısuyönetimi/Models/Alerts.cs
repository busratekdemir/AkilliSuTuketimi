using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace akıllısuyönetimi.Models
{
    [Table("Alerts", Schema = "dbo")]
    public class Alert
    {
        public int Id { get; set; }

        // Veritabanında MeterId olduğu için burası böyle kalmalı
        public int MeterId { get; set; }

        public string AlertType { get; set; }

        // Veritabanındaki isme (AlertTime) tam uyum
        public DateTime AlertTime { get; set; }

        public string Description { get; set; }

        public bool IsResolved { get; set; } = false;

        [ForeignKey("MeterId")]
        public virtual Meter Meter { get; set; }

        // 🚨 HATA BURADAYDI: MeterCode veritabanında yok, o yüzden [NotMapped] ekledik!
        [NotMapped]
        public string MeterCode => Meter?.MeterCode ?? "Bilinmiyor";

        [NotMapped]
        public string Status => IsResolved ? "Çözüldü" : "Açık";

        [NotMapped]
        public string Priority => AlertType == "LeakageSuspicion" ? "Kritik" : "Yüksek";
    }
}