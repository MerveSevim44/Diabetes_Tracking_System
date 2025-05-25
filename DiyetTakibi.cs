using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
    public class DiyetTakibi
    {
        public int DiyetId { get; set; }
        public int HastaId { get; set; }
        public DateTime Tarih { get; set; }
        public string DiyetTuru { get; set; }  // Az Şekerli, Şekersiz, Dengeli
        public bool UygulandiMi { get; set; }

    }
}
