using Microsoft.AspNetCore.Mvc;

namespace akıllısuyönetimi.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index() // Menüdeki Action adı
        {
            ViewData["Title"] = "Ayarlar";
            return View();
        }
    }
}