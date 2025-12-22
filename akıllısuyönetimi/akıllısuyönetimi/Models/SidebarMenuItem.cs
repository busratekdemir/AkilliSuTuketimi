// Models/SidebarMenuItem.cs
namespace akıllısuyönetimi.Models // <-- Projenizin Ana Namespace'i (Önemli!)
{
    public class SidebarMenuItem
    {
        public string Name { get; set; }
        public string IconClass { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; }
    }
}