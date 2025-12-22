// Controllers/DataController.cs
using akıllısuyönetimi.Data;
using akıllısuyönetimi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace akıllısuyönetimi.Controllers
{
    // JSON veri alıp döndüren API Controller
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ************************************************************
        // 1. TÜKETİM VERİSİ ALMA (Consumption)
        // ************************************************************
        // POST: api/Data/Consumption
        // Body:
        // {
        //   "meterSerialNumber": "SYC-1001",
        //   "usageValue": 5.7,
        //   "readingTime": "2025-12-13T10:00:00Z"
        // }

        [HttpPost("Consumption")]
        public async Task<IActionResult> PostConsumptionData(
            [FromBody] ConsumptionRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Sayaç seri numarasına göre sayaç bulunur
            var meter = await _context.Meters
                .FirstOrDefaultAsync(m => m.SerialNumber == request.MeterSerialNumber);

            if (meter == null)
            {
                return NotFound($"Sayaç bulunamadı: {request.MeterSerialNumber}");
            }

            // Yeni tüketim kaydı oluşturulur
            var consumption = new Consumption
            {
                MeterId = meter.Id,
                UsageValue = request.UsageValue,
                ReadingTime = request.ReadingTime ?? DateTime.UtcNow
            };

            // Veritabanına kaydet
            await _context.Consumption.AddAsync(consumption);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Tüketim verisi başarıyla alındı.",
                MeterId = meter.Id
            });
        }

        // ************************************************************
        // 2. SU KAYNAĞI / BARAJ VERİSİ ALMA (WaterSource)
        // ************************************************************
        // POST: api/Data/WaterSource
        // Body:
        // {
        //   "sourceName": "Talaş Barajı",
        //   "usableVolume": 150000000,
        //   "activeFillRate": 85.5
        // }

        [HttpPost("WaterSource")]
        public async Task<IActionResult> PostWaterSourceData(
            [FromBody] WaterSourceRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Su kaynağını adına göre bul
            var waterSource = await _context.WaterSources
                .FirstOrDefaultAsync(w => w.SourceName == request.SourceName);

            if (waterSource == null)
            {
                return NotFound($"Su kaynağı bulunamadı: {request.SourceName}");
            }

            // Verileri güncelle
            waterSource.UsableVolume = request.UsableVolume;
            waterSource.ActiveFillRate = request.ActiveFillRate;
            waterSource.LastUpdateDate = DateTime.UtcNow;

            // Veritabanına kaydet
            _context.WaterSources.Update(waterSource);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Su kaynağı verisi başarıyla güncellendi.",
                SourceName = waterSource.SourceName
            });
        }
    }
}
