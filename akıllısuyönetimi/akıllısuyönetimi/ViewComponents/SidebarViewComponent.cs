using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using akıllısuyönetimi.Models; // Models klasörünüzden SidebarMenuItem'ı çeker

public class SidebarViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // ====================================================================
        // GİRİŞ KONTROLÜ VE ROL ATAMASI
        // ====================================================================

        // Giriş yapılmadıysa boş liste döndür
        if (!User.Identity.IsAuthenticated)
        {
            return View(new List<SidebarMenuItem>());
        }

        var isAdmin = User.IsInRole("Admin");

        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

        // -------------------------------------------------------------------
        // TEMEL KULLANICI MENÜSÜ (Tüm rollerde görünecek olan temel liste)
        // -------------------------------------------------------------------

        var menuItems = new List<SidebarMenuItem>
        {
            new SidebarMenuItem { Name = "Ana Panel", IconClass = "bi-grid-fill", Controller = "Home", Action = "Index" },
            new SidebarMenuItem { Name = "Tüketim Raporları", IconClass = "bi-file-earmark-bar-graph", Controller = "Report", Action = "Consumption" },
            // Not: Sayaç Yönetimi Client için de görünmeli, en sona eklenmiştir.
        };

        // -------------------------------------------------------------------
        // YÖNETİCİYE EK MENÜLERİN KULLANICI LİSTESİNE ENTEGRE EDİLMESİ
        // (Sıralama: Tahminler, Uyarılar, Sayaç Yönetimi)
        // -------------------------------------------------------------------

        if (isAdmin)
        {
            // Gelecek Tahminleri (Tüketim Raporları'ndan sonra, 2. index'e)
            menuItems.Insert(2, new SidebarMenuItem { Name = "Gelecek Tahminleri", IconClass = "bi-graph-up-arrow", Controller = "Forecast", Action = "Index" });

            // Uyarılar (Tahminlerden sonra, 3. index'e)
            menuItems.Insert(3, new SidebarMenuItem { Name = "Uyarılar", IconClass = "bi-bell", Controller = "Alerts", Action = "Index" });

            // Sayaç Yönetimi (Uyarılar'dan sonra, 4. index'e)
            menuItems.Insert(4, new SidebarMenuItem { Name = "Sayaç Yönetimi", IconClass = "bi-clock", Controller = "Meters", Action = "Index" });
        }
       


        // Ayarlar menüsü, her zaman en sona eklenir
        menuItems.Add(new SidebarMenuItem { Name = "Ayarlar", IconClass = "bi-gear", Controller = "Settings", Action = "Index" });

        // ====================================================================
        // AKTİFLİK MANTIĞI
        // ====================================================================

        foreach (var item in menuItems)
        {
            // Eğer Controller ve Action eşleşiyorsa, IsActive'i true yap
            if (item.Controller == currentController && item.Action == currentAction)
            {
                item.IsActive = true;
                break;
            }
        }

        return View(menuItems);
    }
}