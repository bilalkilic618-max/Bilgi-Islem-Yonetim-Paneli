using System;

namespace Yönetim_Paneli.Models
{
    public class ArizaLog
    {
        public int Id { get; set; }
        public int ArizaId { get; set; }
        public string IslemYapan { get; set; } = "Teknik Personel";
        public string YapilanIslem { get; set; } = string.Empty; // Örn: "Durum 'Çözüldü' olarak güncellendi"
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}