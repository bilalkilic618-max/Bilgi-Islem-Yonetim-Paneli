using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yönetim_Paneli.Ayarlar;

namespace Yönetim_Paneli.Services
{
    public class EmailService
    {
        private readonly SmtpAyarlari _ayarlar;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpAyarlari> ayarlar, ILogger<EmailService> logger)
        {
            _ayarlar = ayarlar.Value;
            _logger = logger;
        }

        // Tüm mailler bu tek metottan gider. SMTP bilgileri koda yazılmaz, ayarlardan (User Secrets / ortam değişkeni) okunur.
        private void Gonder(MailMessage mail)
        {
            using var smtp = new SmtpClient(_ayarlar.Host, _ayarlar.Port)
            {
                Credentials = new NetworkCredential(_ayarlar.KullaniciAdi, _ayarlar.Sifre),
                EnableSsl = _ayarlar.SslKullan
            };
            smtp.Send(mail);
        }

        // ==========================================
        // 1. ŞİFRE SIFIRLAMA MAİLİ GÖNDERME
        // ==========================================
        public bool SifreSifirlamaMailiGonder(string aliciEmail, string resetLink)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Bilgi İşlem");
                mail.To.Add(aliciEmail);
                mail.Subject = "🔐 Şifre Sıfırlama Talebi - bilaltech";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #3b82f6;'>bilaltech Şifre Sıfırlama</h2>
                        <p style='color: #cbd5e1;'>Merhaba,</p>
                        <p style='color: #cbd5e1;'>Hesabınız için şifre sıfırlama talebinde bulunuldu. Şifrenizi yenilemek için aşağıdaki butona tıklayın:</p>
                        
                        <div style='margin: 30px 0; text-align: center;'>
                            <a href='{resetLink}' style='background: linear-gradient(135deg, #3b82f6 0%, #6366f1 100%); color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Şifremi Sıfırla</a>
                        </div>
                        
