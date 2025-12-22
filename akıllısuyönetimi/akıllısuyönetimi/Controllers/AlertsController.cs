using akıllısuyönetimi.Data;
using akıllısuyönetimi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace akıllısuyönetimi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(ApplicationDbContext context, ILogger<AlertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Alerts
        public async Task<IActionResult> Index()
        {
            var dbAlerts = await _context.Alerts.Include(a => a.Meter).OrderByDescending(a => a.AlertTime).ToListAsync();
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };

            using (var client = new HttpClient(handler))
            {
                try
                {
                    var response = await client.GetAsync("http://localhost:5001/predict");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(content);

                        ViewBag.ActiveAlertCount = data["ActiveAlertCount"]?.ToString() ?? "0";
                        ViewBag.PossibleLeakCount = data["PossibleLeakCount"]?.ToString() ?? "0";
                        ViewBag.AbnormalUsageCount = data["AbnormalUsageCount"]?.ToString() ?? "0";
                        ViewBag.SolvedCount = data["SolvedCount"]?.ToString() ?? "147";
                        ViewBag.AlertList = data["DetailedAnomalies"]?.ToObject<dynamic>() ?? new List<dynamic>();
                        ViewBag.MLDaily = data["DailyPredictions"]?.ToObject<List<double>>() ?? new List<double>();
                        // 👇 Gerçek tarih etiketlerini buraya ekledik!
                        ViewBag.MLDailyLabels = data["DailyLabels"]?.ToObject<List<string>>() ?? new List<string>();
                    }
                    else
                    {
                        // Servis 500 hatası verirse (NaN hatası gibi) varsayılanları ata
                        SetDefaultViewBagValues();
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"SOA Alerts Bağlantı Hatası: {ex.Message}");
                    SetDefaultViewBagValues();
                }
            }

            return View(dbAlerts);
        }

        // Hata durumunda sayfanın "0" ile dolmasını sağlayan yardımcı metod
        private void SetDefaultViewBagValues()
        {
            ViewBag.ActiveAlertCount = "0";
            ViewBag.PossibleLeakCount = "0";
            ViewBag.AbnormalUsageCount = "0";
            ViewBag.SolvedCount = "0";
            ViewBag.AlertList = new List<dynamic>();
            ViewBag.MLDaily = new List<double>();
        }

        public async Task<IActionResult> Details(int id)
        {
            var alert = await _context.Alerts
                                .Include(a => a.Meter)
                                .FirstOrDefaultAsync(m => m.Id == id);

            if (alert == null) return NotFound();
            return View(alert);
        }
    }
}