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
    public class GelirController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public GelirController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string aciklama, DateTime? tarihBaslangic, DateTime? tarihBitis)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Oturum açan kullanıcının ID'sini al
            var gelirler = _context.Gelirler.AsQueryable().Where(g => g.UserId == userId); // Kullanıcıya ait gelirleri filtrele

            // Mevcut tarih
            var currentDate = DateTime.Now;

            // Gelirleri kontrol et ve yeni kayıtları oluştur
            foreach (var gelir in gelirler.ToList()) // ToList() ile döngü başında verileri yükle
            {
                // Eğer gelir düzenli ise
                if (gelir.IsDüzenli)
                {
                    // Yeni tarih oluştur
                    var yeniTarih = gelir.Tarih.AddMonths(1);

                    // Şu anki tarih ile yeni tarihi karşılaştır
                    while (yeniTarih.Date <= currentDate.Date)
                    {
                        // Aynı ay için zaten bir gelir kaydı var mı kontrol et
                        var ayKontrolu = gelirler.Any(g =>
                            g.Tarih.Year == yeniTarih.Year &&
                            g.Tarih.Month == yeniTarih.Month &&
                            g.UserId == userId); // Kullanıcı ID'sini kontrol et

                        // Eğer aynı ay için bir gelir kaydı yoksa, yeni bir kayıt oluştur
                        if (!ayKontrolu)
                        {
                            var newGelir = new Gelir
                            {
                                Aciklama = gelir.Aciklama,
                                Tutar = gelir.Tutar,
                                Tarih = yeniTarih, // Yeni tarihi kullan
                                GelirKaynağı = gelir.GelirKaynağı,
                                IsDüzenli = false, // Otomatik oluşturulan kayıtlar için IsDüzenli false
                                UserId = userId // Kullanıcı ID'sini ekle
                            };

                            // Yeni gelir kaydını ekle
                            _context.Gelirler.Add(newGelir);
                        }

                        // Yeni tarihi bir ay ileri al
                        yeniTarih = yeniTarih.AddMonths(1);
                    }
                }
            }

            await _context.SaveChangesAsync(); // Değişiklikleri kaydet

            // Açıklama filtresi
            if (!string.IsNullOrEmpty(aciklama))
            {
                gelirler = gelirler.Where(g => g.Aciklama.Contains(aciklama));
            }

            // Tarih aralığı filtresi
            if (tarihBaslangic.HasValue)
            {
                gelirler = gelirler.Where(g => g.Tarih >= tarihBaslangic.Value);
            }

            if (tarihBitis.HasValue)
            {
                gelirler = gelirler.Where(g => g.Tarih <= tarihBitis.Value);
            }

            // ViewData kullanarak arama değerlerini geçirme
            ViewData["Aciklama"] = aciklama;
            ViewData["TarihBaslangic"] = tarihBaslangic.HasValue ? tarihBaslangic.Value.ToString("yyyy-MM-dd") : string.Empty;
            ViewData["TarihBitis"] = tarihBitis.HasValue ? tarihBitis.Value.ToString("yyyy-MM-dd") : string.Empty;

            var model = await gelirler.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Aciklama,Tutar,Tarih,GelirKaynağı,IsDüzenli")] Gelir gelir)
        {
            // Kullanıcı oturum açmış mı kontrol et
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User); // Oturum açan kullanıcının ID'sini al
            gelir.UserId = userId; // Gelir kaydına kullanıcı ID'sini ekle

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
                return View(gelir);
            }

            _context.Gelirler.Add(gelir); // Geliri ekle
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet
            return RedirectToAction(nameof(Index)); // Ana sayfaya yönlendir
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gelir = await _context.Gelirler.FindAsync(id);
            if (gelir == null || gelir.UserId != _userManager.GetUserId(User)) // Kullanıcı kontrolü
            {
                return NotFound();
            }
            return View(gelir);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Aciklama,Tutar,Tarih,GelirKaynağı,IsDüzenli")] Gelir gelir)
        {
            if (id != gelir.Id)
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
                return View(gelir);
            }

            // Mevcut geliri yükle
            var existingGelir = await _context.Gelirler.FindAsync(id);
            if (existingGelir == null || existingGelir.UserId != userId) // Kullanıcı kontrolü
            {
                return NotFound();
            }

            // Geçerli gelir nesnesinin özelliklerini mevcut nesneden kopyala
            existingGelir.Aciklama = gelir.Aciklama;
            existingGelir.Tutar = gelir.Tutar;
            existingGelir.Tarih = gelir.Tarih;
            existingGelir.GelirKaynağı = gelir.GelirKaynağı;
            existingGelir.IsDüzenli = gelir.IsDüzenli;

            try
            {
                // Güncellemeyi kaydet
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GelirExists(gelir.Id))
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gelir = await _context.Gelirler.FindAsync(id);
            if (gelir == null || gelir.UserId != _userManager.GetUserId(User)) // Kullanıcı kontrolü
            {
                return NotFound();
            }
            return View(gelir);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gelir = await _context.Gelirler.FindAsync(id);
            if (gelir != null && gelir.UserId == _userManager.GetUserId(User)) // Kullanıcı kontrolü
            {
                _context.Gelirler.Remove(gelir);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GelirExists(int id)
        {
            return _context.Gelirler.Any(e => e.Id == id);
        }
    }
}
