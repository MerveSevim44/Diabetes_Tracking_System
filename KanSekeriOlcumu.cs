using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Diabetes_Tracking_System_new
{
    public class KanSekeriOlcumu
    {

        public int HastaId { get; set; }    
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public double Seviye { get; set; }
      

        public string StatusText =>
            Seviye < 70 ? "Düşük" :
            Seviye <= 110 ? "Normal" :
            Seviye <= 150 ? "Orta" :
            Seviye <= 200 ? "Yüksek" : "Çok Yüksek";

        public bool IsLow => Seviye < 70;
        public bool IsNormal => Seviye >= 70 && Seviye <= 110;
        public bool IsMediumHigh => Seviye >= 111 && Seviye <= 150;
        public bool IsHigh => Seviye >= 151 && Seviye <= 200;
        public bool IsVeryHigh => Seviye > 200;
    }

}
