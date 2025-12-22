using Microsoft.AspNetCore.Mvc;
using System;

namespace akıllısuyönetimi.Controllers
{
    public class ForecastController : Controller
    {
        public IActionResult Index() // Menüdeki Action adı
        {

            //node.js post http
            ViewData["Title"] = "Gelecek Tahminleri";
            return View();
        }
    }
}