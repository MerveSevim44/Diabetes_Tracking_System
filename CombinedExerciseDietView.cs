using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diabetes_Tracking_System_new
{
   

        public class CombinedExerciseDietView
        {
            public DateTime Date { get; set; }
            public string ExerciseType { get; set; }
            public bool ExerciseCompleted { get; set; }
            public string DietType { get; set; }
            public bool DietFollowed { get; set; }
        }

   
}
