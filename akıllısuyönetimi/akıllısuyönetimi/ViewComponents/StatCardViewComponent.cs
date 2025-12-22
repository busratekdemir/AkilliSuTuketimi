using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; // Task için gereklidir
// akıllısuyönetimi.Models namespace'inde StatCardViewModel'in olduğunu varsayıyoruz
using akıllısuyönetimi.Models;

namespace akıllısuyönetimi.ViewComponents
{
    public class StatCardViewComponent : ViewComponent
    {
        // Constructor, şu an DbContext veya servis bağımlılığı olmadığı için boş kalır.
        public StatCardViewComponent() { }

        // View Component'ler için önerilen eş zamansız (asynchronous) metot.
        // Bu metot, Index.cshtml'deki Component.InvokeAsync çağrısına cevap verir.
        public async Task<IViewComponentResult> InvokeAsync(string title, string value, string iconClass, string changeText, string changeColorClass)
        {
            // 1. Parametrelerin gelip gelmediğini kontrol edin.
            if (string.IsNullOrEmpty(title))
            {
                // Parametre gelmezse, tarayıcıda görünecek uyarı metnini döndürün.
                return Content("Hata: StatCard verisi sağlanmadı (Başlık Eksik).");
            }

            // 2. Gelen parametrelerden Model nesnesini oluşturun.
            var model = new StatCardViewModel
            {
                Title = title,
                Value = value,
                IconClass = iconClass,
                ChangeText = changeText,
                ChangeColorClass = changeColorClass
            };

            // 3. Modeli Views/Shared/Components/StatCard/Default.cshtml'e gönderin.
            // await kullanmıyoruz, çünkü burada async bir işlem yapılmıyor.
            return View(model);
        }
    }
}