using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// Model: Haftalık tahmin verisi
public class WeeklyForecastItem
{
    public string DayShort { get; set; } // Örn: Paz, Sal
    public string DayLong { get; set; } // Örn: Pazartesi
    public string Expected { get; set; } // Örn: 44.500 m³
    public string Range { get; set; } // Örn: 42.000 - 47.000 m³
    public double Progress { get; set; } // İlerleme çubuğu yüzdesi (Örn: 0.70 = %70)
}

public class WeeklyForecastViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var forecast = new List<WeeklyForecastItem>
        {
            new WeeklyForecastItem { DayShort = "Paz", DayLong = "Pazartesi", Expected = "44.500 m³", Range = "42.000 - 47.000 m³", Progress = 0.65 },
            new WeeklyForecastItem { DayShort = "Sal", DayLong = "Salı", Expected = "45.200 m³", Range = "42.500 - 48.000 m³", Progress = 0.75 },
            new WeeklyForecastItem { DayShort = "Çar", DayLong = "Çarşamba", Expected = "46.800 m³", Range = "44.000 - 49.500 m³", Progress = 0.82 },
            new WeeklyForecastItem { DayShort = "Per", DayLong = "Perşembe", Expected = "47.500 m³", Range = "45.000 - 50.000 m³", Progress = 0.90 },
            new WeeklyForecastItem { DayShort = "Cum", DayLong = "Cuma", Expected = "46.200 m³", Range = "43.500 - 49.000 m³", Progress = 0.78 },
            new WeeklyForecastItem { DayShort = "Cum", DayLong = "Cumartesi", Expected = "43.800 m³", Range = "41.000 - 46.500 m³", Progress = 0.60 },
            new WeeklyForecastItem { DayShort = "Paz", DayLong = "Pazar", Expected = "41.200 m³", Range = "38.500 - 44.000 m³", Progress = 0.50 }
        };

        return View(forecast);
    }
}