                        <p style='color: #64748b; font-size: 0.8rem;'>Bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.</p>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }

        // ==========================================
        // 2. YENİ KAYIT E-POSTA ONAY KODU GÖNDERME
        // ==========================================
        public bool OnayKoduGonder(string aliciEmail, string onayKodu)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Bilgi İşlem");
                mail.To.Add(aliciEmail);
                mail.Subject = "🔑 E-Posta Onay Kodu - bilaltech";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #3b82f6;'>bilaltech Hesap Doğrulama</h2>
                        <p style='color: #cbd5e1;'>Merhaba,</p>
                        <p style='color: #cbd5e1;'>Kaydınızı tamamlamak için aşağıdaki 6 haneli doğrulama kodunu sisteme giriniz:</p>
                        
                        <div style='margin: 25px 0; text-align: center; background: #1e293b; padding: 15px; border-radius: 8px; font-size: 26px; font-weight: bold; letter-spacing: 6px; color: #38bdf8; border: 1px solid #334155;'>
                            {onayKodu}
                        </div>
                        
                        <p style='color: #64748b; font-size: 0.8rem;'>Bu doğrulama kodunu güvenlik nedeniyle kimseyle paylaşmayın.</p>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }

        // ==========================================
        // 3. YÖNETİCİYE YENİ KULLANICI BİLDİRİMİ (sadece haber verir; onay panelden yapılır)
        // ==========================================
        public bool YoneticiyeYeniKullaniciBildirimiGonder(string yoneticiEmail, string yeniKullaniciAd, string yeniKullaniciEmail, string yonetimLinki)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Otomasyon");
                mail.To.Add(yoneticiEmail);
                mail.Subject = "🚨 YENİ KULLANICI ONAY BEKLİYOR - bilaltech";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #eab308;'>🔔 Onay Bekleyen Yeni Üyelik</h2>
                        <p style='color: #cbd5e1;'>Merhaba Yönetici,</p>
                        <p style='color: #cbd5e1;'>Sisteme yeni bir kullanıcı kaydoldu ve e-postasını doğruladı:</p>
                        
                        <div style='background: #1e293b; padding: 15px; border-radius: 8px; margin: 15px 0; border: 1px solid #334155;'>
                            <p style='margin: 5px 0;'><strong>Ad Soyad:</strong> {WebUtility.HtmlEncode(yeniKullaniciAd)}</p>
                            <p style='margin: 5px 0;'><strong>E-Posta:</strong> {WebUtility.HtmlEncode(yeniKullaniciEmail)}</p>
                        </div>

                        <p style='color: #cbd5e1;'>Başvuruyu incelemek, rol seçip onaylamak veya reddetmek için panelde Kullanıcı Yönetimi ekranını açın:</p>

                        <div style='margin: 25px 0; text-align: center;'>
                            <a href='{WebUtility.HtmlEncode(yonetimLinki)}' style='background: #3b82f6; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                                👥 Kullanıcı Yönetimi'ni Aç
                            </a>
                        </div>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }

        // ==========================================
        // 3a. KULLANICIYA ONAY BİLGİSİ
        // ==========================================
        public bool KullaniciOnaylandiMailiGonder(string aliciEmail, string adSoyad, string rol, string girisLinki)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Bilgi İşlem");
                mail.To.Add(aliciEmail);
                mail.Subject = "✅ Hesabınız Onaylandı - bilaltech";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #22c55e;'>Hesabınız Onaylandı</h2>
                        <p style='color: #cbd5e1;'>Merhaba {WebUtility.HtmlEncode(adSoyad)},</p>
                        <p style='color: #cbd5e1;'>Bilgi İşlem Destek Portalı üyeliğiniz yönetici tarafından onaylandı. Sisteme <strong>{WebUtility.HtmlEncode(rol)}</strong> rolüyle giriş yapabilirsiniz.</p>

                        <div style='margin: 25px 0; text-align: center;'>
                            <a href='{WebUtility.HtmlEncode(girisLinki)}' style='background: #22c55e; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Giriş Yap</a>
                        </div>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }

        // ==========================================
        // 3b. KULLANICIYA RET BİLGİSİ
        // ==========================================
        public bool KullaniciReddedildiMailiGonder(string aliciEmail, string adSoyad, string? sebep)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Bilgi İşlem");
                mail.To.Add(aliciEmail);
                mail.Subject = "Üyelik Başvurunuz Hakkında - bilaltech";
                mail.IsBodyHtml = true;

                string sebepBlogu = string.IsNullOrEmpty(sebep)
                    ? string.Empty
                    : $@"<div style='background: #1e293b; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #ef4444;'>
                            <p style='margin: 0; color: #cbd5e1;'><strong>Açıklama:</strong> {WebUtility.HtmlEncode(sebep)}</p>
                         </div>";

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #ef4444;'>Üyelik Başvurunuz Onaylanmadı</h2>
                        <p style='color: #cbd5e1;'>Merhaba {WebUtility.HtmlEncode(adSoyad)},</p>
                        <p style='color: #cbd5e1;'>Bilgi İşlem Destek Portalı üyelik başvurunuz yönetici tarafından onaylanmadı ve kaydınız silindi.</p>
                        {sebepBlogu}
                        <p style='color: #64748b; font-size: 0.85rem;'>Bir yanlışlık olduğunu düşünüyorsanız birim yetkilinizle iletişime geçin.</p>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }

        // ==========================================
        // 4. DEPO SORUMLUSU İÇİN KRİTİK STOK BİLDİRİM MAİLİ
        // ==========================================
        public bool KritikStokMailiGonder(string malzemeAdi, string barkodNo, int kalanAdet, string depoSorumlusuEmail)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_ayarlar.GonderenAdres, "bilaltech Depo Otomasyonu");
                mail.To.Add(depoSorumlusuEmail);
                mail.Subject = $"🚨 KRİTİK STOK UYARISI: {malzemeAdi}";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #ef4444; border-bottom: 2px solid #ef4444; padding-bottom: 10px;'>🚨 Kritik Stok Seviyesi</h2>
                        <p style='color: #cbd5e1;'>Merhaba Lütfü Bey,</p>
                        <p style='color: #cbd5e1;'>Bilgi İşlem depomuzdaki bir donanımın stok seviyesi <strong>kritik sınıra (2 ve altı)</strong> ulaşmıştır.</p>
                        
                        <div style='background: #1e293b; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #ef4444;'>
                            <p style='margin: 5px 0; font-size: 16px; color: #cbd5e1;'><strong>Malzeme Adı:</strong> {WebUtility.HtmlEncode(malzemeAdi)}</p>
                            <p style='margin: 5px 0; font-size: 16px; color: #cbd5e1;'><strong>Barkod No:</strong> {(string.IsNullOrEmpty(barkodNo) ? "Yok" : WebUtility.HtmlEncode(barkodNo))}</p>
                            <p style='margin: 5px 0; font-size: 18px; color: #ef4444;'><strong>Elde Kalan Stok: {kalanAdet} Adet</strong></p>
                        </div>
                        
                        <p style='color: #cbd5e1;'>İlgili birimlerin mağduriyet yaşamaması adına lütfen en kısa sürede yeni tedarik/satın alma sürecini başlatınız.</p>
                        <br/>
                        <hr style='border: none; border-top: 1px solid #334155;' />
                        <p style='font-size: 12px; color: #64748b; text-align: center;'>Bu mesaj bilaltech Depo Otomasyonu tarafından otomatik oluşturulmuştur.</p>
                    </div>";

                Gonder(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi.");
                return false;
            }
        }
    }
}