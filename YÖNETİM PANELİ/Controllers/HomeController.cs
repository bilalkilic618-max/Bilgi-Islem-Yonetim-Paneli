using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Yönetim_Paneli.Models;
using Yönetim_Paneli.Services;

namespace Yönetim_Paneli.Controllers
{
    public class HomeController : Controller
    {
        private readonly UygulamaDbContext _context;

        public HomeController(UygulamaDbContext context)
        {
            _context = context;
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
        [HttpGet]
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
                    IslemYapan = Request.Cookies["KullaniciAd"] ?? "Sistem",
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
                    IslemYapan = Request.Cookies["KullaniciAd"] ?? "Sistem",
                    YapilanIslem = $"Arıza bilgileri güncellendi. Yeni Durum: {ariza.Durum}",
                    Tarih = DateTime.Now
                };
                _context.ArizaLoglari.Add(log);

                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
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
                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string klasorYolu = Path.Combine(wwwRootPath, "img", "envanter");

                    if (!Directory.Exists(klasorYolu))
                    {
                        Directory.CreateDirectory(klasorYolu);
                    }

                    string dosyaAdi = Guid.NewGuid().ToString() + "_" + Path.GetFileName(envanter.FotografDosyasi.FileName);
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
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string klasorYolu = Path.Combine(wwwRootPath, "img", "envanter");

                string yeniDosyaAdi = Guid.NewGuid().ToString() + "_" + Path.GetFileName(envanter.FotografDosyasi.FileName);
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

