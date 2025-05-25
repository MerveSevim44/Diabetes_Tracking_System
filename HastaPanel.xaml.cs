using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Diabetes_Tracking_System_new
{
    /// <summary>
    /// Interaction logic for HastaPanel.xaml
    /// </summary>
    public partial class HastaPanel : Window
    {
        private string _connStr = "Server=localhost;Database=Diabetes_System;Trusted_Connection=True;";
        private int _hastaId;

        public string BugunkunDiyetTuru { get; set; }
        public string BugunkunEgzersizTuru { get; set; }
        public bool BugunkunDiyetDurumu { get; set; }
        public bool BugunkunEgzersizDurumu { get; set; }


        public HastaPanel(int hastaId)
        {
            _hastaId = hastaId;
            InitializeComponent();
            LoadHastaProfile(_hastaId);
            LoadCombinedExerciseDietData(_hastaId);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadGunlukOnerilerSafe(); // artık hataları yakalayacak ve gösterilecek
                InsulinBilgileriniGuncelle();

            }
            catch(Exception ex)
            {
                MessageBox.Show($"Pencere yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }   

           


        }


        private void LoadHastaProfile(int hastaId)
        {
            try
            {
                string query = @"SELECT tc_kimlik,ad, profil_resmi FROM Kullanici WHERE kullanici_id = @HastaId";
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@HastaId", hastaId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtHastaAdi.Text = $"{reader["ad"]}";
                                txtTCNo.Text = $"T.C. No: {reader["tc_kimlik"]}";

                                if (reader["profil_resmi"] != DBNull.Value)
                                {
                                    byte[ ] imageData = reader["profil_resmi"] as byte[ ];
                                    if (imageData != null && imageData.Length > 0)
                                    {
                                        using (var ms = new MemoryStream(imageData))
                                        {
                                            BitmapImage image = new BitmapImage();
                                            image.BeginInit();
                                            image.CacheOption = BitmapCacheOption.OnLoad;
                                            image.StreamSource = ms;
                                            image.EndInit();

                                            // Directly set the ImageSource
                                            HastaProfileImageBrush.ImageSource = image;
                                        }
                                    }
                                    else
                                    {
                                        SetDefaultImage();
                                    }
                                }
                                else
                                {
                                    SetDefaultImage();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hasta profili yüklenirken hata oluştu: {ex.Message}");
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            HastaProfileImageBrush.ImageSource = new BitmapImage(new Uri("/Images/default_doctor.png", UriKind.Relative));
        }




        private void btnCikis_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation dialog for patient panel logout
            MessageBoxResult result = MessageBox.Show(
                "Hasta panelinden çıkmak istediğinizden emin misiniz?",
                "Çıkış Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Option 1: Return to login window (recommended for patient panel)
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();

                // Option 2: Complete application exit (uncomment if needed)
                /*
                Application.Current.Shutdown();
                */
            }
        }


        private List<KanSekeriOlcumu> olcumListesi = new List<KanSekeriOlcumu>();

        public int HastaId { get; }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(txtKanSekeri.Text))
                {
                    MessageBox.Show("Lütfen kan şekeri değerini giriniz.", "Uyarı",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtKanSekeri.Focus();
                    return;
                }

                if (!double.TryParse(txtKanSekeri.Text, out double kanSekeri))
                {
                    MessageBox.Show("Lütfen geçerli bir kan şekeri değeri giriniz.", "Hata",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    txtKanSekeri.Focus();
                    txtKanSekeri.SelectAll();
                    return;
                }

                if (kanSekeri <= 0 || kanSekeri > 1000)
                {
                    MessageBox.Show("Kan şekeri değeri 0-1000 mg/dL arasında olmalıdır.", "Uyarı",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtKanSekeri.Focus();
                    txtKanSekeri.SelectAll();
                    return;
                }

                if (cmbOlcumSaati.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen ölçüm saatini seçiniz.", "Uyarı",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbOlcumSaati.Focus();
                    return;
                }

                if (dpTarih.SelectedDate == null)
                {
                    MessageBox.Show("Lütfen tarih seçiniz.", "Uyarı",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpTarih.Focus();
                    return;
                }

                // Get selected values
                string olcumSaati = ((ComboBoxItem)cmbOlcumSaati.SelectedItem).Content.ToString();
                DateTime tarih = dpTarih.SelectedDate.Value;

                // Check if future date
                if (tarih.Date > DateTime.Now.Date)
                {
                    MessageBox.Show("Gelecek tarih seçilemez.", "Uyarı",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpTarih.Focus();
                    return;
                }

                // Save data to database (replace with your actual database logic)
                SaveBloodSugarData(kanSekeri, olcumSaati, tarih);

                // Show success message
                MessageBox.Show("Kan şekeri ölçümleri başarıyla kaydedildi.", "Başarılı",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form for next entry
                ClearForm();

                // Optional: Refresh data grid or update display
                // RefreshDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata oluştu: {ex.Message}", "Hata",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBloodSugarData(double kanSekeri, string olcumSaati, DateTime tarih)
        {
            // Parse time from selected ComboBox item (extract hour from text like "Sabah (07:00-08:00)")
            TimeSpan saatTimeSpan = ParseTimeFromComboBoxItem(olcumSaati);

            // Create new measurement object and add to list
            KanSekeriOlcumu yeniOlcum = new KanSekeriOlcumu
            {
                HastaId = _hastaId,
                Tarih = tarih,
                Saat = saatTimeSpan,
                Seviye = kanSekeri
            };

            olcumListesi.Add(yeniOlcum);

            // Save all measurements to database
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Get current maximum olcum_id
                string getMaxIdQuery = "SELECT ISNULL(MAX(olcum_id), 0) FROM KanSekeriOlcumu";
                SqlCommand getMaxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                int currentMaxId = (int)getMaxIdCmd.ExecuteScalar();

                foreach (var olcum in olcumListesi)
                {
                    currentMaxId++; // New ID for each measurement
                    string insertQuery = @"
INSERT INTO KanSekeriOlcumu (olcum_id, hasta_id, tarih, saat, seviye_mg_dl)
VALUES (@id, @hastaId, @tarih, @saat, @seviye)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@id", currentMaxId);
                    insertCmd.Parameters.AddWithValue("@hastaId", _hastaId);
                    insertCmd.Parameters.AddWithValue("@tarih", olcum.Tarih.Date);
                    insertCmd.Parameters.AddWithValue("@saat", olcum.Saat);
                    insertCmd.Parameters.AddWithValue("@seviye", olcum.Seviye);
                    insertCmd.ExecuteNonQuery();
                }
            }

            // Clear the list after successful save
            olcumListesi.Clear();
        }

        private TimeSpan ParseTimeFromComboBoxItem(string olcumSaati)
        {
            // Extract time from ComboBox items like "Sabah (07:00-08:00)"
            try
            {
                int startIndex = olcumSaati.IndexOf('(') + 1;
                int endIndex = olcumSaati.IndexOf('-');

                if (startIndex > 0 && endIndex > startIndex)
                {
                    string timeStr = olcumSaati.Substring(startIndex, endIndex - startIndex);
                    if (TimeSpan.TryParse(timeStr, out TimeSpan parsedTime))
                    {
                        return parsedTime;
                    }
                }

                // Fallback times if parsing fails
                switch (olcumSaati.ToLower())
                {
                    case var s when s.Contains("sabah"):
                        return new TimeSpan(7, 30, 0);  // 07:30 - Middle of 07:00-08:00 range
                    case var s when s.Contains("öğle"):
                        return new TimeSpan(12, 30, 0); // 12:30 - Middle of 12:00-13:00 range
                    case var s when s.Contains("ikindi"):
                        return new TimeSpan(15, 30, 0); // 15:30 - Middle of 15:00-16:00 range
                    case var s when s.Contains("akşam"):
                        return new TimeSpan(18, 30, 0); // 18:30 - Middle of 18:00-19:00 range
                    case var s when s.Contains("gece"):
                        return new TimeSpan(22, 30, 0); // 22:30 - Middle of 22:00-23:00 range
                    default:
                        return new TimeSpan(12, 30, 0); // Default to öğle time
                }
            }
            catch
            {
                return new TimeSpan(12, 0, 0); // Default fallback
            }
        }

        private void ClearForm()
        {
            txtKanSekeri.Clear();
            cmbOlcumSaati.SelectedIndex = -1;
            dpTarih.SelectedDate = DateTime.Today;
            txtKanSekeri.Focus();
        }

        // Optional: Add validation on text input for real-time feedback
        private void txtKanSekeri_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Remove non-numeric characters except decimal point
                string text = textBox.Text;
                string numericText = "";
                bool hasDecimal = false;

                foreach (char c in text)
                {
                    if (char.IsDigit(c))
                    {
                        numericText += c;
                    }
                    else if (c == '.' && !hasDecimal)
                    {
                        numericText += c;
                        hasDecimal = true;
                    }
                }

                if (text != numericText)
                {
                    int cursorPosition = textBox.SelectionStart;
                    textBox.Text = numericText;
                    textBox.SelectionStart = Math.Min(cursorPosition, numericText.Length);
                }
            }
        }


        private void LoadCombinedExerciseDietData(int hastaId)
        {
            List<CombinedExerciseDietView> records = new List<CombinedExerciseDietView>();

            DateTime startDate = DateTime.Today.AddDays(-30);
            int egzersizToplam = 0, egzersizYapilan = 0;
            int diyetToplam = 0, diyetUygulanan = 0;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Egzersiz ve Diyet Verileri
                string query = @"
            SELECT 
                COALESCE(e.tarih, d.tarih) AS Tarih,
                e.egzersiz_turu,
                e.yapildi_mi,
                d.diyet_turu,
                d.uygulandi_mi
            FROM EgzersizTakibi e
            FULL OUTER JOIN DiyetTakibi d 
                ON e.hasta_id = d.hasta_id AND e.tarih = d.tarih
            WHERE COALESCE(e.hasta_id, d.hasta_id) = @hastaId
              AND COALESCE(e.tarih, d.tarih) >= @startDate
            ORDER BY Tarih DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@startDate", startDate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime tarih = Convert.ToDateTime(reader["Tarih"]);

                        string egzersizTuru = reader["egzersiz_turu"]?.ToString() ?? "Yok";
                        bool egzersizYapildi = reader["yapildi_mi"] != DBNull.Value && Convert.ToBoolean(reader["yapildi_mi"]);

                        string diyetTuru = reader["diyet_turu"]?.ToString() ?? "Yok";
                        bool diyetUygulandi = reader["uygulandi_mi"] != DBNull.Value && Convert.ToBoolean(reader["uygulandi_mi"]);

                        if (reader["egzersiz_turu"] != DBNull.Value)
                            egzersizToplam++;
                        if (egzersizYapildi)
                            egzersizYapilan++;

                        if (reader["diyet_turu"] != DBNull.Value)
                            diyetToplam++;
                        if (diyetUygulandi)
                            diyetUygulanan++;

                        records.Add(new CombinedExerciseDietView
                        {
                            Date = tarih,
                            ExerciseType = egzersizTuru,
                            ExerciseCompleted = egzersizYapildi,
                            DietType = diyetTuru,
                            DietFollowed = diyetUygulandi
                        });
                    }
                }

                // Ortalama Kan Şekeri (Son 30 Günlük)
                string sugarQuery = @"
            SELECT 
                AVG(CAST(seviye_mg_dl AS FLOAT)) as Ortalama, 
                COUNT(*) as OlcumSayisi
            FROM KanSekeriOlcumu
            WHERE hasta_id = @hastaId AND tarih >= @startDate";

                SqlCommand sugarCmd = new SqlCommand(sugarQuery, conn);
                sugarCmd.Parameters.AddWithValue("@hastaId", hastaId);
                sugarCmd.Parameters.AddWithValue("@startDate", startDate);

                double ortalama = 0;
                int sayi = 0;

                using (SqlDataReader reader = sugarCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ortalama = reader["Ortalama"] != DBNull.Value ? Convert.ToDouble(reader["Ortalama"]) : 0;
                        sayi = reader["OlcumSayisi"] != DBNull.Value ? Convert.ToInt32(reader["OlcumSayisi"]) : 0;
                    }
                }

                // txtBloodeSugar’a yaz
                txtBloodeSugar.Text = sayi > 0
                    ? $"Ort. {Math.Round(ortalama, 1)} mg/dL - {sayi} ölçüm"
                    : "Veri yok";
            }



            // Uyum yüzdeleri
            double egzersizUyum = egzersizToplam == 0 ? 0 : (egzersizYapilan * 100.0 / egzersizToplam);
            double diyetUyum = diyetToplam == 0 ? 0 : (diyetUygulanan * 100.0 / diyetToplam);

            // Yüzdeleri XAML TextBlock'lara yaz
            txtExercise.Text = $"%{Math.Round(egzersizUyum)} Uyum (Son 30 gün)";
            txtDiet.Text = $"%{Math.Round(diyetUyum)} Uyum (Son 30 gün)";
        }


        private void LoadBloodSugarChartByDateRange(int hastaId, DateTime startDate, DateTime endDate)
        {
            var data = new List<KanSekeriOlcumu>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT tarih, AVG(seviye_mg_dl) as Ortalama
FROM KanSekeriOlcumu
WHERE hasta_id = @hastaId AND tarih BETWEEN @startDate AND @endDate
GROUP BY tarih
ORDER BY tarih";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new KanSekeriOlcumu
                        {
                            Tarih = Convert.ToDateTime(reader["tarih"]),
                            Seviye = Convert.ToDouble(reader["Ortalama"])
                        });
                    }
                }
            }

            bloodSugarChart.Series = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Kan Şekeri",
            Values = new ChartValues<double>(data.Select(d => d.Seviye)),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 6
        }
    };

            bloodSugarChart.AxisX.Clear();
            bloodSugarChart.AxisX.Add(new Axis
            {
                Title = "Tarih",
                Labels = data.Select(d => d.Tarih.ToString("dd.MM")).ToArray()
            });

            bloodSugarChart.AxisY.Clear();
            bloodSugarChart.AxisY.Add(new Axis
            {
                Title = "Şeker (mg/dL)",
                MinValue = 0,
                MaxValue = Math.Ceiling(data.Max(d => d.Seviye) / 50) * 50,
                LabelFormatter = value => value.ToString("F0")
            });

            bloodSugarChart.DisableAnimations = true;
            bloodSugarChart.Zoom = ZoomingOptions.None;
        }



        private void btnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dpBaslangic.SelectedDate.HasValue && dpBitis.SelectedDate.HasValue)
                {
                    DateTime baslangic = dpBaslangic.SelectedDate.Value.Date;
                    DateTime bitis = dpBitis.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

                    LoadBloodSugarChartByDateRange(_hastaId, baslangic, bitis);


                    try
                    {
                        LoadBloodSugarTableByDateRange(_hastaId, baslangic, bitis);
                        LoadBloodSugarStatistics(_hastaId, baslangic, bitis);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata: " + ex.Message);
                    }



                }
                else
                {
                    MessageBox.Show("Lütfen tarih aralığını doğru seçiniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void LoadBloodSugarTableByDateRange(int hastaId, DateTime startDate, DateTime endDate)
        {
            var olcumler = new List<KanSekeriOlcumu>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT tarih, saat, seviye_mg_dl
FROM KanSekeriOlcumu
WHERE hasta_id = @hastaId AND tarih BETWEEN @startDate AND @endDate
ORDER BY tarih DESC, saat DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        olcumler.Add(new KanSekeriOlcumu
                        {
                            HastaId = hastaId,
                            Tarih = Convert.ToDateTime(reader["tarih"]),
                            Saat = TimeSpan.Parse(reader["saat"].ToString()),
                            Seviye = Convert.ToDouble(reader["seviye_mg_dl"]),

                        });
                    }
                }
            }

            dgOlcumler.ItemsSource = olcumler;

            // Ortalama ve Sayı Hesapla
            if (olcumler.Count > 0)
            {
                double ortalama = olcumler.Average(x => x.Seviye);
                lblOrtalama.Text = ortalama.ToString("0.0") + " mg/dL";
                lblOlcumSayisi.Text = olcumler.Count.ToString();
            }
            else
            {
                lblOrtalama.Text = "-";
                lblOlcumSayisi.Text = "0";
            }
        }




        private void LoadBloodSugarStatistics(int hastaId, DateTime startDate, DateTime endDate)
        {
            double ortalama = 0;
            int sayi = 0;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT AVG(CAST(seviye_mg_dl AS FLOAT)) as Ortalama, 
       COUNT(*) as OlcumSayisi
FROM KanSekeriOlcumu
WHERE hasta_id = @hastaId AND tarih BETWEEN @startDate AND @endDate";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ortalama = reader["Ortalama"] != DBNull.Value ? Convert.ToDouble(reader["Ortalama"]) : 0;
                        sayi = reader["OlcumSayisi"] != DBNull.Value ? Convert.ToInt32(reader["OlcumSayisi"]) : 0;
                    }
                }
            }

            lblOrtalama.Text = $"Ortalama: {ortalama:F1} mg/dL";
            lblOlcumSayisi.Text = $"Ölçüm Sayısı: {sayi}";
        }

        private void LoadGunlukOnerilerSafe()
        {
            DateTime bugun = DateTime.Today;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Diyet türü
                string diyetQuery = "SELECT TOP 1 diyet_turu FROM DiyetTakibi WHERE hasta_id = @hastaId AND tarih = @tarih";
                SqlCommand diyetCmd = new SqlCommand(diyetQuery, conn);
                diyetCmd.Parameters.AddWithValue("@hastaId", _hastaId);
                diyetCmd.Parameters.AddWithValue("@tarih", bugun);
                object diyetResult = diyetCmd.ExecuteScalar();
                BugunkunDiyetTuru = diyetResult?.ToString() ?? "Tanımsız";

                // Egzersiz türü
                string egzersizQuery = "SELECT TOP 1 egzersiz_turu FROM EgzersizTakibi WHERE hasta_id = @hastaId AND tarih = @tarih";
                SqlCommand egzersizCmd = new SqlCommand(egzersizQuery, conn);
                egzersizCmd.Parameters.AddWithValue("@hastaId", _hastaId);
                egzersizCmd.Parameters.AddWithValue("@tarih", bugun);
                object egzersizResult = egzersizCmd.ExecuteScalar();
                BugunkunEgzersizTuru = egzersizResult?.ToString() ?? "Tanımsız";
            }

            // Bu satırı en sona al: UI yüklendikten sonra binding yap
            this.DataContext = this;

        }


        private void btnGunlukKaydet_Click(object sender, RoutedEventArgs e)
        {
            DateTime bugun = DateTime.Today;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Diyet güncelleme
                string updateDiyet = @"
            UPDATE DiyetTakibi 
            SET uygulandi_mi = @uygulandiMi
            WHERE hasta_id = @hastaId AND tarih = @tarih";
                SqlCommand cmdDiyet = new SqlCommand(updateDiyet, conn);
                cmdDiyet.Parameters.AddWithValue("@hastaId", _hastaId);
                cmdDiyet.Parameters.AddWithValue("@tarih", bugun);
                cmdDiyet.Parameters.AddWithValue("@uygulandiMi", BugunkunDiyetDurumu);
                int diyetAffected = cmdDiyet.ExecuteNonQuery();

                // Egzersiz güncelleme
                string updateEgzersiz = @"
            UPDATE EgzersizTakibi 
            SET yapildi_mi = @yapildiMi
            WHERE hasta_id = @hastaId AND tarih = @tarih";
                SqlCommand cmdEgzersiz = new SqlCommand(updateEgzersiz, conn);
                cmdEgzersiz.Parameters.AddWithValue("@hastaId", _hastaId);
                cmdEgzersiz.Parameters.AddWithValue("@tarih", bugun);
                cmdEgzersiz.Parameters.AddWithValue("@yapildiMi", BugunkunEgzersizDurumu);
                int egzersizAffected = cmdEgzersiz.ExecuteNonQuery();

                // Her iki tabloda da kayıt yoksa hata ver
                if (diyetAffected == 0 || egzersizAffected == 0)
                {
                    MessageBox.Show("Bugün için doktorunuz tarafından belirlenmiş kayıt bulunamadı. Lütfen doktorunuza başvurun.",
                                    "Kayıt Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            MessageBox.Show("Günlük uygulama bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void btnInsulinFiltrele_Click(object sender, RoutedEventArgs e)
        {
            DateTime? baslangic = dpInsulinBaslangic.SelectedDate;
            DateTime? bitis = dpInsulinBitis.SelectedDate;

            if (baslangic == null || bitis == null)
            {
                MessageBox.Show("Lütfen başlangıç ve bitiş tarihlerini seçiniz.");
                return;
            }

            List<InsulinOnerisi> liste = new List<InsulinOnerisi>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
            SELECT ortalama_seker, doz_ml, tarih 
            FROM InsulinOnerisi 
            WHERE hasta_id = @id AND tarih BETWEEN @baslangic AND @bitis
            ORDER BY tarih DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", _hastaId);
                cmd.Parameters.AddWithValue("@baslangic", baslangic.Value.Date);
                cmd.Parameters.AddWithValue("@bitis", bitis.Value.Date.AddDays(1).AddTicks(-1)); // günü tam kapatır

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        liste.Add(new InsulinOnerisi
                        {
                            KanSekeriOrtalama = reader["ortalama_seker"] != DBNull.Value ? Convert.ToInt32(Convert.ToDouble(reader["ortalama_seker"])) : 0,
                            OnerilenDoz = reader["doz_ml"] != DBNull.Value ? Convert.ToInt32(reader["doz_ml"]) : 0,
                            Tarih = reader["tarih"] != DBNull.Value ? Convert.ToDateTime(reader["tarih"]) : DateTime.MinValue
                        });
                    }
                }
            }

            dgInsulin.ItemsSource = liste;
        }


        private void InsulinBilgileriniGuncelle()
        {
            double ortalama = 0;
            int doz = 0;
            DateTime sonGuncelleme = DateTime.MinValue;

            string ortalamaQuery = @"
        SELECT AVG(seviye_mg_dl)
        FROM KanSekeriOlcumu
        WHERE hasta_id = @hastaId AND tarih >= CAST(GETDATE() AS DATE)";

            string dozQuery = @"
        SELECT TOP 1 doz_ml, tarih
        FROM InsulinOnerisi
        WHERE hasta_id = @hastaId
        ORDER BY tarih DESC";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Ortalama kan şekeri
                using (SqlCommand cmd = new SqlCommand(ortalamaQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaId", _hastaId);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                        ortalama = Math.Round(Convert.ToDouble(result), 1);
                }

                // Son önerilen doz
                using (SqlCommand cmd = new SqlCommand(dozQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaId", _hastaId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            doz = reader["doz_ml"] != DBNull.Value ? Convert.ToInt32(reader["doz_ml"]) : 0;
                            sonGuncelleme = reader["tarih"] != DBNull.Value ? Convert.ToDateTime(reader["tarih"]) : DateTime.MinValue;
                        }
                    }
                }
            }

            // Arayüze yaz
            txtGunlukOrtalama.Text = ortalama > 0 ? $"{ortalama} mg/dL" : "Veri yok";
            txtDoz.Text = doz > 0 ? $"{doz} ml" : "-";
            txtTarih.Text = sonGuncelleme != DateTime.MinValue ? sonGuncelleme.ToString("dd.MM.yyyy HH:mm") : "-";
        }




        private void chkDiyet_Checked(object sender, RoutedEventArgs e)
        {
            BugunkunDiyetDurumu = true;
        }

        private void chkEgzersiz_Checked(object sender, RoutedEventArgs e)
        {
            BugunkunEgzersizDurumu = true;
        }

    }
}
