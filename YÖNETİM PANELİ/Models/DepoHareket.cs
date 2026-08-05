using System;

namespace Yönetim_Paneli.Models
{
    public class DepoHareket
    {
        public int Id { get; set; }
        public DateTime Tarih { get; set; }
        public string IslemTuru { get; set; } = string.Empty; // GİRİŞ, ÇIKIŞ, DEĞİŞİM

        // 🌟 BARKOD OKUYUCU İÇİN EKLENEN YENİ ALAN
        public string BarkodNo { get; set; } = string.Empty;

        public string MalzemeAdi { get; set; } = string.Empty;
        public int Adet { get; set; }
        public string Mudurluk { get; set; } = string.Empty;
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}