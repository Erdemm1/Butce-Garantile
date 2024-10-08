namespace Butce_Garantile.Models
{
    public class UserTarget
    {
        public int Id { get; set; } // Birincil anahtar
        public string UserId { get; set; } // Kullanıcının ID'si
        public int Target { get; set; } // Kullanıcının belirlediği hedef
    }
}
