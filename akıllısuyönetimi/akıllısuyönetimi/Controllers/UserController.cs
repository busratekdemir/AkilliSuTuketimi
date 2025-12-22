// Controllers/UserController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using akıllısuyönetimi.Models;
using akıllısuyönetimi.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using akıllısuyönetimi.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // .Any() ve diğer LINQ sorguları için

namespace akıllısuyönetimi.Controllers
{
    // Controller seviyesinde yetkilendirme: Sadece Admin rolüne sahip kullanıcılar erişebilir.
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // -------------------------------------------------------------------
        // READ: Kullanıcı Listesi (Index Sayfası)
        // -------------------------------------------------------------------
        // GET: /User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // -------------------------------------------------------------------
        // CREATE: Yeni Kullanıcı Ekleme
        // -------------------------------------------------------------------

        // GET: /User/Create (Formu Göster)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /User/Create (Form Verisini İşle)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            // Şifre alanı zorunlu ve şifre modelde olmadığı için ModelState.IsValid'i manuel kontrol ediyoruz.
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError("password", "Şifre zorunludur ve en az 6 karakter olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlıdır.");
                    return View(user);
                }

                // Şifre Hashleme
                user.PasswordHash = PasswordHelper.HashPassword(password);
                user.IsTemporaryPassword = false;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // -------------------------------------------------------------------
        // UPDATE: Kullanıcı Düzenleme
        // -------------------------------------------------------------------

        // GET: /User/Edit/{id} (Düzenleme Formunu Göster)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Şifre hash'ini güvenlik nedeniyle View'a göndermiyoruz.
            return View(user);
        }

        // POST: /User/Edit (Form Verisini İşle)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // ModelState.IsValid kontrolü sadece kullanıcı adı, soyadı, email ve rol için yapılır.
            // Şifre güncellemesi ayrı bir metotla yapılmalıdır, bu yüzden şimdilik şifreyi hariç tutuyoruz.
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Mevcut kullanıcıyı veritabanından çek (PasswordHash'i korumak için)
                    var existingUser = await _context.Users.FindAsync(id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // 2. Güncellenebilir alanları üzerine yaz
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Role = user.Role; // Rol güncellenebilir

                    // 3. Veritabanına kaydet
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.Id == user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // -------------------------------------------------------------------
        // DELETE: Kullanıcı Silme
        // -------------------------------------------------------------------

        // GET: /User/Delete/{id} (Silme Onay Sayfasını Göster)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /User/Delete/{id} (Silme İşlemini Onayla ve Gerçekleştir)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}