using Microsoft.EntityFrameworkCore;

namespace Yönetim_Paneli.Models
{
    public class UygulamaDbContext : DbContext
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tablolarımız
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Ariza> Arizalar { get; set; }
        public DbSet<Envanter> Envanterler { get; set; }
        public DbSet<Teknisyen> Teknisyenler { get; set; }
        public DbSet<ArizaLog> ArizaLoglari { get; set; }
        public DbSet<DepoHareket> DepoHareketleri { get; set; }
    }
}