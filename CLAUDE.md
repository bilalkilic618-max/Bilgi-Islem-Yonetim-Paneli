# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Internal IT department panel (Turkish UI and code identifiers): fault/ticket tracking (Arıza), hardware inventory (Envanter), warehouse stock movements (Depo), staff (Teknisyen/Personel), and month-end reports. ASP.NET Core MVC on .NET 10, EF Core code-first with SQL Server, Sneat-style Bootstrap admin template under `wwwroot/assets`. Live deploy: http://bilgiislempaneli.runasp.net.

The project lives in the `YÖNETİM PANELİ/` subfolder (non-ASCII path — always quote it). Solution file: `YÖNETİM PANELİ.slnx` at repo root.

## Commands

Run from `YÖNETİM PANELİ/`:

```bash
dotnet build
dotnet run                      # uses Properties/launchSettings.json
dotnet ef migrations add <Ad>   # requires dotnet-ef tool; VS users use Add-Migration
dotnet ef database update       # or Update-Database in Package Manager Console
```

There is no test project and no lint/CI configuration.

## Configuration and secrets

Secrets are never put in code or `appsettings.json` (it only has empty placeholders). Development uses User Secrets (`UserSecretsId` in the csproj); production uses environment variables with `__` separators (`Smtp__Sifre`, `Yonetim__AdminSifre`, `ConnectionStrings__DefaultConnection`). `appsettings.Production.json` is gitignored. See README for the full `dotnet user-secrets set` list.

Settings bind to `Ayarlar/UygulamaAyarlari.cs`:
- `SmtpAyarlari` (section `Smtp`) → used by `EmailService` (OTP codes, password reset, admin approval mail, critical-stock alerts).
- `YonetimAyarlari` (section `Yonetim`) → super-admin username/password, the extra warehouse password (`DepoSifresi`), and notification addresses. An empty admin or depo password disables that login entirely.

## Architecture

- **Single controller**: virtually all logic is in `Controllers/HomeController.cs` (~1000 lines), grouped by `// ====` banner comments (auth, arıza, envanter, personel, rapor, depo, backup). It uses `UygulamaDbContext` directly — there is no repository/service layer except `EmailService` and the static `SifreServisi`. Default route is `Home/Login`.
- **Data**: `Models/UygulamaDbContext.cs` defines the DbSets. `Teknisyen` is the actual user/account table (login by Email, SicilNo, or AdSoyad); the `Kullanici` entity exists but is not used for authentication. `ArizaLog` is the audit trail for fault changes; `DepoHareket` holds stock movements with barcode data.
- **Auth model** (`Program.cs` + auth helpers at the top of `HomeController`):
  - Cookie authentication; the controller is `[Authorize]` and public actions opt out with `[AllowAnonymous]`.
  - Super admin credentials come from config (not DB) and get the `BilgiIslemAdmin` claim; admin-only actions use `[Authorize(Policy = "Admin")]`.
  - Regular users must have both `EmailOnayliMi` (6-digit email code) and `YoneticiOnayliMi` (admin approval) set before login succeeds. Role is derived from `Teknisyen.Unvan` (contains "Depo" → "Depo Sorumlusu").
  - Warehouse pages need a second password; success re-issues the cookie with a `DepoOnayBitis` expiry claim, checked via `DepoOnayliMi()`. To change claims, call `OturumAcAsync`/`DepoOnayiniAyarlaAsync` rather than writing cookies manually.
- **Passwords**: `Services/SifreServisi` hashes with `PasswordHasher` (PBKDF2). `Dogrula` still accepts legacy plain-text passwords and signals rehash; login then saves the hash. Keep this migration path intact.
- **Security conventions established by the last security pass**: destructive POSTs use `[ValidateAntiForgeryToken]`, post-login redirects go through `GuvenliYonlendir` (local URLs only), uploaded images are restricted to `IzinliResimUzantilari`, DB backup is streamed as a download (never written under `wwwroot`) and gated by `DepoOnayliMi()`.
- **Views**: `Views/Home/*.cshtml`, shared layout `Views/Shared/_Layout.cshtml`; some pages (login, register, print views) set `Layout = null`. Messages are passed via `ViewBag.HataMesaji` / `TempData`.

## Gotchas

- **Two namespaces**: the csproj `RootNamespace` is `YÖNETİM_PANELİ` (used by `ErrorViewModel` and `_ViewImports.cshtml`), but all real code uses `Yönetim_Paneli.*`. Views therefore reference models with the fully qualified name (`@model Yönetim_Paneli.Models.Ariza`). New classes should use `Yönetim_Paneli.*`.
- File `Models/kullanıcı.cs` has a non-ASCII filename; class name is `Kullanici`.
- Migrations are named in Turkish (`IlkKurulum`, `DepoModuluEklendi`, ...); follow that style.
