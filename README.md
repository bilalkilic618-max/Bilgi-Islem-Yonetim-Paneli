# 🏢 Kurumsal IT Yönetim ve Arıza Takip Otomasyonu (IT Management System)

Kurumsal organizasyonların bilgi işlem süreçlerini (arıza takibi, donanım envanter yönetimi ve stok kontrolü) dijitalleştirmek, merkezi bir panel üzerinden raporlamak ve otomatize etmek amacıyla geliştirilmiş uçtan uca bir yönetim sistemidir.

## 🚀 Canlı Demo ve Test Ortamı
Uygulamanın çalışan sürümünü (Deploy) aşağıdaki bağlantıdan test edebilirsiniz:

- 🌐 **Canlı Uygulama:** [bilgiislempaneli.runasp.net](http://bilgiislempaneli.runasp.net)
- 👤 **Test Kullanıcısı:** `teknik`
- 🔑 **Test Şifresi:** `2006*`

## ⚙️ Mimari ve Teknolojiler (Tech Stack)
Proje, sürdürülebilirlik ve modern standartlar gözetilerek geliştirilmiştir.

* **Backend:** C#, .NET 8 Core MVC
* **Veritabanı:** MS SQL Server, Entity Framework Core (Code-First Approach)
* **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript, Chart.js
* **Güvenlik:** Role-Based Authorization, Cookie Authentication
* **Bildirim Sistemi:** Otomatik SMTP E-Posta Entegrasyonu (OTP & Stok Uyarıları)

## 📌 Temel Modüller ve Özellikler
* 🔐 **Gelişmiş Rol Yönetimi:** Admin, Teknisyen ve Standart Kullanıcı olmak üzere 3 farklı yetki hiyerarşisi.
* 🛠️ **Dinamik Arıza Takibi:** Birimlerden gelen taleplerin anlık olarak açılması, durum güncellemeleri ve işlem geçmişinin (Audit Log) tutulması.
* 📦 **Akıllı Envanter ve Depo Kontrolü:** Donanım stoklarının güvenli takibi. Kritik stok seviyelerine ulaşıldığında sistemin otomatik olarak ilgili yöneticilere **SMTP üzerinden e-posta uyarısı** göndermesi.
* 📊 **Gerçek Zamanlı Dashboard:** Chart.js entegrasyonu ile arıza durumlarının ve birim istatistiklerinin grafiksel raporlanması.

## 📂 Kurulum (Geliştiriciler İçin)
Projeyi lokalinizde ayağa kaldırmak için `appsettings.json` içerisindeki veritabanı (Connection String) ve SMTP ayarlarını kendi ortamınıza göre düzenleyip, Package Manager Console üzerinden `Update-Database` komutu ile migrasyonları uygulamanız yeterlidir.
