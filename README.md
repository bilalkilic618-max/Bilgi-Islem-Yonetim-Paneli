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

* **Backend:** C#, .NET 8 Core MVC
* **Veritabanı:** MS SQL Server, Entity Framework Core (Code-First Approach)
* **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript, Chart.js
* **Güvenlik:** Role-Based Authorization, Cookie Authentication
* **Bildirim Sistemi:** Otomatik SMTP E-Posta Entegrasyonu (OTP & Stok Uyarıları)
<img width="1877" height="971" alt="image" src="https://github.com/user-attachments/assets/826b9938-3b92-4a2b-b522-e41d98f06951" />

## 📌 Temel Modüller ve Özellikler
* 🔐 **Gelişmiş Rol Yönetimi:** Admin, Teknisyen ve Standart Kullanıcı olmak üzere 3 farklı yetki hiyerarşisi.
* 🛠️ **Dinamik Arıza Takibi:** Birimlerden gelen taleplerin anlık olarak açılması, durum güncellemeleri ve işlem geçmişinin (Audit Log) tutulması.
* 📦 **Akıllı Envanter ve Depo Kontrolü:** Donanım stoklarının güvenli takibi. Kritik stok seviyelerine ulaşıldığında sistemin otomatik olarak ilgili yöneticilere **SMTP üzerinden e-posta uyarısı** göndermesi.
* 📊 **Gerçek Zamanlı Dashboard:** Chart.js entegrasyonu ile arıza durumlarının ve birim istatistiklerinin grafiksel raporlanması.
<img width="1915" height="878" alt="image" src="https://github.com/user-attachments/assets/636dfa1b-9388-41be-965f-03a04e573f49" />

## 📂 Kurulum (Geliştiriciler İçin)
Projeyi lokalinizde ayağa kaldırmak için `appsettings.json` içerisindeki veritabanı (Connection String) ve SMTP ayarlarını kendi ortamınıza göre düzenleyip, Package Manager Console üzerinden `Update-Database` komutu ile migrasyonları uygulamanız yeterlidir.
