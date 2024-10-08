namespace Butce_Garantile.Models
{
    public enum Month
    {
        Ocak = 1,
        Şubat,
        Mart,
        Nisan,
        Mayıs,
        Haziran,
        Temmuz,
        Ağustos,
        Eylül,
        Ekim,
        Kasım,
        Aralık
    }

    public class MonthlyReport
    {
        public Month Month { get; set; } // Ay olarak enum kullanıldı
        public int Year { get; set; } // Ay olarak enum kullanıldı
        public decimal Income { get; set; } // Gelir
        public decimal Expense { get; set; } // Gider
    }
}