        [HttpGet]
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
                _context.Teknisyenler.Add(teknisyen);
                _context.SaveChanges();
                return RedirectToAction("PersonelListesi");
            }
            return View(teknisyen);
        }

        [HttpGet]
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
        public IActionResult OnayBekleyenler()
        {
            var bekleyenler = _context.Teknisyenler.Where(x => x.EmailOnayliMi && !x.YoneticiOnayliMi).ToList();
            return View(bekleyenler);
        }

        [HttpGet]
        public IActionResult KullaniciOnayla(int id)
        {
            var kisi = _context.Teknisyenler.Find(id);
            if (kisi != null)
            {
                kisi.YoneticiOnayliMi = true;
                _context.SaveChanges();
            }
            return RedirectToAction("PersonelListesi");
        }


        // ==========================================
        // GERÇEK ÜYELİK, E-POSTA OTP ONAYI, ŞİFRE SIFIRLAMA VE ROL SİSTEMİ
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string kullaniciAdi, string sifre)
        {
            // 1. SÜPER ADMİN GİRİŞİ (Veritabanındaki gerçek adını ve unvanını otomatik çeker)
            if (kullaniciAdi == "Teknik" && sifre == "2026*")
            {
                var adminPersonel = _context.Teknisyenler.FirstOrDefault(x => x.AdSoyad.Contains("Muhammed Bilal") || x.Email.Contains("bilal"));

                string gercekAd = adminPersonel != null ? adminPersonel.AdSoyad : "Muhammed Bilal Kılıç";
                string gercekRol = adminPersonel != null && !string.IsNullOrEmpty(adminPersonel.Unvan) ? adminPersonel.Unvan : "Sistem Yöneticisi";

                Response.Cookies.Append("KullaniciRol", gercekRol);
                Response.Cookies.Append("KullaniciAd", gercekAd);
                return RedirectToAction("Index");
            }

            // 2. NORMAL PERSONEL & LÜTFÜ ABİ GİRİŞİ
            var personel = _context.Teknisyenler.FirstOrDefault(x =>
                (x.Email == kullaniciAdi || x.SicilNo == kullaniciAdi || x.AdSoyad == kullaniciAdi) && x.Sifre == sifre);

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

                Response.Cookies.Append("KullaniciRol", rol);
                Response.Cookies.Append("KullaniciAd", personel.AdSoyad);

                return RedirectToAction("Index");
            }

            ViewBag.HataMesaji = "Kullanıcı adı, e-posta veya şifre hatalı!";
            return View();
        }

        [HttpGet]
        public IActionResult KayitOl()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KayitOl(Teknisyen teknisyen)
        {
            if (!string.IsNullOrEmpty(teknisyen.Email))
            {
                var varMi = _context.Teknisyenler.FirstOrDefault(x => x.Email == teknisyen.Email);
                if (varMi != null)
                {
                    ViewBag.HataMesaji = "Bu e-posta adresiyle zaten kayıtlı bir kullanıcı var!";
                    return View(teknisyen);
                }
            }

            string rastgeleKod = new Random().Next(100000, 999999).ToString();

            teknisyen.SicilNo = string.IsNullOrEmpty(teknisyen.SicilNo) ? "PERS-" + new Random().Next(1000, 9999) : teknisyen.SicilNo;
            teknisyen.AdSoyad = string.IsNullOrEmpty(teknisyen.AdSoyad) ? "Yeni Teknisyen" : teknisyen.AdSoyad;
            teknisyen.Unvan = string.IsNullOrEmpty(teknisyen.Unvan) ? "Teknik Personel" : teknisyen.Unvan;
            teknisyen.Telefon = string.IsNullOrEmpty(teknisyen.Telefon) ? "-" : teknisyen.Telefon;
            teknisyen.Email = string.IsNullOrEmpty(teknisyen.Email) ? "" : teknisyen.Email;
            teknisyen.Sifre = string.IsNullOrEmpty(teknisyen.Sifre) ? "123456" : teknisyen.Sifre;

            teknisyen.EmailOnayKodu = rastgeleKod;
            teknisyen.EmailOnayliMi = false;
            teknisyen.YoneticiOnayliMi = false;

            try
            {
                _context.Teknisyenler.Add(teknisyen);
                _context.SaveChanges();

                EmailService.OnayKoduGonder(teknisyen.Email, rastgeleKod);

                TempData["Email"] = teknisyen.Email;
                return RedirectToAction("EmailDogrula");
            }
            catch (Exception ex)
            {
                var icHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.HataMesaji = "SQL Hatası: " + icHata;
                return View(teknisyen);
            }
        }

        // ==========================================
        // E-POSTA DOĞRULAMA (OTP) İŞLEMLERİ
        // ==========================================

        [HttpGet]
        public IActionResult EmailDogrula()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        public IActionResult EmailDogrula(string email, string kod)
        {
            var kullanici = _context.Teknisyenler.FirstOrDefault(x => x.Email == email && x.EmailOnayKodu == kod);

            if (kullanici != null)
            {
                kullanici.EmailOnayliMi = true;
                kullanici.EmailOnayKodu = null;
                _context.SaveChanges();

                // 📩 YÖNETİCİYE OTOMATİK BİLDİRİM MAİLİ GÖNDERİLİYOR
                string onayLink = Url.Action("KullaniciOnayla", "Home", new { id = kullanici.Id }, Request.Scheme);
                EmailService.YoneticiyeOnayMailiGonder("bilalkilic618@gmail.com", kullanici.AdSoyad, kullanici.Email, onayLink);

                TempData["BasariMesaji"] = "E-posta adresiniz başarıyla doğrulandı! 🎉 Hesabınız şu an Yönetici Onayı bekliyor. Yönetici onayladıktan sonra giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.HataMesaji = "Girdiğiniz 6 haneli onay kodu hatalı!";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult SifremiUnuttum()
        {
            return View();
        }

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

            string token = Guid.NewGuid().ToString();
            kullanici.ResetsToken = token;
            _context.SaveChanges();

            string resetLink = Url.Action("SifreSifirla", "Home", new { token = token }, Request.Scheme);

            bool mailGonderildi = EmailService.SifreSifirlamaMailiGonder(kullanici.Email, resetLink);

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

        [HttpGet]
        public IActionResult SifreSifirla(string token)
        {
            var kullanici = _context.Teknisyenler.FirstOrDefault(x => x.ResetsToken == token);
            if (kullanici == null)
            {
                TempData["HataMesaji"] = "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş!";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult SifreSifirla(string token, string yeniSifre)
        {
            var kullanici = _context.Teknisyenler.FirstOrDefault(x => x.ResetsToken == token);
            if (kullanici != null)
            {
                kullanici.Sifre = yeniSifre;
                kullanici.ResetsToken = null;
                _context.SaveChanges();

                TempData["BasariMesaji"] = "Şifreniz başarıyla güncellendi! Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ViewBag.HataMesaji = "İşlem başarısız. Lütfen tekrar deneyin.";
            return View();
        }

        [HttpGet]
        public IActionResult CikisYap()
        {
            // Çıkış yaparken yetkileri ve depo şifre durumunu sıfırlıyoruz
            Response.Cookies.Delete("KullaniciRol");
            Response.Cookies.Delete("KullaniciAd");
            Response.Cookies.Delete("DepoSifreOnayli");
            return RedirectToAction("Login");
        }

        [HttpGet("/Login/CikisYap")]
        public IActionResult GizliCikisYap()
        {
            Response.Cookies.Delete("KullaniciRol");
            Response.Cookies.Delete("KullaniciAd");
            Response.Cookies.Delete("DepoSifreOnayli");
            return RedirectToAction("Login");
        }


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
            // 🌟 1. KONTROL: SADECE GİRİŞ YAPMIŞ OLMASI YETERLİ (Rol engeli kaldırıldı, herkes girebilir)
            var aktifAd = Request.Cookies["KullaniciAd"];
            if (string.IsNullOrEmpty(aktifAd))
            {
                TempData["HataMesaji"] = "Lütfen önce sisteme giriş yapın!";
                return RedirectToAction("Login");
            }

            // 🔐 2. KONTROL: TEK ENGEL ÖZEL ŞİFRE (Şifreyi bilen içeri girer)
            var depoSifreOnay = Request.Cookies["DepoSifreOnayli"];
            bool sifreGerekli = (depoSifreOnay != "true");

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
        public IActionResult DepoGirisYap(string depoSifresi)
        {
            // Depo özel şifresi (Varsayılan: 2026* veya senin belirlediğin şifre)
            if (depoSifresi == "2026*" || depoSifresi == "123456" || depoSifresi == "depo2026")
            {
                // Şifre doğruysa çerez kaydedip Depo sayfasına yönlendiriyoruz
                Response.Cookies.Append("DepoSifreOnayli", "true", new CookieOptions { Expires = DateTime.Now.AddHours(2) });
                return RedirectToAction("DepoYonetimi");
            }

            TempData["HataMesaji"] = "Girilen Depo Özel Şifresi Hatalı!";
            return RedirectToAction("DepoYonetimi");
        }

        // 🔐 DEPO OTURUMUNU KAPATMA METODU
        [HttpGet]
        public IActionResult DepoCikisYap()
        {
            Response.Cookies.Delete("DepoSifreOnayli");
            return RedirectToAction("Index");
        }

        // ==============================================================
        // 🚨 DEPO HAREKET EKLE & STOK TAKİBİ & MAİL 🚨
        // ==============================================================
        [HttpPost]
        public IActionResult DepoHareketEkle(DepoHareket hareket)
        {
            // İşlem yapmadan önce giriş ve depo şifre kontrolü
            var aktifAd = Request.Cookies["KullaniciAd"];
            var depoSifreOnay = Request.Cookies["DepoSifreOnayli"];

            if (string.IsNullOrEmpty(aktifAd) || depoSifreOnay != "true")
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
                        if (envanter.Adet <= 2)
                        {
                            string lutfuAbiMail = "lutfu.sevinc29@gmail.com";
                            EmailService.KritikStokMailiGonder(envanter.MalzemeAdi, envanter.BarkodNo, envanter.Adet, lutfuAbiMail);
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
                TempData["HataMesaji"] = "Kayıt hatası: " + ex.Message;
            }

            return RedirectToAction("DepoYonetimi", new { sekme = "hareketler" });
        }

        // ==========================================
        // EXCEL DIŞA AKTARMA (EXPORT TO EXCEL)
        // ==========================================

        [HttpGet]
        public IActionResult DepoExcelAktar()
        {
            var aktifAd = Request.Cookies["KullaniciAd"];
            if (string.IsNullOrEmpty(aktifAd)) return RedirectToAction("Login");

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

        [HttpGet]
        public IActionResult VeritabaniYedekAl()
        {
            try
            {
                string backupKlasor = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups");
                if (!Directory.Exists(backupKlasor))
                {
                    Directory.CreateDirectory(backupKlasor);
                }

                string dosyaAdi = $"BilgiIslem_SistemYedek_{DateTime.Now:yyyyMMdd_HHmm}.json";
                string tamYol = Path.Combine(backupKlasor, dosyaAdi);

                var yedekPaketi = new
                {
                    YedekTarihi = DateTime.Now,
                    Arizalar = _context.Arizalar.ToList(),
                    Envanterler = _context.Envanterler.ToList(),
                    DepoHareketleri = _context.DepoHareketleri.ToList(),
                    Teknisyenler = _context.Teknisyenler.Select(x => new { x.Id, x.AdSoyad, x.Email, x.SicilNo, x.Unvan }).ToList()
                };

                string jsonVeri = System.Text.Json.JsonSerializer.Serialize(yedekPaketi, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(tamYol, jsonVeri, System.Text.Encoding.UTF8);

                byte[] fileBytes = System.IO.File.ReadAllBytes(tamYol);
                return File(fileBytes, "application/json", dosyaAdi);
            }
            catch (Exception ex)
            {
                TempData["HataMesaji"] = "Yedek oluşturulurken hata oluştu: " + ex.Message;
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