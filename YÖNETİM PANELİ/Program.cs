using Microsoft.EntityFrameworkCore;
using Yönetim_Paneli.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles(); // MapStaticAssets yerine bunu kullan, daha garantidir.

app.UseRouting();
app.UseAuthorization();

// VARSAYILAN SAYFA LOGIN OLARAK GÜNCELLENDİ
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();