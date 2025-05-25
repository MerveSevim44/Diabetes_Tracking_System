using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
    public class HastaListeModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public double GlucoseLevel { get; set; }
        public List<string> Symptoms { get; set; } = new List<string>(); // semptom listesi
    }

}
