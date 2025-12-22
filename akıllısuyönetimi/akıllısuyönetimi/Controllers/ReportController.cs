using Microsoft.AspNetCore.Mvc;

namespace akıllısuyönetimi.Controllers
{
    public class ReportController : Controller // 👈 Adı doğru mu?
    {
        public IActionResult Consumption() // 👈 Action adı doğru mu?
        {
            ViewData["Title"] = "Tüketim Raporları";
            return View();
        }
    }
}