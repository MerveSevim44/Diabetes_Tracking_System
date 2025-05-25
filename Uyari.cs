using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Diabetes_Tracking_System_new
{
    public class Uyari
    {
        public int UyariId { get; set; }
        public int HastaId { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public string UyariTipi { get; set; }
        public string Mesaj { get; set; }
        public string Time => Saat.ToString(@"hh\:mm");
        public string Message => Mesaj;
        public Brush WarningColor =>
            UyariTipi == "Acil Uyarı" || UyariTipi == "Acil Müdahale Uyarısı" ? Brushes.Red :
            UyariTipi == "Takip Uyarısı" ? Brushes.Orange :
            UyariTipi == "İzleme Uyarısı" ? Brushes.Blue :
            UyariTipi == "Ölçüm Eksik Uyarısı" ? Brushes.Red :
            UyariTipi == "Ölçüm Yetersiz Uyarısı" ? Brushes.Orange :
            Brushes.Red;

    }
}
