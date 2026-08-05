using System;
using System.ComponentModel.DataAnnotations;

namespace Yönetim_Paneli.Models
{
    public class Ariza
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Personel numarası boş bırakılamaz!")]
        public string PersonelNo { get; set; } // Bildiren kişinin sicil/personel numarası

        [Required(ErrorMessage = "Ad soyad boş bırakılamaz!")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Departman boş bırakılamaz!")]
        public string Departman { get; set; }

        [Required(ErrorMessage = "Cihaz türü seçilmelidir!")]
        public string CihazTuru { get; set; }

        [Required(ErrorMessage = "Aciliyet durumu seçilmelidir!")]
        public string AciliyetDurumu { get; set; }

        [Required(ErrorMessage = "Hata açıklaması boş bırakılamaz!")]
        public string HataAciklaması { get; set; }

        // Arızayı üstlenen bilgi işlem personeli (İlk başta otomatik olarak Atanmadı yazacak)
        public string ArizayiAlan { get; set; } = "Atanmadı";

        public string Durum { get; set; } = "Bekliyor";

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}