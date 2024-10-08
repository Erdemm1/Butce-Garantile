using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Butce_Garantile.Models
{
    public class Gelir
    {
        public int Id { get; set; } // Gelir kaydının benzersiz kimliği
        public string Aciklama { get; set; } // Gelirin açıklaması
        public decimal Tutar { get; set; } // Gelirin miktarı
        public DateTime Tarih { get; set; } // Gelirin alındığı tarih
        public string GelirKaynağı { get; set; } // Gelirin kaynağı
        public bool IsDüzenli { get; set; } // Gelirin düzenli bir gelir olup olmadığını belirtmek için
        [BindNever]
        public string UserId { get; set; }

        public static List<string> GelirKaynaklari => new List<string>
        {
            "Maas", "Diger", "Kira", "Yatirim", "Faiz", "Emeklilik"
        };
    }
}
