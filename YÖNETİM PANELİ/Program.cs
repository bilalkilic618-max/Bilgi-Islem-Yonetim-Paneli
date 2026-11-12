using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Yönetim_Paneli.Ayarlar;
using Yönetim_Paneli.Models;
using Yönetim_Paneli.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ayarlar: gizli değerler User Secrets (geliştirme) veya ortam değişkeninden (sunucu) gelir.
builder.Services.Configure<SmtpAyarlari>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<YonetimAyarlari>(builder.Configuration.GetSection("Yonetim"));
builder.Services.AddScoped<EmailService>();

// Kimlik doğrulama: ASP.NET Core'un şifreli ve imzalı oturum çerezi.
// Eskiden kullanıcı adı ve rol düz çerezde tutuluyordu; tarayıcıdan değiştirilip admin olunabiliyordu.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/CikisYap";
        options.AccessDeniedPath = "/Home/YetkisizErisim";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireClaim(YetkiSabitleri.AdminClaim, "true"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // MapStaticAssets yerine bunu kullan, daha garantidir.

app.UseRouting();
app.UseAuthentication(); // UseAuthorization'dan ÖNCE gelmeli: önce "kimsin", sonra "yetkin var mı"
app.UseAuthorization();

// VARSAYILAN SAYFA LOGIN OLARAK GÜNCELLENDİ
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
