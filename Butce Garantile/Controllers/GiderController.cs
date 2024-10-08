using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Butce_Garantile.Data;
using Butce_Garantile.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Butce_Garantile.Controllers
{
    [Authorize]
    public class GiderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GiderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string aciklama, DateTime? tarihBaslangic, DateTime? tarihBitis)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var giderler = _context.Giderler.AsQueryable().Where(g => g.UserId == userId);

            // Mevcut tarih
            var currentDate = DateTime.Now;

            // Kullanıcının düzenli giderlerini kontrol et ve gerekirse yeni kayıt oluştur
            foreach (var gider in giderler.ToList())
            {
                // Eğer gider düzenli ise
                if (gider.IsDüzenli)
                {
                    // Yeni tarih oluştur
                    var yeniTarih = gider.Tarih.AddMonths(1);

                    // Şu anki tarih ile yeni tarihi karşılaştır
                    while (yeniTarih.Date <= currentDate.Date)
                    {
                        // Aynı ay için zaten bir gider kaydı var mı kontrol et
                        var ayKontrolu = giderler.Any(g =>
                            g.Tarih.Year == yeniTarih.Year &&
                            g.Tarih.Month == yeniTarih.Month &&
                            g.UserId == userId && // Kullanıcı ID'sini kontrol et
                            g.Aciklama == gider.Aciklama); // Açıklamayı kontrol et

                        // Eğer aynı ay için bir gider kaydı yoksa, yeni bir kayıt oluştur
                        if (!ayKontrolu)
                        {
                            var yeniGider = new Gider
                            {
                                Aciklama = gider.Aciklama,
                                Tutar = gider.Tutar,
                                Tarih = yeniTarih, // Yeni tarihi kullan
                                GiderTuru = gider.GiderTuru,
                                IsDüzenli = false, // Otomatik oluşturulan kayıtlar için IsDüzenli false
                                UserId = userId // Kullanıcıyı ata
                            };

                            _context.Giderler.Add(yeniGider); // Yeni gider kaydını ekle
                        }

                        // Yeni tarihi bir ay ileri al
                        yeniTarih = yeniTarih.AddMonths(1);
                    }
                }
            }

            await _context.SaveChangesAsync(); // Yeni giderleri kaydet

            // Açıklama filtresi
            if (!string.IsNullOrEmpty(aciklama))
            {
                giderler = giderler.Where(g => g.Aciklama.Contains(aciklama));
            }

            // Tarih aralığı filtresi
            if (tarihBaslangic.HasValue)
            {
                giderler = giderler.Where(g => g.Tarih >= tarihBaslangic.Value);
            }

            if (tarihBitis.HasValue)
            {
                giderler = giderler.Where(g => g.Tarih <= tarihBitis.Value);
            }

            // ViewData kullanarak arama değerlerini geçirme
            ViewData["Aciklama"] = aciklama;
            ViewData["TarihBaslangic"] = tarihBaslangic.HasValue ? tarihBaslangic.Value.ToString("yyyy-MM-dd") : string.Empty;
            ViewData["TarihBitis"] = tarihBitis.HasValue ? tarihBitis.Value.ToString("yyyy-MM-dd") : string.Empty;

            var model = await giderler.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendirin
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Giriş sayfası adı ve controller'ını güncelleyin
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Aciklama,Tutar,Tarih,GiderTuru,IsDüzenli")] Gider gider)
        {
            // Kullanıcı oturum açmış mı kontrol et
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının UserId'sini al
            var userId = _userManager.GetUserId(User);
            gider.UserId = userId;

            // UserId geçerli değilse ModelState'ten kaldır
            ModelState.Remove("UserId");

            // Model geçerliliğini kontrol et
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View(gider);
            }

            _context.Add(gider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gider = await _context.Giderler.FindAsync(id);
            if (gider == null || gider.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return NotFound();
            }
            return View(gider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Aciklama,Tutar,Tarih,GiderTuru,IsDüzenli")] Gider gider)
        {
            if (id != gider.Id)
            {
                return NotFound();
            }

            // Kullanıcının UserId'sini al
            var userId = _userManager.GetUserId(User);
            ModelState.Remove("UserId"); // UserId'yi modelden kaldır

            // Model geçerliliğini kontrol et
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View(gider);
            }

            // Mevcut gideri yükle
            var existingGider = await _context.Giderler.FindAsync(id);
            if (existingGider == null || existingGider.UserId != userId)
            {
                return NotFound();
            }

            // Geçerli gider nesnesinin özelliklerini mevcut nesneden kopyala
            existingGider.Aciklama = gider.Aciklama;
            existingGider.Tutar = gider.Tutar;
            existingGider.Tarih = gider.Tarih;
            existingGider.GiderTuru = gider.GiderTuru;
            existingGider.IsDüzenli = gider.IsDüzenli;

            try
            {
                // Güncellemeyi kaydet
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GiderExists(gider.Id))
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

        private bool GiderExists(int id)
        {
            return _context.Giderler.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gider = await _context.Giderler.FindAsync(id);
            if (gider == null || gider.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return NotFound();
            }
            return View(gider);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gider = await _context.Giderler.FindAsync(id);
            if (gider != null && gider.UserId == User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                _context.Giderler.Remove(gider);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
