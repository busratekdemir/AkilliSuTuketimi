using akıllısuyönetimi.Data;
using akıllısuyönetimi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    private const string AdminDomain = "@izsu.gov.tr";
    private const string WeatherCity = "Izmir";
    private const string SoaUrl = "http://localhost:5001/predict";
    private const string WeatherApiKey = "8bca5e710184b1301468e1f273bc010a";

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        await SetUserGreetingAsync();

        using var client = new HttpClient();

        await FillWeatherAsync(client);
        await FillSoaAsync(client);

        var viewModel = await BuildDashboardAsync();

        TryOverrideTotalUsageFromDb(viewModel);

        FillChartFromDb(viewModel);

        return View(viewModel);
    }

    private async Task SetUserGreetingAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int currentUserId))
        {
            ViewBag.UserFullName = "Kullanıcı";
            return;
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);
        ViewBag.UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Kullanıcı";
    }

    private async Task FillWeatherAsync(HttpClient client)
    {
        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={WeatherCity}&appid={WeatherApiKey}&units=metric&lang=tr";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            ViewBag.Temp = Math.Round((double)result.main.temp);
            ViewBag.WeatherDesc = result.weather[0].description;
            ViewBag.WeatherIcon = result.weather[0].icon;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hava durumu hatası");
        }
    }

    private async Task FillSoaAsync(HttpClient client)
    {
        try
        {
            var soaResponse = await client.GetAsync(SoaUrl);
            if (!soaResponse.IsSuccessStatusCode)
            {
                SetSoaDefaults();
                ViewBag.MLPredictionError = "Analiz servisine şu an ulaşılamıyor.";
                return;
            }

            var content = await soaResponse.Content.ReadAsStringAsync();

            JObject analysisData;
            try
            {
                analysisData = JObject.Parse(content);
            }
            catch
            {
                SetSoaDefaults();
                ViewBag.MLPredictionError = "Analiz servisinden geçersiz veri döndü.";
                return;
            }

            ViewBag.TotalMonthlyUsage = analysisData["TotalMLUsage"]?.ToString() ?? "48.73";
            ViewBag.ForecastNextDay = analysisData["ForecastNextDay"]?.ToString() ?? "0";
            ViewBag.ActiveAlertCount = analysisData["ActiveAlertCount"]?.ToString() ?? "0";

            ViewBag.MLDaily = analysisData["DailyPredictions"]?.ToObject<List<double>>() ?? new List<double>();
            ViewBag.MLPredictions = analysisData["WeeklyPredictions"]?.ToObject<List<double>>() ?? new List<double>();
            ViewBag.MLMonthly = analysisData["MonthlyPredictions"]?.ToObject<List<double>>() ?? new List<double>();

            ViewBag.MLAnomalies = analysisData["DetailedAnomalies"]?.ToObject<dynamic>() ?? new List<dynamic>();
            ViewBag.WaterResources = analysisData["Resources"]?.ToObject<dynamic>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SOA Bağlantı Hatası");
            SetSoaDefaults();
            ViewBag.MLPredictionError = "Analiz servisine şu an ulaşılamıyor.";
        }
    }

    private void SetSoaDefaults()
    {
        ViewBag.MLAnomalies = new List<dynamic>();
        ViewBag.MLDaily = new List<double>();
        ViewBag.MLPredictions = new List<double>();
        ViewBag.MLMonthly = new List<double>();
        ViewBag.ActiveAlertCount = "0";
    }

    private async Task<DashboardViewModel> BuildDashboardAsync()
    {
        var viewModel = new DashboardViewModel
        {
            WaterSources = await _context.WaterSources.AsNoTracking().ToListAsync(),
            ConsumptionRecords = await _context.Consumption.AsNoTracking().ToListAsync(),
            RecentAlerts = await _context.Alerts.AsNoTracking()
                .OrderByDescending(a => a.AlertTime)
                .Take(5)
                .ToListAsync()
        };

        viewModel.ActiveAlertCount = viewModel.RecentAlerts?.Count ?? 0;
        return viewModel;
    }

    private void TryOverrideTotalUsageFromDb(DashboardViewModel viewModel)
    {
        if (viewModel?.ConsumptionRecords == null || viewModel.ConsumptionRecords.Count == 0) return;

        try
        {
            // UsageValue türün decimal/double/int olabilir; burası tipine göre uyumlu çalışır.
            double dbTotal = viewModel.ConsumptionRecords.Sum(c => Convert.ToDouble(c.UsageValue));
            if (dbTotal > 0)
                ViewBag.TotalMonthlyUsage = dbTotal.ToString("N2");
        }
        catch
        {
            // tip dönüşüm hatası olursa ML verisini bozmayalım
        }
    }

    private void FillChartFromDb(DashboardViewModel viewModel)
    {
        if (viewModel?.ConsumptionRecords == null)
        {
            ViewBag.ChartLabels = new List<string>();
            ViewBag.ChartData = new List<double>();
            return;
        }

        var dailyUsageData = viewModel.ConsumptionRecords
            .GroupBy(c => c.ReadingTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Date = g.Key.ToString("dd.MM"),
                TotalUsage = g.Sum(x => Convert.ToDouble(x.UsageValue))
            })
            .ToList();

        ViewBag.ChartLabels = dailyUsageData.Select(d => d.Date).ToList();
        ViewBag.ChartData = dailyUsageData.Select(d => d.TotalUsage).ToList();
    }

    // -------------------------------------------------------------------
    // GİRİŞ İŞLEMLERİ (LOGIN)
    // -------------------------------------------------------------------

    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            // Düz Metin Şifre Kontrolü
            if (user != null && user.PasswordHash == model.Password)
            {
                // Yönetici Rolü Ataması 
                if (user.Email.EndsWith(AdminDomain, StringComparison.OrdinalIgnoreCase))
                {
                    user.Role = "Admin";
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                else if (user.Role == "Admin" && !user.Email.EndsWith(AdminDomain, StringComparison.OrdinalIgnoreCase))
                {
                    // Admin domaininde olmayan bir e-posta ile Admin rolü varsa Client'a düşür 
                    user.Role = "Client";
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                // Kimlik (Claims) oluşturma
                // ... Mevcut kodlarınız (Giriş kontrolü vb.) ...

                // Kimlik (Claims) oluşturma kısmını bu şekilde güncelleyin:
                var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role),

    // 👇 EKLEDİĞİMİZ SATIRLAR: Veritabanındaki FirstName ve LastName'i sisteme tanıtır
    new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
    new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
    new Claim(ClaimTypes.Surname, user.LastName ?? ""),


};



                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                _logger.LogInformation($"Kullanıcı {user.Email} ({user.Role}) başarıyla giriş yaptı.");

                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. E-posta veya şifre hatalı.");
        }

        return View(model);
    }

    // GET: /Home/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Kullanıcı çıkış yaptı.");
        return RedirectToAction(nameof(Login));
    }

    // -------------------------------------------------------------------
    // KAYIT İŞLEMLERİ (REGİSTER)
    // -------------------------------------------------------------------

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // YÖNETİCİ KAYIT ENGELİ
            if (model.Email.EndsWith(AdminDomain, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Email", "Bu e-posta alan adına sahip hesaplar manuel olarak oluşturulur ve kayıt olamaz.");
                return View(model);
            }

            // 1. E-posta zaten kayıtlı mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlıdır.");
                return View(model);
            }

            // 2. Yeni Kullanıcı (Client) nesnesini oluştur
            var user = new User
            {
                Email = model.Email,
                PasswordHash = model.Password, // DÜZ METİN ŞİFRE KAYDI

                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = "Client",
                CreatedAt = DateTime.Now // ZORUNLU ALAN GARANTİSİ
                // Diğer zorunlu alanlar buraya eklenmeli (örneğin IsActive = true)
            };

            // HATA YAKALAMA (Try-Catch)
            try
            {
                // 3. Veritabanına kaydet
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 4. Kayıt başarılı, kullanıcıyı Login sayfasına yönlendir
                TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                // Hata oluşursa, hatayı form üzerinde göster.
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                _logger.LogError(ex, "Kayıt işlemi sırasında veritabanı hatası oluştu.");
                ModelState.AddModelError("", "Kayıt sırasında hata oluştu. Lütfen tüm alanları kontrol edin. Hata Detayı: " + errorMessage);
                return View(model);
            }
        }

        return View(model);
    }

    // -------------------------------------------------------------------
    // ŞİFRE SIFIRLAMA İŞLEMLERİ (FORGOT PASSWORD)
    // -------------------------------------------------------------------

    // GET: /Home/ForgotPassword
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    // POST: /Home/ForgotPassword (E-posta kontrolü)
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                // E-posta bulunamazsa, kullanıcıya bildirim yap
                ModelState.AddModelError("Email", "Bu e-posta adresi sistemde kayıtlı değil.");
                return View(model);
            }

            // E-posta bulunduysa, şifre sıfırlama sayfasına yönlendir.
            TempData["ResetEmail"] = user.Email;
            return RedirectToAction(nameof(ResetPasswordConfirm));
        }

        return View(model);
    }

    // GET: /Home/ResetPasswordConfirm
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirm()
    {
        // E-posta adresi taşınmadıysa, geri yönlendir
        if (TempData["ResetEmail"] == null)
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        // View'e taşınan e-posta bilgisini ResetPasswordViewModel ile gönderiyoruz.
        var model = new ResetPasswordViewModel { Email = TempData["ResetEmail"].ToString() };
        TempData["ResetEmail"] = model.Email; // Bir sonraki POST için tekrar TempData'ya kaydet

        return View(model);
    }

    // POST: /Home/ResetPasswordConfirm (Şifreyi Güncelleme)
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPasswordConfirm(ResetPasswordViewModel model)
    {
        // E-posta TempData'dan veya modelden gelmelidir
        if (TempData["ResetEmail"] != null)
        {
            model.Email = TempData["ResetEmail"].ToString();
            TempData["ResetEmail"] = model.Email; // POST sonrası yeniden yükleme için
        }

        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                // Kullanıcı bulunamazsa, Login sayfasına yönlendir.
                TempData["SuccessMessage"] = "İşlem başarılı. Giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            // DÜZ METİN ŞİFRE GÜNCELLEMESİ
            user.PasswordHash = model.Password;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi! Lütfen yeni şifrenizle giriş yapın.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Şifre güncelleme sırasında bir hata oluştu: " + ex.InnerException?.Message);
            }
        }
        return View(model);
    }


    // GET: /Home/AccessDenied
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Helper metot
    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index), "Home");
    }

}