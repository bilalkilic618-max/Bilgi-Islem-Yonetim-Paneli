using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Yönetim_Paneli.Models
{
    public class Envanter
    {
        [Key]
        public int Id { get; set; }

        // 🌟 BARKOD SİSTEMİ İÇİN EKLENEN YENİ ALAN
        public string BarkodNo { get; set; }

        [Required(ErrorMessage = "Malzeme adı boş bırakılamaz!")]
        public string MalzemeAdi { get; set; }

        public string MarkaModel { get; set; }

        [Required(ErrorMessage = "Adet boş bırakılamaz!")]
        public int Adet { get; set; }

        public string Aciklama { get; set; }

        // Veritabanında resmin dosya adını tutacağız (Örn: kablo.jpg)
        public string FotografYolu { get; set; }

        // Bu alan veritabanına eklenmeyecek, sadece formdan resmi seçip sunucuya taşımak için kullanılacak
        [NotMapped]
        public IFormFile FotografDosyasi { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;
    }
}