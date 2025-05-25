using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
    public class Kullanici
    {
        public int KullaniciId { get; set; }
        public string TcKimlik { get; set; }
        public byte[ ] Sifre { get; set; }       // VARBINARY SHA-256
        public string Rol { get; set; }        // "doktor" veya "hasta"
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Email { get; set; }
        public string Cinsiyet { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string ResimYolu { get; set; }  // Profil fotoğrafı yolu

        // Örnek computed property
        public int Yas => DateTime.Today.Year - DogumTarihi.Year;
    }
}
