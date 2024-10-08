using Butce_Garantile.Data;
using Butce_Garantile.Models;
using Microsoft.AspNetCore.Authorization; // Import this for authorization
using Microsoft.AspNetCore.Identity; // Import this for User management
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Import this for Claims
using System.Threading.Tasks;

namespace Butce_Garantile.Controllers
{
    [Authorize] // Ensure this controller is secured
    public class RaporController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gelirler = await _context.Gelirler.Where(g => g.UserId == userId).ToListAsync();
            var giderler = await _context.Giderler.Where(g => g.UserId == userId).ToListAsync();

            var reports = new List<MonthlyReport>();

            foreach (var year in gelirler.Select(g => g.Tarih.Year).Union(giderler.Select(g => g.Tarih.Year)).Distinct())
            {
                foreach (var month in Enum.GetValues(typeof(Month)).Cast<Month>())
                {
                    var monthlyIncome = gelirler
                        .Where(g => g.Tarih.Month == (int)month && g.Tarih.Year == year)
                        .Sum(g => g.Tutar);
                    var monthlyExpense = giderler
                        .Where(g => g.Tarih.Month == (int)month && g.Tarih.Year == year)
                        .Sum(g => g.Tutar);

                    reports.Add(new MonthlyReport
                    {
                        Month = month,
                        Year = year,
                        Income = monthlyIncome,
                        Expense = monthlyExpense
                    });
                }
            }

            // Kullanıcının hedefini al
            var userTarget = await _context.UserTargets.FirstOrDefaultAsync(ut => ut.UserId == userId);
            var totalSpent = reports.Sum(r => r.Expense); // Toplam gideri hesapla
            var targetProgress = userTarget != null ? $"{totalSpent}/{userTarget.Target}" : "Hedef Belirlenmedi"; // Hedef durumu

            ViewBag.UserTarget = targetProgress; // Hedef durumu için ViewBag'e ekleyin
            return View(reports);
        }

    }
}
