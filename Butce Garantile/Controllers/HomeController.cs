using Butce_Garantile.Data;
using Butce_Garantile.Models; // ApplicationUser'ı burada kullanabilmek için ekledik
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Butce_Garantile.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // UserManager türünü ApplicationUser ile değiştir

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Initialize UserManager
        }

        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user's ID
            var currentUserId = _userManager.GetUserId(User);

            // Gelir ve Gider toplamlarını hesapla (filtered by user ID)
            var toplamGelir = await _context.Gelirler
                .Where(g => g.UserId == currentUserId) // Filter by user ID
                .SumAsync(g => g.Tutar);

            var toplamGider = await _context.Giderler
                .Where(g => g.UserId == currentUserId) // Filter by user ID
                .SumAsync(g => g.Tutar);

            // Gelir kategorilerini topla (filtered by user ID)
            var gelirKategorileri = await _context.Gelirler
                .Where(g => g.UserId == currentUserId) // Filter by user ID
                .GroupBy(g => g.GelirKaynağı)
                .Select(g => new { Kategori = g.Key, Tutar = g.Sum(x => x.Tutar) })
                .ToListAsync();

            // Gider kategorilerini topla (filtered by user ID)
            var giderKategorileri = await _context.Giderler
                .Where(g => g.UserId == currentUserId) // Filter by user ID
                .GroupBy(g => g.GiderTuru)
                .Select(g => new { Kategori = g.Key, Tutar = g.Sum(x => x.Tutar) })
                .ToListAsync();

            // Kullanıcının hedefini al
            var userTarget = await _context.UserTargets
                .FirstOrDefaultAsync(ut => ut.UserId == currentUserId);

            // Toplam değişim hesapla
            ViewBag.Degisim = toplamGelir - toplamGider;
            ViewBag.ToplamGelir = toplamGelir;
            ViewBag.ToplamGider = toplamGider;
            ViewBag.ToplamTutar = toplamGelir - toplamGider; // Toplam Tutarı hesapla
            ViewBag.GelirKategorileri = gelirKategorileri;
            ViewBag.GiderKategorileri = giderKategorileri;

            // Hedef bilgisi için ViewBag'e ekleme
            ViewBag.UserTarget = userTarget != null ? userTarget.Target : 0; // Hedef yoksa 0 olarak ayarlıyoruz

            // Kalan miktarı hesapla
            var kalan = (userTarget != null ? userTarget.Target : 0) - (toplamGelir - toplamGider);
            ViewBag.Kalan = kalan; // Kalan miktarı ViewBag'e ekle

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SetTarget(int target)
        {
            var userId = _userManager.GetUserId(User);

            // Kullanıcının hedefini güncelle veya oluştur
            var userTarget = await _context.UserTargets.FirstOrDefaultAsync(ut => ut.UserId == userId);
            if (userTarget == null)
            {
                userTarget = new UserTarget { UserId = userId, Target = target };
                _context.UserTargets.Add(userTarget);
            }
            else
            {
                userTarget.Target = target;
                _context.UserTargets.Update(userTarget);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index"); // Hedef kaydedildikten sonra ana sayfaya dön
        }

    }
}