# 🏢 Kurumsal IT Yönetim ve Arıza Takip Otomasyonu (IT Management System)
"Bu otomasyon, Bilgi İşlem teknik ekibinin günlük iş yükünü hafifletmek ve envanter süreçlerini hızlandırmak amacıyla, doğrudan ekibin ihtiyaçları doğrultusunda bir takım içi araç (internal tool) olarak geliştirilmiştir."
<img width="1870" height="974" alt="image" src="https://github.com/user-attachments/assets/b96f6a21-c49f-4f0b-9c2c-8ac6a56ee697" />


Kurumsal organizasyonların bilgi işlem süreçlerini (arıza takibi, donanım envanter yönetimi ve stok kontrolü) dijitalleştirmek, merkezi bir panel üzerinden raporlamak ve otomatize etmek amacıyla geliştirilmiş uçtan uca bir yönetim sistemidir.

## 🚀 Canlı Demo ve Test Ortamı
Uygulamanın çalışan sürümünü (Deploy) aşağıdaki bağlantıdan test edebilirsiniz:

- 🌐 **Canlı Uygulama

[http://bilgiislempaneli.runasp.net](http://bilgiislempaneli.runasp.net)


## ⚙️ Mimari ve Teknolojiler (Tech Stack)
Proje, sürdürülebilirlik ve modern standartlar gözetilerek geliştirilmiştir.

* **Backend:** C#, ASP.NET Core MVC (.NET 10)
* **Veritabanı:** MS SQL Server, Entity Framework Core (Code-First Approach)
* **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript, Chart.js
* **Güvenlik:** ASP.NET Core Cookie Authentication, `[Authorize]` ve policy tabanlı yetkilendirme, PBKDF2 ile şifre hash'leme (`PasswordHasher`), AntiForgery token
* **Bildirim Sistemi:** Otomatik SMTP E-Posta Entegrasyonu (OTP & Stok Uyarıları)
<img width="1877" height="971" alt="image" src="https://github.com/user-attachments/assets/826b9938-3b92-4a2b-b522-e41d98f06951" />

## 📌 Temel Modüller ve Özellikler
* 🔐 **Gelişmiş Rol Yönetimi:** Admin, Teknisyen ve Standart Kullanıcı olmak üzere 3 farklı yetki hiyerarşisi.
* 🛠️ **Dinamik Arıza Takibi:** Birimlerden gelen taleplerin anlık olarak açılması, durum güncellemeleri ve işlem geçmişinin (Audit Log) tutulması.
* 📦 **Akıllı Envanter ve Depo Kontrolü:** Donanım stoklarının güvenli takibi. Kritik stok seviyelerine ulaşıldığında sistemin otomatik olarak ilgili yöneticilere **SMTP üzerinden e-posta uyarısı** göndermesi.
* 📊 **Gerçek Zamanlı Dashboard:** Chart.js entegrasyonu ile arıza durumlarının ve birim istatistiklerinin grafiksel raporlanması.
<img width="1915" height="878" alt="image" src="https://github.com/user-attachments/assets/636dfa1b-9388-41be-965f-03a04e573f49" />

## 📂 Kurulum (Geliştiriciler İçin)
Gizli bilgiler (veritabanı şifresi, SMTP şifresi, admin ve depo şifresi) **koda veya `appsettings.json`'a yazılmaz.**

**1. Geliştirme ortamı (User Secrets):** Proje klasöründe şu komutları çalıştırın:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=true;"
dotnet user-secrets set "Smtp:KullaniciAdi" "ornek@gmail.com"
dotnet user-secrets set "Smtp:GonderenAdres" "ornek@gmail.com"
dotnet user-secrets set "Smtp:Sifre" "GMAIL_UYGULAMA_SIFRESI"
dotnet user-secrets set "Yonetim:AdminKullaniciAdi" "admin"
dotnet user-secrets set "Yonetim:AdminSifre" "GUCLU_BIR_SIFRE"
dotnet user-secrets set "Yonetim:DepoSifresi" "DEPO_SIFRESI"
dotnet user-secrets set "Yonetim:AdminAdSoyad" "Ad Soyad"          # isteğe bağlı, boşsa "Sistem Yöneticisi"
dotnet user-secrets set "Yonetim:AdminUnvan" "Bilgi İşlem Sorumlusu" # isteğe bağlı
dotnet user-secrets set "Yonetim:YoneticiEmail" "yonetici@ornek.com"
dotnet user-secrets set "Yonetim:DepoSorumlusuEmail" "depo@ornek.com"
```

**2. Sunucu (canlı ortam):** Aynı değerleri ortam değişkeni olarak girin. İki nokta yerine çift alt çizgi kullanılır: `Smtp__Sifre`, `Yonetim__AdminSifre`, `ConnectionStrings__DefaultConnection` vb.

**3. Veritabanı:** Package Manager Console üzerinden `Update-Database` komutu ile migrasyonları uygulayın.

> Mevcut kullanıcıların düz metin şifreleri, ilk başarılı girişte otomatik olarak hash'e çevrilir.
