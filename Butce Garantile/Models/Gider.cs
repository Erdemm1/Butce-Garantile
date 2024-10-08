using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Butce_Garantile.Models
{
    public class Gider
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
        public decimal Tutar { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        public string GiderTuru { get; set; }
        public bool IsDüzenli { get; set; }
        [BindNever]
        public string UserId { get; set; }
        public static List<string> GiderTurleri => new List<string>
        {
            "Konut",
            "Faturalar",
            "Beslenme",
            "Ulasim",
            "Saglik",
            "Egitim",
            "Kredi/Borc",
            "Giysi",
            "Eglence",
            "Acil Durum",
            "Diger"
        };
    }
}
