// Models/Consumption.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace akıllısuyönetimi.Models
{
    // Sayaçlardan gelen anlık tüketim verileri
    public class Consumption
    {
        public long Id { get; set; }
        public int MeterId { get; set; }
        public DateTime ReadingTime { get; set; }
        public decimal UsageValue { get; set; } // Tüketim miktarı (m³)

        // İlişki: Hangi sayaca ait olduğunu gösterir
        [ForeignKey("MeterId")]
        public Meter Meter { get; set; }
    }
}