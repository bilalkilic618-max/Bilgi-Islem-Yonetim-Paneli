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

        // ==========================================
        // YENİ E-POSTA DOĞRULAMA VE YÖNETİCİ ONAY ALANLARI
        // ==========================================

        // Kullanıcının mailine gidecek 6 haneli doğrulama kodu
        public string? EmailOnayKodu { get; set; }

        // Kullanıcı e-posta adresini doğruladı mı?
        public bool EmailOnayliMi { get; set; } = false;

        // Sen (yönetici) bu kullanıcıya sisteme giriş onayı verdin mi?
        public bool YoneticiOnayliMi { get; set; } = false;
    }
}