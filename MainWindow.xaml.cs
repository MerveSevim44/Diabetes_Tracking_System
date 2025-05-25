using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diabetes_Tracking_System_new
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();

            // Adding mouse events for window dragging
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.MouseMove += MainWindow_MouseMove;
        }

        #region Window Control Methods

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
                this.CaptureMouse();
            }
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
                this.ReleaseMouseCapture();
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector diff = startPoint - currentPosition;

                this.Left -= diff.X;
                this.Top -= diff.Y;
            }
        }

        #endregion

        #region Security Methods

        // Hash the password using SHA256
        private byte[ ] HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Compare two byte arrays (constant time comparison to prevent timing attacks)
        private bool CompareByteArrays(byte[ ] array1, byte[ ] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            bool areEqual = true;
            for (int i = 0; i < array1.Length; i++)
            {
                // Still check all bytes even if we found a mismatch
                // This prevents timing attacks that measure how long the comparison takes
                areEqual &= (array1[i] == array2[i]);
            }

            return areEqual;
        }

        #endregion

        #region Database Operations

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed;

            string tc = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string userType = ((ComboBoxItem)cmbUserType.SelectedItem).Content.ToString().ToLower(); // "doktor" ya da "hasta"

            if (string.IsNullOrEmpty(tc) || string.IsNullOrEmpty(password))
            {
                ShowError("Lütfen tüm alanları doldurun.");
                return;
            }

            byte[ ] hashedPassword = HashPassword(password);

            string connectionString = "Server=localhost;Database=Diabetes_System;Trusted_Connection=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM kullanici WHERE tc_kimlik = @tc AND rol = @rol";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tc", tc);
                        command.Parameters.AddWithValue("@rol", userType); // hasta ya da doktor

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[ ] storedHash = (byte[ ])reader["sifre"];
                                if (CompareByteArrays(hashedPassword, storedHash))
                                {
                                    int kullaniciId = Convert.ToInt32(reader["kullanici_id"]);

                                    if (userType == "doktor")
                                        OpenDoctorDashboard(kullaniciId);
                                    else
                                        OpenPatientDashboard(kullaniciId);
                                }
                                else
                                {
                                    ShowError("Hatalı şifre girdiniz.");
                                }
                            }
                            else
                            {
                                ShowError("Kullanıcı bulunamadı.");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowError($"Veritabanı hatası: {ex.Message}");
                LogError(ex);
            }
            catch (Exception ex)
            {
                ShowError("Beklenmeyen bir hata oluştu.");
                LogError(ex);
            }
        }



        #endregion

        #region UI Helpers

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }

        private void LogError(Exception ex)
        {
            // You could implement proper logging here
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        #endregion

        #region Navigation

        private void OpenDoctorDashboard(int doktorId)
        {
            try
            {
                // Create and show doctor dashboard

                MessageBox.Show($"Doktor girişi başarılı! Doktor ID: {doktorId}");
                DoctorPanel doctorPanel = new DoctorPanel(doktorId);
                doctorPanel.Show();
                this.Close();
               
            }
            catch (Exception ex)
            {
                ShowError("Panel açılırken hata oluştu.");
                LogError(ex);
            }
        }

        private void OpenPatientDashboard(int hastaId)
        {
            try
            {
                // Create and show patient dashboard
              
                MessageBox.Show($"Hasta girişi başarılı! Hasta ID: {hastaId}");
                HastaPanel hastaPanel = new HastaPanel(hastaId);
                hastaPanel.Show();
                this.Close();
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Panel açılırken hata oluştu:\n\n{ex.Message}\n\n{ex.StackTrace}");
                // İstersen loglama da ekleyebilirsin
            }
        }

        #endregion
    }
}
