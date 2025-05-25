using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
    public class EgzersizTakibi
    {
        public int EgzersizId { get; set; }
        public int HastaId { get; set; }
        public DateTime Tarih { get; set; }
        public string EgzersizTuru { get; set; } // Yürüyüş, Klinik Egzersiz ...
        public bool UygulandiMi { get; set; }
    }
}
