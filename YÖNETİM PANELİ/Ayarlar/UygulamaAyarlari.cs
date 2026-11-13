namespace Yönetim_Paneli.Ayarlar
{
    // appsettings.json → "Smtp" bölümü. Şifre ASLA bu dosyaya ya da koda yazılmaz:
    // geliştirmede User Secrets, sunucuda ortam değişkeni (Smtp__Sifre) kullanılır.
    public class SmtpAyarlari
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool SslKullan { get; set; } = true;
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public string GonderenAdres { get; set; } = string.Empty;
    }

    // appsettings.json → "Yonetim" bölümü.
    public class YonetimAyarlari
    {
        // Süper admin girişi ve depo ek şifresi. Bu değerler de koda/appsettings.json'a yazılmaz,
        // User Secrets veya ortam değişkeninden (Yonetim__AdminSifre, Yonetim__DepoSifresi) gelir.
        // Boş bırakılırsa ilgili giriş tamamen kapalı olur.
        public string AdminKullaniciAdi { get; set; } = string.Empty;
        public string AdminSifre { get; set; } = string.Empty;
        public string DepoSifresi { get; set; } = string.Empty;

        // Süper admin oturumunda görünen ad ve unvan (gizli değil). Boşsa "Sistem Yöneticisi" kullanılır.
        // Bu ad, onaylanan kullanıcılarda OnaylayanAdmin alanına da yazılır.
        public string AdminAdSoyad { get; set; } = string.Empty;
        public string AdminUnvan { get; set; } = string.Empty;

        // Bildirim alacak adresler
        public string YoneticiEmail { get; set; } = string.Empty;
        public string DepoSorumlusuEmail { get; set; } = string.Empty;
    }

    // Oturum çerezine yazılan özel claim adları
    public static class YetkiSabitleri
    {
        public const string AdminClaim = "BilgiIslemAdmin";
        // Depo ek şifresi girildiğinde, onayın bitiş zamanı (UTC, "o" formatı) bu claim'e yazılır.
        public const string DepoOnayBitisClaim = "DepoOnayBitis";
    }
}
