using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Diabetes_Tracking_System_new
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Uygulama başladığında hangi pencere açılsın?
            new MainWindow().Show();  // veya DoctorPanel gibi başka bir pencere
        }

    }
}
