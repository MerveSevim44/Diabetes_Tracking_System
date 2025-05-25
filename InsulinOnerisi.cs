using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
    public class InsulinOnerisi
    {
        public int OneriId { get; set; }
        public int HastaId { get; set; }
        public DateTime Tarih { get; set; }
        public int KanSekeriOrtalama { get; set; }
        public float OnerilenDoz { get; set; } // "1 ml", "2 ml" vb.

    }
}
