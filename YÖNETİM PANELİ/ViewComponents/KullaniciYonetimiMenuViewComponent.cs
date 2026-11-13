using Microsoft.AspNetCore.Mvc;
using Yönetim_Paneli.Ayarlar;
using Yönetim_Paneli.Models;

namespace Yönetim_Paneli.ViewComponents
{
    // Kenar menüsündeki "Kullanıcı Yönetimi" öğesi. Sadece süper admin görür;
    // rozet, onay bekleyen (e-postası doğrulanmış, yönetici onayı almamış) kullanıcı sayısını gösterir.
    // Menü birden fazla sayfada kopyalandığı için öğe tek yerde tutulur.
    public class KullaniciYonetimiMenuViewComponent : ViewComponent
    {
        private readonly UygulamaDbContext _context;

        public KullaniciYonetimiMenuViewComponent(UygulamaDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            if (!UserClaimsPrincipal.HasClaim(YetkiSabitleri.AdminClaim, "true"))
                return Content(string.Empty);

            int bekleyenSayisi = _context.Teknisyenler.Count(x => x.EmailOnayliMi && !x.YoneticiOnayliMi);
            return View(bekleyenSayisi);
        }
    }
}
