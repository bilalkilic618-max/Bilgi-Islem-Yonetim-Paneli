using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Yönetim_Paneli.Ayarlar;
using Yönetim_Paneli.Models;
using Yönetim_Paneli.Services;

namespace Yönetim_Paneli.Controllers
{
    // Varsayılan olarak TÜM sayfalar giriş ister. Giriş gerektirmeyenler [AllowAnonymous] ile işaretlendi.
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UygulamaDbContext _context;
        private readonly EmailService _email;
        private readonly YonetimAyarlari _yonetim;

        public HomeController(UygulamaDbContext context, EmailService email, IOptions<YonetimAyarlari> yonetim)
        {
            _context = context;
            _email = email;
            _yonetim = yonetim.Value;
        }

        // Giriş yapan kullanıcının adı (eskiden düz çerezden okunuyordu)
        private string AktifKullaniciAdi => User.Identity?.Name ?? "Sistem";

        // ==========================================
        // OTURUM YARDIMCILARI
        // ==========================================

        private async Task OturumAcAsync(string ad, string rol, bool admin, DateTime? depoOnayBitisUtc)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, ad),
                new Claim(ClaimTypes.Role, rol)
            };
            if (admin) claims.Add(new Claim(YetkiSabitleri.AdminClaim, "true"));
            if (depoOnayBitisUtc.HasValue)
                claims.Add(new Claim(YetkiSabitleri.DepoOnayBitisClaim, depoOnayBitisUtc.Value.ToString("o")));

            var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(kimlik));
        }

        // Mevcut oturumu koruyarak sadece depo onayını açar/kapatır
        private Task DepoOnayiniAyarlaAsync(DateTime? bitisUtc) =>
            OturumAcAsync(
                AktifKullaniciAdi,
                User.FindFirstValue(ClaimTypes.Role) ?? "Personel",
                User.HasClaim(YetkiSabitleri.AdminClaim, "true"),
                bitisUtc);

        // Depo ek şifresi girildi mi ve süresi dolmadı mı? (Eskiden "DepoSifreOnayli=true" düz çerezi, elle yazılabiliyordu)
        private bool DepoOnayliMi()
        {
            var deger = User.FindFirstValue(YetkiSabitleri.DepoOnayBitisClaim);
            return DateTime.TryParse(deger, null, System.Globalization.DateTimeStyles.RoundtripKind, out var bitis)
                   && bitis > DateTime.UtcNow;
        }

        // Sadece resim yüklenebilsin: aksi halde wwwroot'a .html/.js gibi dosyalar yüklenip çalıştırılabilirdi
        private static readonly string[] IzinliResimUzantilari = { ".jpg", ".jpeg", ".png", ".webp" };
        private static bool ResimDosyasiMi(IFormFile dosya) =>
            dosya.Length <= 5 * 1024 * 1024
            && IzinliResimUzantilari.Contains(Path.GetExtension(dosya.FileName).ToLowerInvariant());

        [AllowAnonymous]
        [HttpGet]
        public IActionResult YetkisizErisim()
        {
            TempData["HataMesaji"] = "Bu işlem için yetkiniz yok.";
            return RedirectToAction("Index");
        }

        // ==========================================
        // ARIZA MODÜLÜ & ANA PANEL
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            var veriler = _context.Arizalar.ToList();

            // Ana panel özet kartları için istatistikler
            ViewBag.ToplamAriza = veriler.Count;
            ViewBag.BekleyenAriza = veriler.Count(x => x.Durum == "BEKLİYO" || x.Durum == "BEKLİYOR" || x.Durum == "Beklemede");
            ViewBag.HazirAriza = veriler.Count(x => x.Durum == "HAZIR" || x.Durum == "Hazır");
            ViewBag.CozulenAriza = veriler.Count(x => x.Durum == "ÇÖZÜLDÜ" || x.Durum == "Çözüldü" || x.Durum == "Tamamlandı");

            // 📊 GRAFİK VERİLERİ (Chart.js İçin)
            var departmanGrafik = veriler
                .GroupBy(x => string.IsNullOrEmpty(x.Departman) ? "Diğer Birimler" : x.Departman)
                .Select(g => new { Departman = g.Key, Sayi = g.Count() })
                .Take(5)
                .ToList();

            ViewBag.GrafikEtiketler = System.Text.Json.JsonSerializer.Serialize(departmanGrafik.Select(x => x.Departman));
            ViewBag.GrafikVeriler = System.Text.Json.JsonSerializer.Serialize(departmanGrafik.Select(x => x.Sayi));

            return View(veriler);
        }

        // Tek tıkla arızayı çözüldü yapma metodu + OTOMATİK LOG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DurumCozulduYap(int id)
        {
            var ariza = _context.Arizalar.Find(id);
            if (ariza != null)
            {
                ariza.Durum = "ÇÖZÜLDÜ";

                // 📝 LOG KAYDI OLUŞTURULUYOR
                var log = new ArizaLog
                {
                    ArizaId = ariza.Id,
                    IslemYapan = AktifKullaniciAdi,
                    YapilanIslem = "Arıza durumu 'ÇÖZÜLDÜ' olarak güncellendi.",
                    Tarih = DateTime.Now
                };
                _context.ArizaLoglari.Add(log);

                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ArizaEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ArizaEkle(Ariza ariza)
        {
            _context.Arizalar.Add(ariza);
            _context.SaveChanges();

            // 📝 İLK KAYIT LOGU OLUŞTURULUYOR
            var log = new ArizaLog
            {
                ArizaId = ariza.Id,
                IslemYapan = string.IsNullOrEmpty(ariza.ArizayiAlan) ? "Sistem" : ariza.ArizayiAlan,
                YapilanIslem = "Yeni arıza kaydı oluşturuldu.",
                Tarih = DateTime.Now
            };
            _context.ArizaLoglari.Add(log);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ArizaDuzenle(int id)
        {
            var veri = _context.Arizalar.Find(id);
            if (veri == null)
            {
                return NotFound();
            }
            return View(veri);
        }

        [HttpPost]
        public IActionResult ArizaDuzenle(Ariza ariza)
        {
            var mevcutAriza = _context.Arizalar.Find(ariza.Id);
            if (mevcutAriza != null)
            {
                mevcutAriza.PersonelNo = ariza.PersonelNo;
                mevcutAriza.AdSoyad = ariza.AdSoyad;
                mevcutAriza.Departman = ariza.Departman;
                mevcutAriza.CihazTuru = ariza.CihazTuru;
                mevcutAriza.AciliyetDurumu = ariza.AciliyetDurumu;
                mevcutAriza.HataAciklaması = ariza.HataAciklaması;
                mevcutAriza.ArizayiAlan = ariza.ArizayiAlan;
                mevcutAriza.Durum = ariza.Durum;

                // 📝 DÜZENLEME LOGU OLUŞTURULUYOR
                var log = new ArizaLog
                {
                    ArizaId = mevcutAriza.Id,
                    IslemYapan = AktifKullaniciAdi,
                    YapilanIslem = $"Arıza bilgileri güncellendi. Yeni Durum: {ariza.Durum}",
                    Tarih = DateTime.Now
                };
                _context.ArizaLoglari.Add(log);

                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ArizaSil(int id)
        {
            var veri = _context.Arizalar.Find(id);
            if (veri != null)
            {
                _context.Arizalar.Remove(veri);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Arıza Hareket / İşlem Geçmişini Listeleme
        [HttpGet]
        public IActionResult ArizaGecmisi(int id)
        {
            var loglar = _context.ArizaLoglari.Where(x => x.ArizaId == id).OrderByDescending(x => x.Tarih).ToList();
            ViewBag.ArizaId = id;
            return View(loglar);
        }


        // ==========================================
        // GELİŞMİŞ DONANIM ENVANTERİ MODÜLÜ
        // ==========================================

        [HttpGet]
        public IActionResult EnvanterListesi()
        {
            return RedirectToAction("DepoYonetimi", new { sekme = "envanter" });
        }

        [HttpGet]
        public IActionResult EnvanterEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnvanterEkle(Envanter envanter)
        {
            try
            {
                // 📸 FOTOĞRAF YÜKLEME KONTROLÜ
                if (envanter.FotografDosyasi != null && envanter.FotografDosyasi.Length > 0)
                {
                    if (!ResimDosyasiMi(envanter.FotografDosyasi))
                    {
                        ViewBag.HataMesaji = "Sadece .jpg, .jpeg, .png veya .webp uzantılı ve 5 MB'tan küçük resim yükleyebilirsiniz.";
                        return View(envanter);
                    }

                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string klasorYolu = Path.Combine(wwwRootPath, "img", "envanter");

                    if (!Directory.Exists(klasorYolu))
                    {
                        Directory.CreateDirectory(klasorYolu);
                    }

                    string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(envanter.FotografDosyasi.FileName).ToLowerInvariant();
                    string tamYol = Path.Combine(klasorYolu, dosyaAdi);

                    using (var fileStream = new FileStream(tamYol, FileMode.Create))
                    {
                        envanter.FotografDosyasi.CopyTo(fileStream);
                    }

                    envanter.FotografYolu = dosyaAdi;
                }
                else
                {
                    envanter.FotografYolu = "varsayilan.png";
                }

                _context.Envanterler.Add(envanter);
                _context.SaveChanges();

                return RedirectToAction("DepoYonetimi", new { sekme = "envanter" });
            }
            catch (Exception ex)
            {
                ViewBag.HataMesaji = "Kayıt hatası: " + ex.Message;
                return View(envanter);
            }
        }

        [HttpGet]
        public IActionResult EnvanterDuzenle(int id)
        {
            var veri = _context.Envanterler.Find(id);
            if (veri == null)
            {
                return NotFound();
            }
            return View(veri);
        }

        [HttpPost]
        public IActionResult EnvanterDuzenle(Envanter envanter)
        {
            var mevcutVeri = _context.Envanterler.Find(envanter.Id);
            if (mevcutVeri == null) return NotFound();

            mevcutVeri.MalzemeAdi = envanter.MalzemeAdi;
            mevcutVeri.MarkaModel = envanter.MarkaModel;
            mevcutVeri.Adet = envanter.Adet;
            mevcutVeri.Aciklama = envanter.Aciklama;

            if (envanter.FotografDosyasi != null && envanter.FotografDosyasi.Length > 0)
            {
                if (!ResimDosyasiMi(envanter.FotografDosyasi))
                {
                    ViewBag.HataMesaji = "Sadece .jpg, .jpeg, .png veya .webp uzantılı ve 5 MB'tan küçük resim yükleyebilirsiniz.";
                    return View(mevcutVeri);
                }

                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string klasorYolu = Path.Combine(wwwRootPath, "img", "envanter");

                string yeniDosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(envanter.FotografDosyasi.FileName).ToLowerInvariant();
                string tamYol = Path.Combine(klasorYolu, yeniDosyaAdi);

                using (var fileStream = new FileStream(tamYol, FileMode.Create))
                {
                    fileStream.Flush();
                    envanter.FotografDosyasi.CopyTo(fileStream);
                }

                if (!string.IsNullOrEmpty(mevcutVeri.FotografYolu) && mevcutVeri.FotografYolu != "varsayilan.png")
                {
                    string eskiDosyaYolu = Path.Combine(klasorYolu, mevcutVeri.FotografYolu);
                    if (System.IO.File.Exists(eskiDosyaYolu))
                    {
                        System.IO.File.Delete(eskiDosyaYolu);
                    }
                }

                mevcutVeri.FotografYolu = yeniDosyaAdi;
            }

            _context.SaveChanges();
            return RedirectToAction("DepoYonetimi", new { sekme = "envanter" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnvanterSil(int id)
        {
            var veri = _context.Envanterler.Find(id);
            if (veri != null)
            {
                if (!string.IsNullOrEmpty(veri.FotografYolu) && veri.FotografYolu != "varsayilan.png")
                {
                    string dosyaYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "envanter", veri.FotografYolu);
                    if (System.IO.File.Exists(dosyaYolu))
                    {
                        System.IO.File.Delete(dosyaYolu);
                    }
                }

                _context.Envanterler.Remove(veri);
                _context.SaveChanges();
            }
            return RedirectToAction("DepoYonetimi", new { sekme = "envanter" });
        }


        // ==========================================
        // BİLGİ İŞLEM PERSONEL (TEKNİSYEN) YÖNETİMİ
        // ==========================================

        [HttpGet]
        public IActionResult PersonelListesi()
        {
            var teknisyenler = _context.Teknisyenler.ToList();
            return View(teknisyenler);
        }

        [HttpGet]
        public IActionResult PersonelEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PersonelEkle(Teknisyen teknisyen)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(teknisyen.Sifre))
                    teknisyen.Sifre = SifreServisi.Hashle(teknisyen.Sifre);

                _context.Teknisyenler.Add(teknisyen);
                _context.SaveChanges();
                return RedirectToAction("PersonelListesi");
            }
            return View(teknisyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public IActionResult PersonelSil(int id)
        {
            var veri = _context.Teknisyenler.Find(id);
            if (veri != null)
            {
                _context.Teknisyenler.Remove(veri);
                _context.SaveChanges();
            }
            return RedirectToAction("PersonelListesi");
        }

        // ==========================================
        // YÖNETİCİ ONAY SİSTEMİ
        // ==========================================

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult OnayBekleyenler()
        {
            var bekleyenler = _context.Teknisyenler.Where(x => x.EmailOnayliMi && !x.YoneticiOnayliMi).ToList();
            return View(bekleyenler);
        }

        // Yöneticiye giden onay mailindeki link buraya geldiği için GET kaldı; ama artık sadece admin çalıştırabilir.
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult KullaniciOnayla(int id)
        {
            var kisi = _context.Teknisyenler.Find(id);
            if (kisi == null)
            {
                TempData["HataMesaji"] = "Onaylanacak kullanıcı bulunamadı.";
                return RedirectToAction("PersonelListesi");
            }

            kisi.YoneticiOnayliMi = true;
            _context.SaveChanges();
            TempData["BasariMesaji"] = $"{kisi.AdSoyad} ({kisi.Email}) onaylandı, artık giriş yapabilir.";
            return RedirectToAction("PersonelListesi");
        }


        // ==========================================
        // GERÇEK ÜYELİK, E-POSTA OTP ONAYI, ŞİFRE SIFIRLAMA VE ROL SİSTEMİ
        // ==========================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre, string? returnUrl = null)
        {
            kullaniciAdi = kullaniciAdi?.Trim() ?? string.Empty;
            sifre ??= string.Empty;

            // 1. SÜPER ADMİN GİRİŞİ — kullanıcı adı ve şifre artık koddan değil ayarlardan geliyor.
            //    Ayar boşsa admin girişi tamamen kapalıdır.
            if (!string.IsNullOrEmpty(_yonetim.AdminKullaniciAdi) && !string.IsNullOrEmpty(_yonetim.AdminSifre)
                && kullaniciAdi == _yonetim.AdminKullaniciAdi
                && SifreServisi.SabitZamanliEsit(sifre, _yonetim.AdminSifre))
            {
                var adminPersonel = _context.Teknisyenler.FirstOrDefault(x => x.AdSoyad.Contains("Muhammed Bilal") || x.Email.Contains("bilal"));

                string gercekAd = adminPersonel != null ? adminPersonel.AdSoyad : "Muhammed Bilal Kılıç";
                string gercekRol = adminPersonel != null && !string.IsNullOrEmpty(adminPersonel.Unvan) ? adminPersonel.Unvan : "Sistem Yöneticisi";

                await OturumAcAsync(gercekAd, gercekRol, admin: true, depoOnayBitisUtc: null);
                return GuvenliYonlendir(returnUrl);
            }

            // 2. NORMAL PERSONEL & DEPO SORUMLUSU GİRİŞİ
            //    Şifre artık SQL sorgusunda düz metin olarak aranmıyor; önce kullanıcı bulunuyor, sonra hash doğrulanıyor.
            var adaylar = _context.Teknisyenler
                .Where(x => x.Email == kullaniciAdi || x.SicilNo == kullaniciAdi || x.AdSoyad == kullaniciAdi)
                .ToList();

            Teknisyen? personel = null;
            foreach (var aday in adaylar)
            {
                if (SifreServisi.Dogrula(aday.Sifre, sifre, out bool yenidenHashle))
                {
                    personel = aday;
                    if (yenidenHashle)
                    {
                        // Eski düz şifreli kayıt → ilk başarılı girişte otomatik hash'e çevrilir
                        aday.Sifre = SifreServisi.Hashle(sifre);
                        _context.SaveChanges();
                    }
                    break;
                }
            }

            if (personel != null)
            {
                if (!personel.EmailOnayliMi)
                {
                    ViewBag.HataMesaji = "E-posta adresiniz henüz doğrulanmamış! Lütfen mailinize gelen kodu onaylayın.";
                    return View();
                }

                if (!personel.YoneticiOnayliMi)
                {
                    ViewBag.HataMesaji = "Hesabınız henüz Bilgi İşlem Yöneticisi tarafından onaylanmadı. Lütfen birim yetkilisi ile iletişime geçin.";
                    return View();
                }

                string rol = (personel.Unvan != null && personel.Unvan.Contains("Depo")) ? "Depo Sorumlusu" : (string.IsNullOrEmpty(personel.Unvan) ? "Teknisyen" : personel.Unvan);

                await OturumAcAsync(personel.AdSoyad, rol, admin: false, depoOnayBitisUtc: null);
                return GuvenliYonlendir(returnUrl);
            }

            ViewBag.HataMesaji = "Kullanıcı adı, e-posta veya şifre hatalı!";
            return View();
        }

        // Girişten sonra sadece kendi sitemizdeki bir adrese dön (open redirect engeli)
        private IActionResult GuvenliYonlendir(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index");

        [AllowAnonymous]
        [HttpGet]
        public IActionResult KayitOl()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult KayitOl(Teknisyen teknisyen)
        {
            if (string.IsNullOrWhiteSpace(teknisyen.Sifre) || teknisyen.Sifre.Length < 6)
            {
                ViewBag.HataMesaji = "Şifre en az 6 karakter olmalıdır!";
                return View(teknisyen);
            }

            if (!string.IsNullOrEmpty(teknisyen.Email))
            {
                var varMi = _context.Teknisyenler.FirstOrDefault(x => x.Email == teknisyen.Email);
                if (varMi != null)
                {
                    ViewBag.HataMesaji = "Bu e-posta adresiyle zaten kayıtlı bir kullanıcı var!";
                    return View(teknisyen);
                }
            }

            // Tahmin edilemeyen kod için kriptografik rastgele sayı (new Random() tahmin edilebilir)
            string rastgeleKod = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            teknisyen.SicilNo = string.IsNullOrEmpty(teknisyen.SicilNo) ? "PERS-" + new Random().Next(1000, 9999) : teknisyen.SicilNo;
            teknisyen.AdSoyad = string.IsNullOrEmpty(teknisyen.AdSoyad) ? "Yeni Teknisyen" : teknisyen.AdSoyad;
            teknisyen.Unvan = string.IsNullOrEmpty(teknisyen.Unvan) ? "Teknik Personel" : teknisyen.Unvan;
            teknisyen.Telefon = string.IsNullOrEmpty(teknisyen.Telefon) ? "-" : teknisyen.Telefon;
            teknisyen.Email = string.IsNullOrEmpty(teknisyen.Email) ? "" : teknisyen.Email;
            teknisyen.Sifre = SifreServisi.Hashle(teknisyen.Sifre); // Düz metin değil, hash saklanır

            teknisyen.EmailOnayKodu = rastgeleKod;
            teknisyen.EmailOnayliMi = false;
            teknisyen.YoneticiOnayliMi = false;

            try
            {
                _context.Teknisyenler.Add(teknisyen);
                _context.SaveChanges();

                _email.OnayKoduGonder(teknisyen.Email, rastgeleKod);

                TempData["Email"] = teknisyen.Email;
                return RedirectToAction("EmailDogrula");
            }
            catch (Exception)
            {
                ViewBag.HataMesaji = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                return View(teknisyen);
            }
        }

        // ==========================================
        // E-POSTA DOĞRULAMA (OTP) İŞLEMLERİ
        // ==========================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult EmailDogrula()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult EmailDogrula(string email, string kod)
        {
            var kullanici = string.IsNullOrEmpty(kod)
                ? null
                : _context.Teknisyenler.FirstOrDefault(x => x.Email == email && x.EmailOnayKodu == kod);

            if (kullanici != null)
            {
                kullanici.EmailOnayliMi = true;
                kullanici.EmailOnayKodu = null;
                _context.SaveChanges();

                // 📩 YÖNETİCİYE OTOMATİK BİLDİRİM MAİLİ GÖNDERİLİYOR
                string onayLink = Url.Action("KullaniciOnayla", "Home", new { id = kullanici.Id }, Request.Scheme);
                if (!string.IsNullOrEmpty(_yonetim.YoneticiEmail))
                    _email.YoneticiyeOnayMailiGonder(_yonetim.YoneticiEmail, kullanici.AdSoyad, kullanici.Email, onayLink);

                TempData["BasariMesaji"] = "E-posta adresiniz başarıyla doğrulandı! 🎉 Hesabınız şu an Yönetici Onayı bekliyor. Yönetici onayladıktan sonra giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.HataMesaji = "Girdiğiniz 6 haneli onay kodu hatalı!";
            ViewBag.Email = email;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SifremiUnuttum()
        {
            // SifreSifirla'dan süresi dolmuş/geçersiz link nedeniyle yönlendirildiyse mesajı göster
            if (TempData["HataMesaji"] is string hata) ViewBag.HataMesaji = hata;
            return View();
        }

        private static readonly TimeSpan ResetTokenSuresi = TimeSpan.FromHours(1);
        private const string ResetLinkSuresiDolduMesaji = "Linkin süresi dolmuş, tekrar şifre sıfırlama isteyin.";

        // Token'a ait kullanıcıyı döner; token yok/bilinmiyor/süresi dolmuşsa null.
        // Süresi dolmuş token veritabanından temizlenir.
        private Teknisyen? ResetTokenKullanicisi(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var kullanici = _context.Teknisyenler.FirstOrDefault(x => x.ResetsToken == token);
            if (kullanici == null) return null;

            if (kullanici.ResetsTokenBitis == null || kullanici.ResetsTokenBitis <= DateTime.UtcNow)
            {
                kullanici.ResetsToken = null;
                kullanici.ResetsTokenBitis = null;
                _context.SaveChanges();
                return null;
            }
            return kullanici;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SifremiUnuttum(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.HataMesaji = "Lütfen geçerli bir e-posta adresi girin!";
                return View();
            }

            // 🌟 HERKESİ KAPSAYAN AKILLI ARAMA (Büyük/küçük harf ve boşlukları temizler)
            string temizEmail = email.Trim().ToLower();
            var kullanici = _context.Teknisyenler.FirstOrDefault(x => x.Email != null && x.Email.Trim().ToLower() == temizEmail);

            if (kullanici == null)
            {
                ViewBag.HataMesaji = "Sistemde kayıtlı böyle bir e-posta adresi bulunamadı!";
                return View();
            }

            // Kriptografik olarak güvenli 256 bit rastgele token (URL'de sorunsuz taşınması için hex)
            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            kullanici.ResetsToken = token;
            kullanici.ResetsTokenBitis = DateTime.UtcNow.Add(ResetTokenSuresi);
            _context.SaveChanges();

            string resetLink = Url.Action("SifreSifirla", "Home", new { token = token }, Request.Scheme);

            bool mailGonderildi = _email.SifreSifirlamaMailiGonder(kullanici.Email, resetLink);

            if (mailGonderildi)
            {
                ViewBag.BasariMesaji = $"{kullanici.Email} adresinize şifre sıfırlama bağlantısı gönderildi! Lütfen gelen kutunuzu (veya Spam klasörünü) kontrol edin.";
            }
            else
            {
                ViewBag.HataMesaji = "E-posta gönderilirken bir sunucu hatası oluştu. Lütfen Gmail SMTP ayarlarınızı kontrol edin.";
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SifreSifirla(string token)
        {
            if (ResetTokenKullanicisi(token) == null)
            {
                TempData["HataMesaji"] = ResetLinkSuresiDolduMesaji;
                return RedirectToAction("SifremiUnuttum");
            }

            ViewBag.Token = token;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SifreSifirla(string token, string yeniSifre)
        {
            if (string.IsNullOrWhiteSpace(yeniSifre) || yeniSifre.Length < 6)
            {
                ViewBag.HataMesaji = "Yeni şifre en az 6 karakter olmalıdır!";
                ViewBag.Token = token;
                return View();
            }

            var kullanici = ResetTokenKullanicisi(token);
            if (kullanici == null)
            {
                TempData["HataMesaji"] = ResetLinkSuresiDolduMesaji;
                return RedirectToAction("SifremiUnuttum");
            }

            kullanici.Sifre = SifreServisi.Hashle(yeniSifre);
            kullanici.ResetsToken = null;
            kullanici.ResetsTokenBitis = null;
            _context.SaveChanges();

            TempData["BasariMesaji"] = "Şifreniz başarıyla güncellendi! Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CikisYap()
        {
            // Oturum çerezini (yetkiler ve depo onayı dahil) siliyoruz.
            // Eski sürümden kalan düz çerezler de temizleniyor.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("KullaniciRol");
            Response.Cookies.Delete("KullaniciAd");
            Response.Cookies.Delete("DepoSifreOnayli");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet("/Login/CikisYap")]
        public Task<IActionResult> GizliCikisYap() => CikisYap();


        // ==========================================
        // DİNAMİK & AYRI AYRI RAPORLAMA MODÜLÜ
        // ==========================================

        [HttpGet]
        public IActionResult AySonuRaporu()
        {
            return RedirectToAction("RaporDetay", new { durum = "Tumu" });
        }

        [HttpGet]
        public IActionResult RaporDetay(string durum)
        {
            var tumArizalar = _context.Arizalar.AsQueryable();
            List<Ariza> filtrelenmisList;

            if (durum == "Bekleyen")
            {
                filtrelenmisList = tumArizalar.Where(x => x.Durum == "BEKLİYO" || x.Durum == "BEKLİYOR" || x.Durum == "Beklemede").ToList();
                ViewBag.RaporBasligi = "⏳ Bekleyen Arızalar Raporu";
            }
            else if (durum == "Hazir")
            {
                filtrelenmisList = tumArizalar.Where(x => x.Durum == "HAZIR" || x.Durum == "Hazır").ToList();
                ViewBag.RaporBasligi = "📦 Hazır Durumdaki Arızalar Raporu";
            }
            else if (durum == "Cozulen")
            {
                filtrelenmisList = tumArizalar.Where(x => x.Durum == "ÇÖZÜLDÜ" || x.Durum == "Çözüldü" || x.Durum == "Tamamlandı").ToList();
                ViewBag.RaporBasligi = "✅ Çözülen Arızalar Raporu";
            }
            else
            {
                filtrelenmisList = tumArizalar.ToList();
                ViewBag.RaporBasligi = "📊 Genel Tüm Arızalar Raporu";
            }

            // Sayısal İstatistikler
            ViewBag.ToplamAriza = _context.Arizalar.Count();
            ViewBag.BekleyenAriza = _context.Arizalar.Count(x => x.Durum == "BEKLİYO" || x.Durum == "BEKLİYOR" || x.Durum == "Beklemede");
            ViewBag.HazirAriza = _context.Arizalar.Count(x => x.Durum == "HAZIR" || x.Durum == "Hazır");
            ViewBag.CozulenAriza = _context.Arizalar.Count(x => x.Durum == "ÇÖZÜLDÜ" || x.Durum == "Çözüldü" || x.Durum == "Tamamlandı");

            ViewBag.AktifDurum = durum;

            return View("RaporDetay", filtrelenmisList);
        }

        // ==========================================
        // BİLGİ İŞLEM DEPO & STOK YÖNETİMİ MODÜLÜ (ŞİFRE KORUMALI)
        // ==========================================

        [HttpGet]
        public IActionResult DepoYonetimi(string sekme = "hareketler", int? yil = null, int? ay = null, string mudurluk = "")
        {
            // 🌟 1. KONTROL: Giriş zorunluluğu artık [Authorize] ile sağlanıyor.

            // 🔐 2. KONTROL: TEK ENGEL ÖZEL ŞİFRE (Şifreyi bilen içeri girer)
            bool sifreGerekli = !DepoOnayliMi();

            if (sifreGerekli)
            {
                // Şifre girilmeden depo verileri sayfaya hiç gönderilmez (eskiden HTML'e gömülüp sadece gizleniyordu olabilir)
                ViewBag.Envanterler = new List<Envanter>();
                ViewBag.ToplamGiris = 0;
                ViewBag.ToplamCikis = 0;
                ViewBag.KalanStok = 0;
                ViewBag.KritikStokSayisi = 0;
                ViewBag.SifreGerekli = true;
                ViewBag.AktifSekme = sekme;
                ViewBag.SecilenYil = yil ?? DateTime.Now.Year;
                ViewBag.SecilenAy = ay ?? 0;
                ViewBag.SecilenMudurluk = mudurluk;
                return View(new List<DepoHareket>());
            }

            var hareketlerQuery = _context.DepoHareketleri.AsQueryable();

            // 🗓️ YIL VE AY FİLTRESİ
            int secilenYil = yil ?? DateTime.Now.Year;
            hareketlerQuery = hareketlerQuery.Where(x => x.Tarih.Year == secilenYil);

            if (ay.HasValue && ay.Value > 0)
            {
                hareketlerQuery = hareketlerQuery.Where(x => x.Tarih.Month == ay.Value);
            }

            if (!string.IsNullOrEmpty(mudurluk))
            {
                hareketlerQuery = hareketlerQuery.Where(x => x.Mudurluk.Contains(mudurluk));
            }

            var hareketListesi = hareketlerQuery.OrderByDescending(x => x.Tarih).ToList();

            // Depo Donanım Envanteri Listesini Çekiyoruz
            var envanterler = _context.Envanterler.ToList();
            ViewBag.Envanterler = envanterler;

            // Depo Özet İstatistik Hesaplamaları
            ViewBag.ToplamGiris = hareketListesi.Where(x => x.IslemTuru == "GİRİŞ").Sum(x => x.Adet);
            ViewBag.ToplamCikis = hareketListesi.Where(x => x.IslemTuru == "ÇIKIŞ" || x.IslemTuru == "DEĞİŞİM").Sum(x => x.Adet);
            ViewBag.KalanStok = ViewBag.ToplamGiris - ViewBag.ToplamCikis;

            // 🚨 KRİTİK STOK UYARISI HESAPLAMA
            ViewBag.KritikStokSayisi = envanterler.Count(x => x.Adet <= 2);

            // Şifre Ekranı İçin Parametre
            ViewBag.SifreGerekli = sifreGerekli;

            // Görünüme Parametreleri Aktarma
            ViewBag.AktifSekme = sekme;
            ViewBag.SecilenYil = secilenYil;
            ViewBag.SecilenAy = ay ?? 0;
            ViewBag.SecilenMudurluk = mudurluk;

            return View(hareketListesi);
        }

        // 🔐 DEPO ŞİFRE GİRİŞ METODU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepoGirisYap(string depoSifresi)
        {
            // Depo şifresi artık koddan değil ayarlardan (Yonetim:DepoSifresi) okunuyor
            if (!string.IsNullOrEmpty(_yonetim.DepoSifresi)
                && SifreServisi.SabitZamanliEsit(depoSifresi ?? string.Empty, _yonetim.DepoSifresi))
            {
                // Onay, şifreli oturum çerezine 2 saatlik claim olarak yazılıyor (elle taklit edilemez)
                await DepoOnayiniAyarlaAsync(DateTime.UtcNow.AddHours(2));
                return RedirectToAction("DepoYonetimi");
            }

            TempData["HataMesaji"] = "Girilen Depo Özel Şifresi Hatalı!";
            return RedirectToAction("DepoYonetimi");
        }

        // 🔐 DEPO OTURUMUNU KAPATMA METODU
        [HttpGet]
        public async Task<IActionResult> DepoCikisYap()
        {
            await DepoOnayiniAyarlaAsync(null);
            Response.Cookies.Delete("DepoSifreOnayli"); // eski sürümden kalan çerez
            return RedirectToAction("Index");
        }

        // ==============================================================
        // 🚨 DEPO HAREKET EKLE & STOK TAKİBİ & MAİL 🚨
        // ==============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DepoHareketEkle(DepoHareket hareket)
        {
            // İşlem yapmadan önce depo şifre kontrolü (giriş kontrolü [Authorize] ile)
            if (!DepoOnayliMi())
            {
                return RedirectToAction("DepoYonetimi");
            }

            try
            {
                // 1. Hareketi veritabanına ekle
                hareket.Tarih = DateTime.Now;
                _context.DepoHareketleri.Add(hareket);

                // 2. İşlem türüne göre ENVANTER (Stok) Güncellemesi Yap
                var envanter = _context.Envanterler.FirstOrDefault(x => x.BarkodNo == hareket.BarkodNo || x.MalzemeAdi == hareket.MalzemeAdi);

                if (envanter != null)
                {
                    if (hareket.IslemTuru == "ÇIKIŞ" || hareket.IslemTuru == "DEĞİŞİM")
                    {
                        envanter.Adet -= hareket.Adet;
                        if (envanter.Adet < 0) envanter.Adet = 0; // Stok eksiye düşmesin

                        // 3. KRİTİK STOK KONTROLÜ VE MAİL TETİKLEME
                        if (envanter.Adet <= 2 && !string.IsNullOrEmpty(_yonetim.DepoSorumlusuEmail))
                        {
                            // Alıcı adresi artık koddan değil ayarlardan (Yonetim:DepoSorumlusuEmail)
                            _email.KritikStokMailiGonder(envanter.MalzemeAdi, envanter.BarkodNo, envanter.Adet, _yonetim.DepoSorumlusuEmail);
                        }
                    }
                    else if (hareket.IslemTuru == "GİRİŞ")
                    {
                        envanter.Adet += hareket.Adet;
                    }
                }

                _context.SaveChanges();
                TempData["BasariMesaji"] = "Depo hareketi başarıyla işlendi ve stoklar güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["HataMesaji"] = "Kayıt sırasında bir hata oluştu: " + ex.GetType().Name;
            }

            return RedirectToAction("DepoYonetimi", new { sekme = "hareketler" });
        }

        // ==========================================
        // EXCEL DIŞA AKTARMA (EXPORT TO EXCEL)
        // ==========================================

        [HttpGet]
        public IActionResult DepoExcelAktar()
        {
            if (!DepoOnayliMi()) return RedirectToAction("DepoYonetimi");

            var hareketler = _context.DepoHareketleri.OrderByDescending(x => x.Tarih).ToList();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Tarih;İşlem Türü;Malzeme Adı;Adet;Müdürlük;Personel;Açıklama");

            foreach (var item in hareketler)
            {
                builder.AppendLine($"{item.Tarih:dd.MM.yyyy HH:mm};{item.IslemTuru};{item.MalzemeAdi};{item.Adet};{item.Mudurluk};{item.PersonelAdSoyad};{item.Aciklama}");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"Depo_Raporu_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        // ==========================================
        // MANUEL VERİTABANI YEDEKLEME (BACKUP)
        // ==========================================

        // Eskiden: giriş kontrolü yoktu ve yedek wwwroot/backups içine yazılıyordu (linki bilen herkes indirebiliyordu).
        // Şimdi: giriş + depo şifresi gerekli, dosya diske yazılmadan doğrudan indiriliyor.
        [HttpGet]
        public IActionResult VeritabaniYedekAl()
        {
            if (!DepoOnayliMi()) return RedirectToAction("DepoYonetimi");

            try
            {
                string dosyaAdi = $"BilgiIslem_SistemYedek_{DateTime.Now:yyyyMMdd_HHmm}.json";

                var yedekPaketi = new
                {
                    YedekTarihi = DateTime.Now,
                    Arizalar = _context.Arizalar.ToList(),
                    Envanterler = _context.Envanterler.ToList(),
                    DepoHareketleri = _context.DepoHareketleri.ToList(),
                    Teknisyenler = _context.Teknisyenler.Select(x => new { x.Id, x.AdSoyad, x.Email, x.SicilNo, x.Unvan }).ToList()
                };

                byte[] fileBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(yedekPaketi, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                return File(fileBytes, "application/json", dosyaAdi);
            }
            catch (Exception)
            {
                TempData["HataMesaji"] = "Yedek oluşturulurken bir hata oluştu.";
                return RedirectToAction("DepoYonetimi");
            }
        }

        [HttpGet]
        public IActionResult TutanakYazdir(int id)
        {
            var hareket = _context.DepoHareketleri.Find(id);
            if (hareket == null)
            {
                return NotFound("Kayıt bulunamadı.");
            }
            return View(hareket);
        }
    }
}