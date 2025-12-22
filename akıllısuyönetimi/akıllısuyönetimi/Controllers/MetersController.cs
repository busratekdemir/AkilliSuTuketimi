using akıllısuyönetimi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace akıllısuyönetimi.Controllers
{
    public class MetersController : Controller
    {
        // Örnek verileri sildik; böylece Any() kontrolü Python verisini alabilecek
        private static List<Meter> _meters = new List<Meter>();

        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("http://localhost:5001/predict");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(content);

                        // Kart verileri her zaman Python'dan güncellenir
                        ViewBag.TotalMonthly = data["TotalMonthlyUsage"]?.ToString();
                        ViewBag.DailyAvg = data["DailyAverage"]?.ToString();
                        ViewBag.PerPerson = data["PerPersonUsage"]?.ToString();
                        ViewBag.Savings = data["SavingsPotential"]?.ToString();
                        ViewBag.ChangeRate = data["UsageChangeRate"]?.ToString();

                        // Eğer liste boşsa (ilk açılışta), Python'dan gelen ML listesini doldur
                        if (!_meters.Any())
                        {
                            var mlMeters = data["MeterDataList"]?.ToObject<List<Meter>>();
                            if (mlMeters != null)
                            {
                                _meters.AddRange(mlMeters);
                            }
                        }
                    }
                }
                catch { SetDefaultStats(); }
            }
            // İşlem yapılabilen (Ekle/Sil/Düzenle) listeyi döndür
            return View(_meters);
        }

       

        private void SetDefaultStats()
        {
            ViewBag.TotalMonthly = "0";
            ViewBag.DailyAvg = "0";
            ViewBag.PerPerson = "142";
            ViewBag.Savings = "0";
            ViewBag.ChangeRate = "%0";
        }

        [HttpPost]
        public IActionResult Create(Meter model)
        {
            if (model != null)
            {
                // Yeni eklenen sayaca benzersiz bir Id atıyoruz
                model.Id = _meters.Any() ? _meters.Max(m => m.Id) + 1 : 1;
                _meters.Add(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Meter model)
        {
            var existingMeter = _meters.FirstOrDefault(m => m.Id == model.Id);
            if (existingMeter != null)
            {
                existingMeter.Location = model.Location;
                existingMeter.Status = model.Status;
                existingMeter.Type = model.Type;
                existingMeter.SerialNumber = model.SerialNumber;
                existingMeter.MeterCode = model.MeterCode;
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var meter = _meters.FirstOrDefault(m => m.Id == id);
            if (meter != null)
            {
                _meters.Remove(meter);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Sayaç bulunamadı." });
        }

        public IActionResult Create() => View();
        public IActionResult Edit(int id)
        {
            var meter = _meters.FirstOrDefault(m => m.Id == id);
            return meter == null ? NotFound() : View(meter);
        }
    }
}