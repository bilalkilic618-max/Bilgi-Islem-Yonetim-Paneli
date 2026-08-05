using System;
using System.Net;
using System.Net.Mail;

namespace Yönetim_Paneli.Services
{
    public static class EmailService
    {
        // ==========================================
        // 1. ŞİFRE SIFIRLAMA MAİLİ GÖNDERME
        // ==========================================
        public static bool SifreSifirlamaMailiGonder(string aliciEmail, string resetLink)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("bilalkilic618@gmail.com", "bilaltech Bilgi İşlem");
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

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("bilalkilic618@gmail.com", "gqgpsnxsbniyhqrh");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ==========================================
        // 2. YENİ KAYIT E-POSTA ONAY KODU GÖNDERME
        // ==========================================
        public static bool OnayKoduGonder(string aliciEmail, string onayKodu)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("bilalkilic618@gmail.com", "bilaltech Bilgi İşlem");
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

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("bilalkilic618@gmail.com", "gqgpsnxsbniyhqrh");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ==========================================
        // 3. YÖNETİCİYE BİLDİRİM MAİLİ (TEK TIKLA ONAY)
        // ==========================================
        public static bool YoneticiyeOnayMailiGonder(string yoneticiEmail, string yeniKullaniciAd, string yeniKullaniciEmail, string onayLink)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("bilalkilic618@gmail.com", "bilaltech Otomasyon");
                mail.To.Add(yoneticiEmail);
                mail.Subject = "🚨 YENİ KULLANICI ONAY BEKLİYOR - bilaltech";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #eab308;'>🔔 Onay Bekleyen Yeni Üyelik</h2>
                        <p style='color: #cbd5e1;'>Merhaba Yönetici,</p>
                        <p style='color: #cbd5e1;'>Sisteme yeni bir kullanıcı kaydoldu ve e-postasını doğruladı:</p>
                        
                        <div style='background: #1e293b; padding: 15px; border-radius: 8px; margin: 15px 0; border: 1px solid #334155;'>
                            <p style='margin: 5px 0;'><strong>Ad Soyad:</strong> {yeniKullaniciAd}</p>
                            <p style='margin: 5px 0;'><strong>E-Posta:</strong> {yeniKullaniciEmail}</p>
                        </div>

                        <p style='color: #cbd5e1;'>Bu kullanıcının sisteme giriş yapmasına izin vermek istiyorsanız aşağıdaki butona tıklayın:</p>
                        
                        <div style='margin: 25px 0; text-align: center;'>
                            <a href='{onayLink}' style='background: #22c55e; color: white; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                                ✅ Kullanıcıyı Onayla ve Giriş Yetkisi Ver
                            </a>
                        </div>
                    </div>";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("bilalkilic618@gmail.com", "gqgpsnxsbniyhqrh");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ==========================================
        // 4. LÜTFÜ ABİ İÇİN KRİTİK STOK BİLDİRİM MAİLİ
        // ==========================================
        public static bool KritikStokMailiGonder(string malzemeAdi, string barkodNo, int kalanAdet, string lutfuAbiEmail)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("bilalkilic618@gmail.com", "bilaltech Depo Otomasyonu");
                mail.To.Add(lutfuAbiEmail);
                mail.Subject = $"🚨 KRİTİK STOK UYARISI: {malzemeAdi}";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #0f172a; color: #ffffff; border-radius: 10px; max-width: 500px;'>
                        <h2 style='color: #ef4444; border-bottom: 2px solid #ef4444; padding-bottom: 10px;'>🚨 Kritik Stok Seviyesi</h2>
                        <p style='color: #cbd5e1;'>Merhaba Lütfü Bey,</p>
                        <p style='color: #cbd5e1;'>Bilgi İşlem depomuzdaki bir donanımın stok seviyesi <strong>kritik sınıra (2 ve altı)</strong> ulaşmıştır.</p>
                        
                        <div style='background: #1e293b; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #ef4444;'>
                            <p style='margin: 5px 0; font-size: 16px; color: #cbd5e1;'><strong>Malzeme Adı:</strong> {malzemeAdi}</p>
                            <p style='margin: 5px 0; font-size: 16px; color: #cbd5e1;'><strong>Barkod No:</strong> {(string.IsNullOrEmpty(barkodNo) ? "Yok" : barkodNo)}</p>
                            <p style='margin: 5px 0; font-size: 18px; color: #ef4444;'><strong>Elde Kalan Stok: {kalanAdet} Adet</strong></p>
                        </div>
                        
                        <p style='color: #cbd5e1;'>İlgili birimlerin mağduriyet yaşamaması adına lütfen en kısa sürede yeni tedarik/satın alma sürecini başlatınız.</p>
                        <br/>
                        <hr style='border: none; border-top: 1px solid #334155;' />
                        <p style='font-size: 12px; color: #64748b; text-align: center;'>Bu mesaj bilaltech Depo Otomasyonu tarafından otomatik oluşturulmuştur.</p>
                    </div>";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("bilalkilic618@gmail.com", "gqgpsnxsbniyhqrh");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}