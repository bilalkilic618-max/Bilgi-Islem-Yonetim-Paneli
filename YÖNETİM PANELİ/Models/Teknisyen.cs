using System.ComponentModel.DataAnnotations;

namespace Yönetim_Paneli.Models
{
    public class Teknisyen
    {
        public int Id { get; set; }

        public string SicilNo { get; set; } = string.Empty;

        public string AdSoyad { get; set; } = string.Empty;

        public string Unvan { get; set; } = string.Empty;

        public string Telefon { get; set; } = string.Empty;

        // ==========================================
        // GERÇEK E-POSTA VE GÜVENLİK ALANLARI
        // ==========================================

        public string Email { get; set; } = string.Empty;

        public string Sifre { get; set; } = string.Empty;

        // Şifre sıfırlama linkinde gönderilecek benzersiz GUID token
        public string? ResetsToken { get; set; }

        // Şifre sıfırlama token'ının geçerlilik bitişi (UTC). Boşsa token geçersiz sayılır.
        public DateTime? ResetsTokenBitis { get; set; }

        // ==========================================
        // YENİ E-POSTA DOĞRULAMA VE YÖNETİCİ ONAY ALANLARI
        // ==========================================

        // Kullanıcının mailine gidecek 6 haneli doğrulama kodu
        public string? EmailOnayKodu { get; set; }

        // Kullanıcı e-posta adresini doğruladı mı?
        public bool EmailOnayliMi { get; set; } = false;

        // Sen (yönetici) bu kullanıcıya sisteme giriş onayı verdin mi?
        public bool YoneticiOnayliMi { get; set; } = false;

        // ==========================================
        // KULLANICI YÖNETİMİ ALANLARI
        // ==========================================

        // Kayıt anı. Bu alan eklenmeden önceki kayıtlarda bilinmediği için boş olabilir.
        public DateTime? KayitTarihi { get; set; }

        // Oturumdaki rol. Değerler TeknisyenRolleri sınıfında; onaylarken admin seçer.
        public string Rol { get; set; } = TeknisyenRolleri.Teknisyen;

        // Onayı veren adminin adı ve onay anı
        public string? OnaylayanAdmin { get; set; }
        public DateTime? OnayTarihi { get; set; }
    }

    public static class TeknisyenRolleri
    {
        public const string Teknisyen = "Teknisyen";
        public const string DepoSorumlusu = "Depo Sorumlusu";

        public static readonly string[] Tumu = { Teknisyen, DepoSorumlusu };

        public static bool GecerliMi(string? rol) => rol != null && Tumu.Contains(rol);
    }
}