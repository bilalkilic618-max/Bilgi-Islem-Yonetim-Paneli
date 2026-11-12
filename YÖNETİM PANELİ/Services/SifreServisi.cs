using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Yönetim_Paneli.Models;

namespace Yönetim_Paneli.Services
{
    // Şifreleri düz metin yerine hash olarak saklar (ASP.NET Core'un PasswordHasher'ı: PBKDF2 + salt).
    public static class SifreServisi
    {
        private static readonly PasswordHasher<Teknisyen> Hasher = new();

        public static string Hashle(string sifre) => Hasher.HashPassword(null!, sifre);

        // Doğrulama. Eski kayıtlarda şifre düz metin olabilir; o durumda düz karşılaştırma yapılır
        // ve 'yenidenHashlenmeli' true döner, controller şifreyi hash'leyip kaydeder.
        // Böylece mevcut kullanıcılar şifrelerini kaybetmeden ilk girişte hash'e geçer.
        public static bool Dogrula(string? kayitliDeger, string girilenSifre, out bool yenidenHashlenmeli)
        {
            yenidenHashlenmeli = false;
            if (string.IsNullOrEmpty(kayitliDeger) || string.IsNullOrEmpty(girilenSifre)) return false;

            PasswordVerificationResult sonuc;
            try
            {
                sonuc = Hasher.VerifyHashedPassword(null!, kayitliDeger, girilenSifre);
            }
            catch (FormatException)
            {
                sonuc = PasswordVerificationResult.Failed; // Base64 değil → büyük ihtimalle eski düz şifre
            }

            if (sonuc == PasswordVerificationResult.Success) return true;
            if (sonuc == PasswordVerificationResult.SuccessRehashNeeded) { yenidenHashlenmeli = true; return true; }

            // Eski düz metin şifre desteği (geçiş dönemi). Kayıtlı değer zaten hash ise düz karşılaştırma yapılmaz,
            // yoksa hash'in kendisini bilen biri onu şifre gibi girip giriş yapabilirdi.
            bool hashGibiGorunuyor = kayitliDeger.StartsWith("AQAAAA", StringComparison.Ordinal) && kayitliDeger.Length >= 60;
            if (!hashGibiGorunuyor && SabitZamanliEsit(kayitliDeger, girilenSifre))
            {
                yenidenHashlenmeli = true;
                return true;
            }
            return false;
        }

        // Zamanlama saldırısına karşı sabit sürede karşılaştırma
        public static bool SabitZamanliEsit(string a, string b) =>
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
    }
}
