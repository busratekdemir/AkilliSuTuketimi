// Data/ApplicationDbContext.cs

using akıllısuyönetimi.Models; // Modelleri kullanmak için ekledik
using Microsoft.EntityFrameworkCore;


namespace akıllısuyönetimi.Data
{
    // DbContext sınıfı, Entity Framework Core'dan türemelidir.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 👇 VERİTABANINDAKİ TABLOLARINIZ (DbSet'ler) 👇

        // Kullanıcı ve Rol Yönetimi için (Admin/Client)
        public DbSet<User> Users { get; set; }
        public DbSet<WaterSource> WaterSources { get; set; }

        // --- DİNAMİK VERİ İÇİN ZORUNLU EKLENEN DBSET'LER ---
        // HomeController.Index ve filtreleme için gerekli
        public DbSet<Meter> Meters { get; set; }

        // Tüketim verilerini çekmek için gerekli
        public DbSet<Consumption> Consumption { get; set; }

        // Anomali/Uyarı verilerini çekmek için gerekli
        public DbSet<Alert> Alerts { get; set; }
        // ----------------------------------------------------
    }
}