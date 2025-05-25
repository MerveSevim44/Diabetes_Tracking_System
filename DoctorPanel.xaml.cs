using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Data;
using System.Net;
using System.IO;
using LiveCharts;
using LiveCharts.Wpf;

namespace Diabetes_Tracking_System_new
{
    /// <summary>
    /// Interaction logic for DoctorPanel.xaml
    /// </summary>
    public partial class DoctorPanel : Window
    {
        private int _doktorId;
        private string _connStr = "Server=localhost;Database=Diabetes_System;Trusted_Connection=True;";


        private readonly string _emailSender = "ankmrv044@gmail.com";

        private Point scrollMousePoint = new Point();
        private bool isDragging = false;
        private ScrollViewer scrollViewer;

        public DoctorPanel(int doktorId)
        {
            try
            {
              
                InitializeComponent();
                _doktorId = doktorId;
                MessageBox.Show($"DoctorPanel loaded with ID: {_doktorId}");
                LoadDoctorProfile(_doktorId);
                // Add debug output


                HastalariYukle();
                LoadPatientIds();
            }
            catch (Exception ex)
            {
                // Log the exception
                MessageBox.Show($"Error initializing DoctorPanel: {ex.Message}\n\nStack Trace: {ex.StackTrace}");
                MessageBox.Show($"Hata: {ex.Message}\n\nStack: {ex.StackTrace}\n\nInner: {ex.InnerException}");
                MessageBox.Show("XAML hatası: " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }


        // Method to load doctor profile picture
        private void LoadDoctorProfile(int doctorId)
        {
            try
            {
              
                string query = @"SELECT kullanici_id, profil_resmi FROM Kullanici WHERE kullanici_id = @DoctorId";

                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Set Doctor ID
                                DoctorIdTextBlock.Text = $"ID: {reader["kullanici_id"]}";

                                // Load Doctor Profile Picture
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
                                            DoctorProfileImage.Source = image;
                                        }
                                    }
                                    else
                                    {
                                        // Set default doctor image
                                        DoctorProfileImage.Source = new BitmapImage(new Uri("/Images/default_doctor.png", UriKind.Relative));
                                    }
                                }
                                else
                                {
                                    // Set default doctor image
                                    DoctorProfileImage.Source = new BitmapImage(new Uri("/Images/default_doctor.png", UriKind.Relative));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Doktor profili yüklenirken hata oluştu: {ex.Message}");
                // Set default image on error
                DoctorProfileImage.Source = new BitmapImage(new Uri("/Images/default_doctor.png", UriKind.Relative));
                DoctorIdTextBlock.Text = "ID: --";
            }
        }

       

        // Call this method when the form loads or when doctor logs in
     

      
        private ScrollViewer GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
                return o as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // Handle mouse wheel for both vertical and horizontal scrolling (with Shift key)
        private void DataGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scrollViewer == null)
                scrollViewer = GetScrollViewer(dgPatients);

            if (scrollViewer != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Horizontal scrolling when Shift is pressed
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                }
                else
                {
                    // Normal vertical scrolling
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                }
                e.Handled = true;
            }
        }

        // Enable click-and-drag scrolling (mouse panning)
        private void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (scrollViewer == null)
                scrollViewer = GetScrollViewer(dgPatients);

            if (scrollViewer != null && e.OriginalSource is ScrollViewer)
            {
                isDragging = true;
                scrollMousePoint = e.GetPosition(scrollViewer);
                scrollViewer.Cursor = Cursors.Hand;
                scrollViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void DataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && scrollViewer != null)
            {
                Point currentMousePoint = e.GetPosition(scrollViewer);
                double deltaX = scrollMousePoint.X - currentMousePoint.X;
                double deltaY = scrollMousePoint.Y - currentMousePoint.Y;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + deltaX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + deltaY);

                scrollMousePoint = currentMousePoint;
            }
        }

        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && scrollViewer != null)
            {
                isDragging = false;
                scrollViewer.Cursor = Cursors.Arrow;
                scrollViewer.ReleaseMouseCapture();
            }
        }

        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ad = txtPatientName.Text.Trim();
                string soyad = txtPatientSurname.Text.Trim();
                string tc = txtPatientTC.Text.Trim();
                string mail = txtPatientEmail.Text.Trim();
                string cinsiyet = cbPatientGender.Text.Trim();
                DateTime? dogum = dpBirthDate.SelectedDate;

                // Validate required fields
                if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad) ||
                    string.IsNullOrEmpty(tc) || dogum == null)
                {
                    MessageBox.Show("Lütfen tüm zorunlu alanları doldurunuz.");
                    return;
                }

                // Validate email format
                if (!string.IsNullOrEmpty(mail) && !IsValidEmail(mail))
                {
                    MessageBox.Show("Geçerli bir e-posta adresi giriniz.");
                    return;
                }

                string sifrePlain = SifreUret(ad);
                byte[ ] sifreHash = HashPasswordToBytes(sifrePlain);

                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Kullanici ekle (cinsiyet ve doğum tarihi eklemyi unutma) 
                            string insertKullanici = @"INSERT INTO Kullanici (tc_kimlik, sifre, rol, ad, soyad, email,dogum_tarihi,cinsiyet) 
                                                  OUTPUT INSERTED.kullanici_id 
                                                  VALUES (@tc, @sifre, 'hasta', @ad, @soyad, @mail,@dogum,@cinsiyet)";

                            SqlCommand cmd1 = new SqlCommand(insertKullanici, conn, transaction);
                            cmd1.Parameters.AddWithValue("@tc", tc);
                            cmd1.Parameters.Add("@sifre", SqlDbType.VarBinary, 32).Value = sifreHash;
                            cmd1.Parameters.AddWithValue("@ad", ad);
                            cmd1.Parameters.AddWithValue("@soyad", soyad);
                            cmd1.Parameters.AddWithValue("@mail", mail);
                            cmd1.Parameters.AddWithValue("@dogum", dogum);
                            cmd1.Parameters.AddWithValue("@cinsiyet", cinsiyet);
                            int kullaniciId = (int)cmd1.ExecuteScalar();

                            // Hasta tablosuna ekle
                            string insertHasta = "INSERT INTO Hasta (hasta_id, doktor_id) VALUES (@kId, @doktorId)";
                            SqlCommand cmd2 = new SqlCommand(insertHasta, conn, transaction);
                            cmd2.Parameters.AddWithValue("@kId", kullaniciId);
                            cmd2.Parameters.AddWithValue("@doktorId", _doktorId);
                            cmd2.ExecuteNonQuery();

                            transaction.Commit();

                            // Only try to send email if database operations succeed
                            if (!string.IsNullOrEmpty(mail))
                            {
                                EpostaGonder(mail, ad, sifrePlain);
                            }

                            MessageBox.Show("Hasta başarıyla eklendi" +
                                (!string.IsNullOrEmpty(mail) ? " ve şifre e-posta ile gönderildi." : "."));

                            // Clear form fields
                            ClearFormFields();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Hasta eklenirken hata oluştu: " + ex.Message);
                        }
                    }
                }

                HastalariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem sırasında bir hata oluştu: " + ex.Message);
            }
        }

        private void HastalariYukle()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    string query = @"SELECT k.tc_kimlik, k.ad, k.soyad FROM Hasta h JOIN Kullanici k ON h.hasta_id = k.kullanici_id
                                   WHERE h.doktor_id = @doktorId";



                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@doktorId", _doktorId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPatients.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hasta listesi yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private byte[ ] HashPasswordToBytes(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        private string SifreUret(string ad)
        {
            return ad + new Random().Next(100, 999);
        }

        private void EpostaGonder(string alici, string ad, string sifre)
        {
            try
            {
                MailMessage mail = new MailMessage(_emailSender, alici);
                mail.Subject = "Diabetes Takip Sistemi - Giriş Bilgileri";
                mail.Body = $"Merhaba {ad},\n\nDiabetes Takip Sistemi'ne giriş için bilgileriniz:\n" +
                            $"Kullanıcı adı: {alici}\n" +
                            $"Şifre: {sifre}\n\n" +
                            $"İlk girişinizde şifrenizi değiştirmeniz önerilir.";
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                string _emailPassword = "cguctdlfmwfzzcni";
                client.Credentials = new NetworkCredential(_emailSender, _emailPassword);

                client.Send(mail);


                // Web uygulamasında JavaScript alert kullanılabilir
                // Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('E-posta başarıyla gönderildi.');", true);
            }
            catch (Exception ex)
            {
                // Hata mesajını loglama
                Console.WriteLine($"E-posta gönderirken hata oluştu: {ex.Message}");
                // Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('E-posta gönderirken hata oluştu: {ex.Message}');", true);
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClearFormFields()
        {
            txtPatientName.Text = string.Empty;
            txtPatientSurname.Text = string.Empty;
            txtPatientTC.Text = string.Empty;
            txtPatientEmail.Text = string.Empty;
            cbPatientGender.SelectedIndex = -1;
            dpBirthDate.SelectedDate = null;
        }


        private void BtnFilterPatients_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();

                    string searchText = txtPatientSearch.Text.Trim();

                    string query = @"
                    SELECT k.tc_kimlik, k.ad, k.soyad
                    FROM Hasta h
                    JOIN Kullanici k ON h.hasta_id = k.kullanici_id
                    WHERE h.doktor_id = @doktorId";

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query += @"
                    AND (
                        k.tc_kimlik LIKE @search OR 
                        k.ad LIKE @search OR 
                        k.soyad LIKE @search
                    )";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@doktorId", _doktorId);

                    if (!string.IsNullOrEmpty(searchText))
                        cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgPatients.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Filtreleme sırasında hata oluştu: " + ex.Message);
            }

        }
        //16.05.2025
        private void LoadPatientIds()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    connection.Open();

                    // Query to get all patient IDs
                    string query = @"SELECT k.kullanici_id FROM Hasta h  JOIN Kullanici " +
                        "k ON h.hasta_id = k.kullanici_id WHERE h.doktor_id = @doktorId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@doktorId", _doktorId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<int> patientIds = new List<int>();
                        while (reader.Read())
                        {
                            patientIds.Add(reader.GetInt32(0));
                        }

                        // Populate combobox with patient IDs
                        cmbPatientId.ItemsSource = patientIds;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanına bağlanırken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }







        private void LoadPatientData(int hastaId)
        {
            MessageBox.Show("LoadPatientData() çalıştı"); // 🔍 TEST

            try
            {
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    connection.Open();

                    string query = @"
                SELECT 
                k.ad + ' ' + k.soyad AS FullName,
                k.tc_kimlik AS TCNo,
                DATEDIFF(YEAR, k.dogum_tarihi, GETDATE()) AS Age,
                 k.cinsiyet AS Gender,
                kt.tarih AS MeasurementDate,
                 kt.saat AS Time,
                 kt.seviye_mg_dl AS BloodSugarLevel,
                dt.diyet_turu AS DietDescription,
                 et.egzersiz_turu AS ExerciseDescription,
                 k.profil_resmi AS ImagePath
                FROM Hasta h
                JOIN Kullanici k ON h.hasta_id = k.kullanici_id

                 LEFT JOIN (
                 SELECT TOP 1 * FROM KanSekeriOlcumu 
                 WHERE hasta_id = @hastaId 
                 ORDER BY tarih DESC, saat DESC
                 ) kt ON kt.hasta_id = h.hasta_id

                 LEFT JOIN (
                 SELECT TOP 1 * FROM DiyetTakibi 
                 WHERE hasta_id = @hastaId 
                 ORDER BY tarih DESC
                 ) dt ON dt.hasta_id = h.hasta_id

                 LEFT JOIN (
                 SELECT TOP 1 * FROM EgzersizTakibi 
                  WHERE hasta_id = @hastaId 
                 ORDER BY tarih DESC
                 ) et ON et.hasta_id = h.hasta_id

                 WHERE h.hasta_id = @hastaId
                 ";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@hastaId", hastaId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fullName = reader["FullName"].ToString();
                            string tcNo = reader["TCNo"].ToString();
                            string gender = reader["Gender"].ToString();
                            int age = Convert.ToInt32(reader["Age"]);

                            string lastMeasurement = "Kayıt yok";
                            if (reader["MeasurementDate"] != DBNull.Value)
                            {
                                DateTime tarih = Convert.ToDateTime(reader["MeasurementDate"]);


                                string seviye = reader["BloodSugarLevel"].ToString();



                                lastMeasurement = $"{tarih:dd.MM.yyyy} - {seviye} mg/dL";
                            }

                            string diet = reader["DietDescription"]?.ToString() ?? "Yok";
                            string exercise = reader["ExerciseDescription"]?.ToString() ?? "Yok";
                            string imagePath = reader["ImagePath"]?.ToString();

                            // UI Güncelleme
                            txtPatientFullName.Text = fullName;
                            txtPatientTCNo.Text = tcNo;
                            txtPatientAgeGender.Text = $"{age} / {gender}";
                            txtLastMeasurement.Text = lastMeasurement;
                            txtCurrentDiet.Text = diet;
                            txtCurrentExercise.Text = exercise;

                            // Hasta Resmini Yükle
                            try
                            {
                                if (reader["ImagePath"] != DBNull.Value)
                                {
                                    byte[ ] imageData = reader["ImagePath"] as byte[ ];
                                    if (imageData != null && imageData.Length > 0)
                                    {
                                        using (var ms = new MemoryStream(imageData))
                                        {
                                            BitmapImage image = new BitmapImage();
                                            image.BeginInit();
                                            image.CacheOption = BitmapCacheOption.OnLoad;
                                            image.StreamSource = ms;
                                            image.EndInit();
                                            imgPatient.Source = image;
                                        }
                                    }
                                    else
                                    {
                                        imgPatient.Source = new BitmapImage(new Uri("/Images/default_patient.png", UriKind.Relative));
                                    }
                                }
                                else
                                {
                                    imgPatient.Source = new BitmapImage(new Uri("/Images/default_patient.png", UriKind.Relative));
                                }
                            }
                            catch
                            {
                                imgPatient.Source = new BitmapImage(new Uri("/Images/default_patient.png", UriKind.Relative));
                            }
                        }
                        else
                        {
                            MessageBox.Show("Hasta bilgisi bulunamadı.");
                            ClearPatientData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yüklenemedi: " + ex.Message);
                ClearPatientData();
            }
        }


        private void ClearPatientData()
        {
            // Clear all patient data fields
            txtPatientFullName.Text = string.Empty;
            txtPatientTCNo.Text = string.Empty;
            txtPatientAgeGender.Text = string.Empty;
            txtLastMeasurement.Text = string.Empty;
            txtCurrentDiet.Text = string.Empty;
            txtCurrentExercise.Text = string.Empty;
            imgPatient.Source = new BitmapImage(new Uri("/Images/default_patient.png", UriKind.Relative));
        }



        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


        //hasta verileri eklenmesi ve öneri sistemi 18.05.2025
        private void btnBelirtiEkle_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBelirtiTuru.SelectedItem is ComboBoxItem seciliItem)
            {
                string belirti = seciliItem.Content.ToString();

                // Eğer aynı belirti daha önce eklenmemişse listeye ekle
                if (!string.IsNullOrWhiteSpace(belirti) && !lstBelirtiler.Items.Contains(belirti))
                {
                    lstBelirtiler.Items.Add(belirti);
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir belirti seçiniz.");
            }
        }


        private void btnBelirtiSil_Click(object sender, RoutedEventArgs e)
        {
            if (lstBelirtiler.SelectedItem != null)
            {
                lstBelirtiler.Items.Remove(lstBelirtiler.SelectedItem);
            }
            else
            {
                MessageBox.Show("Lütfen silinecek bir belirti seçiniz.");
            }
        }



        private int GetNextBelirtiId()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = "SELECT ISNULL(MAX(belirti_id), 0) + 1 FROM Belirti";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }



        private void btnBelirtiKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);
            int yeniBelirtiId = GetNextBelirtiId(); // Elle ID üret

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                foreach (var item in lstBelirtiler.Items)
                {
                    string belirtiAdi = item.ToString();

                    string query = "INSERT INTO Belirti (belirti_id,hasta_id, tarih, belirti_adi) VALUES (@BelirtiId,@hastaId, @tarih, @belirti)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BelirtiId", yeniBelirtiId);
                        cmd.Parameters.AddWithValue("@hastaId", hastaId);
                        cmd.Parameters.AddWithValue("@tarih", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@belirti", belirtiAdi);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Belirtiler başarıyla kaydedildi.");
        }



        private List<KanSekeriOlcumu> olcumListesi = new List<KanSekeriOlcumu>();


        private void btnOlcumEkle_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);

            if (dtpTarihSecimi.SelectedDate == null || string.IsNullOrWhiteSpace(txtSaat.Text) || string.IsNullOrWhiteSpace(txtSekerSeviyesi.Text))
            {
                MessageBox.Show("Lütfen tüm kan şekeri bilgilerini giriniz.");
                return;
            }

            if (!TimeSpan.TryParse(txtSaat.Text, out TimeSpan saat) || !double.TryParse(txtSekerSeviyesi.Text, out double seviye))
            {
                MessageBox.Show("Geçerli saat ve seviye formatı giriniz.");
                return;
            }

            // Saat aralığı kontrolü
            var gecerliSaatAraliklari = new List<(TimeSpan, TimeSpan)>
    {
        (TimeSpan.Parse("07:00"), TimeSpan.Parse("08:00")),
        (TimeSpan.Parse("12:00"), TimeSpan.Parse("13:00")),
        (TimeSpan.Parse("15:00"), TimeSpan.Parse("16:00")),
        (TimeSpan.Parse("18:00"), TimeSpan.Parse("19:00")),
        (TimeSpan.Parse("22:00"), TimeSpan.Parse("23:00"))
    };

            bool aralikIciMi = gecerliSaatAraliklari.Any(a => saat >= a.Item1 && saat <= a.Item2);

            if (!aralikIciMi)
            {
                MessageBox.Show("⚠️ Ölçüm saat aralığı dışında! Kayıt yapılacak ancak ortalamaya dahil edilmeyecektir.");
            }

            // Ölçüm bilgilerini hem listeye hem tabloya ekle
            var yeniOlcum = new KanSekeriOlcumu
            {
                Tarih = dtpTarihSecimi.SelectedDate.Value,
                Saat = saat,
                Seviye = seviye
            };
            olcumListesi.Add(yeniOlcum); // 🔴 bu kritik

            // Görsel tabloya ekle
            DataTable dt = dgKanSekeriOlcumleri.ItemsSource as DataTable;
            if (dt == null || dt.Columns.Count == 0)
            {
                dt = new DataTable();
                dt.Columns.Add("Tarih", typeof(DateTime));
                dt.Columns.Add("Saat", typeof(TimeSpan));
                dt.Columns.Add("Seviye", typeof(double));
            }

            dt.Rows.Add(yeniOlcum.Tarih, yeniOlcum.Saat, yeniOlcum.Seviye);
            dgKanSekeriOlcumleri.ItemsSource = dt.DefaultView;

            // Giriş kutularını temizle
            txtSaat.Clear();
            txtSekerSeviyesi.Clear();

            // Eksik ve yetersiz veri kontrolü
            int olcumSayisi = olcumListesi.Count;

            if (olcumSayisi < 5)
            {
                MessageBox.Show($"⚠️ Ölçüm eksik! Ortalama alınırken bazı ölçümler hesaba katılmayacak. {olcumSayisi}");
            }

            if (olcumSayisi <= 3)
            {
                MessageBox.Show("⚠️ Yetersiz veri! Ortalama hesaplaması güvenilir değildir.");
            }
        }



        private void btnOlcumSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKanSekeriOlcumleri.SelectedItem is KanSekeriOlcumu secili)
            {
                olcumListesi.Remove(secili);
                dgKanSekeriOlcumleri.ItemsSource = null;
                dgKanSekeriOlcumleri.ItemsSource = olcumListesi;
            }
        }

        private void btnOlcumKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);

            if (olcumListesi == null || olcumListesi.Count == 0)
            {
                MessageBox.Show("Kaydedilecek ölçüm bulunmamaktadır.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // Mevcut maksimum olcum_id'yi al
                string getMaxIdQuery = "SELECT ISNULL(MAX(olcum_id), 0) FROM KanSekeriOlcumu";
                SqlCommand getMaxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                int currentMaxId = (int)getMaxIdCmd.ExecuteScalar();

                foreach (var olcum in olcumListesi)
                {
                    currentMaxId++; // her ölçüm için yeni ID

                    string insertQuery = @"
                INSERT INTO KanSekeriOlcumu (olcum_id, hasta_id, tarih, saat, seviye_mg_dl)
                VALUES (@id, @hastaId, @tarih, @saat, @seviye)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@id", currentMaxId);
                    insertCmd.Parameters.AddWithValue("@hastaId", hastaId);
                    insertCmd.Parameters.AddWithValue("@tarih", olcum.Tarih.Date);
                    insertCmd.Parameters.AddWithValue("@saat", olcum.Saat);
                    insertCmd.Parameters.AddWithValue("@seviye", olcum.Seviye);

                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kan şekeri ölçümleri başarıyla kaydedildi.");
            }

            // Listeyi temizle
            olcumListesi.Clear();
            dgKanSekeriOlcumleri.ItemsSource = null;
        }



        private void btnInsulinOneriKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);

            if (dtpTarihSecimi.SelectedDate == null)
            {
                MessageBox.Show("Lütfen bir tarih seçiniz.");
                return;
            }

            DateTime secilenTarih = dtpTarihSecimi.SelectedDate.Value;

            // Veritabanından ölçümleri al
            List<double> seviyeListesi = new List<double>();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
            SELECT seviye_mg_dl 
            FROM KanSekeriOlcumu 
            WHERE hasta_id = @hastaId AND CAST(tarih AS DATE) = @tarih";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@tarih", secilenTarih.Date);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (double.TryParse(reader["seviye_mg_dl"].ToString(), out double seviye))
                        {
                            seviyeListesi.Add(seviye);
                        }
                    }
                }
            }

            if (seviyeListesi.Count == 0)
            {
                MessageBox.Show("Seçilen tarihe ait veritabanında kan şekeri ölçümü bulunamadı.");
                return;
            }

            // Ortalama hesapla
            double ortalama = seviyeListesi.Average();
            txtOrtalamaSeker.Text = ortalama.ToString("F1");

            // Doz önerisini belirle
            float? doz = null;

            if (ortalama >= 111 && ortalama <= 150)
                doz = 1f;
            else if (ortalama >= 151 && ortalama <= 200)
                doz = 2f;
            else if (ortalama > 200)
                doz = 3f;

            txtInsülinDoz.Text = doz.HasValue ? $"{doz} ml" : "Yok";

            MessageBox.Show($"📊 Ortalama: {ortalama:F1} mg/dL\n💉 Önerilen Doz: {doz} ml");

            // Veritabanına öneriyi kaydetmeden önce mevcut max ID'yi al
            int currentMaxId = 0;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string getMaxIdQuery = "SELECT ISNULL(MAX(oner_id), 0) FROM InsulinOnerisi";
                SqlCommand getMaxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                currentMaxId = (int)getMaxIdCmd.ExecuteScalar();

                // Yeni öneri kaydı
                string insertQuery = @"
            INSERT INTO InsulinOnerisi (oner_id, hasta_id, tarih, ortalama_seker, doz_ml) 
            VALUES (@onerId, @hastaId, @tarih, @ortalama, @doz)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@onerId", currentMaxId + 1); // manuel ID
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@tarih", secilenTarih);
                cmd.Parameters.AddWithValue("@ortalama", ortalama);
                if (doz.HasValue)
                    cmd.Parameters.AddWithValue("@doz", doz.Value);
                else
                    cmd.Parameters.AddWithValue("@doz", DBNull.Value);  // NULL destekliyorsa

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu: " + ex.Message);
                }

            }
        }



        private List<EgzersizTakibi> egzersizListesi = new List<EgzersizTakibi>();
        private void btnEgzersizEkle_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null || cmbEgzersizTuru.SelectedItem == null || dtpTarihSecimi.SelectedDate == null)
            {
                MessageBox.Show("Lütfen hasta, tarih ve egzersiz türünü seçiniz.");
                return;
            }

            DataTable dt = dgEgzersizListesi.ItemsSource as DataTable ?? new DataTable();
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("Tarih", typeof(DateTime));
                dt.Columns.Add("EgzersizTuru", typeof(string));
                dt.Columns.Add("YapildiMi", typeof(bool));
            }

            dt.Rows.Add(dtpTarihSecimi.SelectedDate.Value, (cmbEgzersizTuru.SelectedItem as ComboBoxItem).Content.ToString(), false);
            dgEgzersizListesi.ItemsSource = dt.DefaultView;

            cmbEgzersizTuru.SelectedIndex = -1;
        }


        private void btnEgzersizSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgEgzersizListesi.SelectedItem is EgzersizTakibi secili)
            {
                egzersizListesi.Remove(secili);
                dgEgzersizListesi.ItemsSource = null;
                dgEgzersizListesi.ItemsSource = egzersizListesi;
            }
        }

        private void btnEgzersizKaydet_Click(object sender, RoutedEventArgs e)
        {
            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                foreach (var eg in egzersizListesi)
                {
                    string query = "INSERT INTO EgzersizTakibi (hasta_id, tarih, egzersiz_turu, yapildi_mi) VALUES (@id, @t, @tur, @yapildi)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", hastaId);
                    cmd.Parameters.AddWithValue("@t", eg.Tarih);
                    cmd.Parameters.AddWithValue("@tur", eg.EgzersizTuru);
                    cmd.Parameters.AddWithValue("@yapildi", eg.UygulandiMi);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Egzersiz kayıtları kaydedildi.");
            }

        }

        private void btnDiyetEkle_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null || cmbDiyetTuru.SelectedItem == null || dtpTarihSecimi.SelectedDate == null)
            {
                MessageBox.Show("Lütfen hasta, tarih ve diyet türünü seçiniz.");
                return;
            }

            DataTable dt = dgDiyetListesi.ItemsSource as DataTable ?? new DataTable();
            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("Tarih", typeof(DateTime));
                dt.Columns.Add("DiyetTuru", typeof(string));
                dt.Columns.Add("UygulandiMi", typeof(bool));
            }

            dt.Rows.Add(dtpTarihSecimi.SelectedDate.Value, (cmbDiyetTuru.SelectedItem as ComboBoxItem).Content.ToString(), false);
            dgDiyetListesi.ItemsSource = dt.DefaultView;

            cmbDiyetTuru.SelectedIndex = -1;
        }


        private void btnDiyetSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiyetListesi.SelectedItem != null)
            {
                dgDiyetListesi.Items.Remove(dgDiyetListesi.SelectedItem);
            }
        }


        private void btnDiyetKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                foreach (var item in dgDiyetListesi.Items)
                {
                    if (item is DataRowView row)
                    {
                        DateTime tarih = Convert.ToDateTime(row["Tarih"]);
                        string diyetTuru = row["DiyetTuru"].ToString();
                        bool uygulandi = Convert.ToBoolean(row["UygulandiMi"]);

                        string query = @"INSERT INTO DiyetTakibi (hasta_id, tarih, diyet_turu, uygulandi_mi)
                                 VALUES (@hastaId, @tarih, @diyetTuru, @uygulandiMi)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@hastaId", hastaId);
                            cmd.Parameters.AddWithValue("@tarih", tarih);
                            cmd.Parameters.AddWithValue("@diyetTuru", diyetTuru);
                            cmd.Parameters.AddWithValue("@uygulandiMi", uygulandi);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Diyet verileri kaydedildi.");
            }
        }


        public string GenerateRecommendation(List<Belirti> belirtiler, List<KanSekeriOlcumu> olcumler)
        {
            string diyet = "Dengeli Beslenme";
            string egzersiz = "Yürüyüş";
            List<string> yorumlar = new List<string>();

            // Belirti kontrolü
            var belirtiAdlari = belirtiler.Select(b => b.BelirtiAdi).ToList();

            if (belirtiAdlari.Any(b => b.Contains("Poliüri") || b.Contains("Polifaji") || b.Contains("Polidipsi")))
            {
                diyet = "Şekersiz Diyet";
                yorumlar.Add("Hiperglisemi belirtisi mevcut. Şekersiz diyete geçilmesi önerilir.");
            }
            else if (belirtiAdlari.Any(b => b.Contains("Kilo kaybı") || b.Contains("Yorgunluk")))
            {
                diyet = "Az Şekerli Diyet";
                yorumlar.Add("Beslenme desteği için az şekerli diyete geçilmesi önerilir.");
            }

            if (belirtiAdlari.Any(b => b.Contains("Nöropati")))
            {
                egzersiz = "Klinik Egzersiz";
                yorumlar.Add("Nöropati belirtisi görüldü. Klinik egzersiz önerilir.");
            }
            else if (belirtiAdlari.Any(b => b.Contains("Yorgunluk") || b.Contains("Kilo kaybı")))
            {
                egzersiz = "Yürüyüş";
            }
            else
            {
                egzersiz = "Bisiklet";
            }

            // Ortalama kan şekeri
            var gecerliOlcumler = olcumler
                .Where(o => o.Saat >= TimeSpan.FromHours(7) && o.Saat <= TimeSpan.FromHours(23))
                .Select(o => o.Seviye)
                .ToList();

            double ortalama = gecerliOlcumler.Count > 0 ? gecerliOlcumler.Average() : 0;

            if (gecerliOlcumler.Count < 3)
            {
                yorumlar.Add("Yetersiz veri! Ortalama hesaplaması güvenilir değildir.");
            }

            if (gecerliOlcumler.Count < olcumler.Count)
            {
                yorumlar.Add("Ölçüm eksik! Ortalama alınırken bazı ölçümler hesaba katılmadı.");
            }

            if (ortalama < 70)
            {
                yorumlar.Add("Hipoglisemi riski mevcut. Egzersiz süresi azaltılmalı, öğün sıklığı artırılmalı.");
            }
            else if (ortalama >= 126)
            {
                yorumlar.Add("Diyabet seviyesi yüksek. Diyet ve egzersiz planı sıkılaştırılmalı.");
            }

            return $"💡 Diyet Önerisi: {diyet}\n🏃 Egzersiz Önerisi: {egzersiz}\n📊 Ortalama Şeker: {ortalama:F1} mg/dL\n📌 Açıklamalar:\n- {string.Join("\n- ", yorumlar)}";
        }


        private List<Belirti> GetBelirtilerFromDatabase(int hastaId)
        {
            var list = new List<Belirti>();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = "SELECT belirti_adi FROM Belirti WHERE hasta_id = @hastaId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaId", hastaId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Belirti
                            {
                                HastaId = hastaId,
                                BelirtiAdi = reader["belirti_adi"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        private List<KanSekeriOlcumu> GetOlcumlerFromDatabase(int hastaId)
        {

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = "SELECT tarih, saat, seviye_mg_dl FROM KanSekeriOlcumu WHERE hasta_id = @hastaId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaId", hastaId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            olcumListesi.Add(new KanSekeriOlcumu
                            {
                                HastaId = hastaId,
                                Tarih = Convert.ToDateTime(reader["tarih"]),
                                Saat = TimeSpan.Parse(reader["saat"].ToString()),
                                Seviye = Convert.ToDouble(reader["seviye_mg_dl"])
                            });
                        }
                    }
                }
            }
            return olcumListesi;
        }




        private void btnTumVerileriKaydet_Click(object sender, RoutedEventArgs e)
        {
            // 1. Önce tüm alt kayıt işlemlerini gerçekleştir
            btnBelirtiKaydet_Click(sender, e);
            btnOlcumKaydet_Click(sender, e);
            btnInsulinOneriKaydet_Click(sender, e);
            btnEgzersizKaydet_Click(sender, e);
            btnDiyetKaydet_Click(sender, e);

            // 2. Hasta ID kontrolü
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);

            // 3. Belirti ve kan şekeri verilerini veri tabanından çek
            var belirtiler = GetBelirtilerFromDatabase(hastaId);
            var olcumler = GetOlcumlerFromDatabase(hastaId);

            // 4. Öneri üret
            string oneriler = GenerateRecommendation(belirtiler, olcumler);

            // 5. Doktora önerileri göster
            MessageBox.Show(oneriler, "📌 Sistem Önerileri", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            cmbBelirtiTuru.SelectedIndex = -1;   // ComboBox temizlenir
            lstBelirtiler.Items.Clear();
            lstBelirtiler.Items.Clear();
            txtSaat.Clear();
            txtSekerSeviyesi.Clear();
            dgKanSekeriOlcumleri.ItemsSource = null;
            txtOrtalamaSeker.Clear();
            txtInsülinDoz.Clear();
            dgEgzersizListesi.ItemsSource = null;
            dgDiyetListesi.ItemsSource = null;
        }


        private void cmbEgzersizTuru_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbDiyetTuru_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dtpTarihSecimi_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            txtSaat.Clear();
            txtSekerSeviyesi.Clear();
            dgKanSekeriOlcumleri.ItemsSource = null;
            dgEgzersizListesi.ItemsSource = null;
            dgDiyetListesi.ItemsSource = null;
        }


        // Buraya kadar düzenledm

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


        private List<KanSekeriOlcumu> GetFilteredBloodSugarData(int hastaId, DateTime tarih, TimeSpan baslangic, TimeSpan bitis)
        {
            var records = new List<KanSekeriOlcumu>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
         SELECT tarih, saat, seviye_mg_dl
        FROM KanSekeriOlcumu
        WHERE hasta_id = @hastaId AND tarih = @tarih 
          AND saat BETWEEN @baslangic AND @bitis";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaId", hastaId);
                    cmd.Parameters.AddWithValue("@tarih", tarih.Date);
                    cmd.Parameters.AddWithValue("@baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@bitis", bitis);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new KanSekeriOlcumu
                            {
                                Tarih = Convert.ToDateTime(reader["tarih"]),
                                Saat = TimeSpan.Parse(reader["saat"].ToString()),
                                Seviye = Convert.ToDouble(reader["seviye_mg_dl"])  // Fixed column name
                            });
                        }
                    }
                }
            }

            return records;
        }

        private void UpdateStatistics(List<KanSekeriOlcumu> records)
        {
            if (records.Count == 0)
            {
                txtAverage.Text = "-- mg/dL";
                txtMinimum.Text = "-- mg/dL";
                txtMaximum.Text = "-- mg/dL";
                txtMeasurementCount.Text = "0";
                return;
            }

            // Calculate and display statistics
            txtAverage.Text = records.Average(r => r.Seviye).ToString("F1") + " mg/dL";
            txtMinimum.Text = records.Min(r => r.Seviye).ToString("F0") + " mg/dL";
            txtMaximum.Text = records.Max(r => r.Seviye).ToString("F0") + " mg/dL";
            txtMeasurementCount.Text = records.Count.ToString();
        }

        private void LoadBloodSugarWithFilters()
        {
            // Hasta seçimi kontrolü
            object selectedItem = cmbPatientId.SelectedItem;

            if (selectedItem == null || !(selectedItem is int hastaId))
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            // Ölçüm tipi ve tarih seçimi kontrolü
            if (cbMeasurementType.SelectedItem == null || datePicker.SelectedDate == null)
            {
                MessageBox.Show("Lütfen ölçüm türü ve tarih seçiniz.");
                return;
            }

            DateTime tarih = datePicker.SelectedDate.Value;
            var saatAraligi = ParseSaatAraligi();
            TimeSpan baslangic = saatAraligi.baslangic;
            TimeSpan bitis = saatAraligi.bitis;

            // Verileri çek
            var veriler = GetFilteredBloodSugarData(hastaId, tarih, baslangic, bitis);

            // Tabloya ata
            dgBloodGlucose.ItemsSource = veriler;

            // İstatistikleri güncelle
            UpdateStatistics(veriler);

            // Ölçüm değeri girişini temizle
            if (txtBloodSugarValue != null)
                txtBloodSugarValue.Text = string.Empty;

            // Saat alanına varsayılan başlangıç saati ata
            if (timeInput != null && baslangic != TimeSpan.Zero)
                timeInput.Text = baslangic.ToString(@"hh\:mm");

            // Kullanıcıya bilgi ver (isteğe bağlı)
            if (veriler.Count == 0)
            {
                MessageBox.Show("Seçilen tarih ve zaman aralığında ölçüm bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private (TimeSpan baslangic, TimeSpan bitis) ParseSaatAraligi()
        {
            if (cbMeasurementType.SelectedItem is ComboBoxItem selectedItem)
            {
                // Example: "Sabah Ölçümü (07:00-08:00)"
                var icerik = selectedItem.Content.ToString();
                var saatParca = icerik.Split('(')[1].TrimEnd(')'); // "07:00-08:00"
                var saatler = saatParca.Split('-');

                TimeSpan baslangic = TimeSpan.Parse(saatler[0]);
                TimeSpan bitis = TimeSpan.Parse(saatler[1]);

                return (baslangic, bitis);
            }

            return (TimeSpan.Zero, TimeSpan.Zero); // Invalid selection
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBloodSugarWithFilters();
        }

        private void MeasurementType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBloodSugarWithFilters();

            // Set the time input field based on selected measurement type
            if (timeInput != null && cbMeasurementType.SelectedItem != null)
            {
                var (baslangic, _) = ParseSaatAraligi();
                if (baslangic != TimeSpan.Zero)
                    timeInput.Text = baslangic.ToString(@"hh\:mm");
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
                string query = @"
            SELECT 
                COALESCE(e.tarih, d.tarih) AS Tarih,
                e.egzersiz_turu,
                e.yapildi_mi,
                d.diyet_turu,
                d.uygulandi_mi
            FROM EgzersizTakibi e
            FULL OUTER JOIN DiyetTakibi d ON e.hasta_id = d.hasta_id AND e.tarih = d.tarih
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
            }

            // DataGrid'e bağla
            dgExerciseDiet.ItemsSource = records;

            // ProgressBar ve yüzdeler (isimleri XAML'da tanımlamayı unutma!)
            double egzersizUyum = egzersizToplam == 0 ? 0 : (egzersizYapilan * 100.0 / egzersizToplam);
            double diyetUyum = diyetToplam == 0 ? 0 : (diyetUygulanan * 100.0 / diyetToplam);
            double genelUyum = (egzersizUyum + diyetUyum) / 2;

            pbEgzersiz.Width = egzersizUyum;
            pbDiyet.Width = diyetUyum;
            pbGenel.Width = genelUyum;

            pbEgzersiz.Value = egzersizUyum;
            pbDiyet.Value = diyetUyum;
            pbGenel.Value = genelUyum;

            txtEgzersizYuzde.Text = $"%{Math.Round(egzersizUyum)} Uyum (Son 30 gün)";
            txtDiyetYuzde.Text = $"%{Math.Round(diyetUyum)} Uyum (Son 30 gün)";
            txtGenelYuzde.Text = $"%{Math.Round(genelUyum)} Uyum (Son 30 gün)";
        }
        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is ProgressBar bar)
            {
                double value = bar.Value;

    

                // Renk ayarla
                if (value >= 100)
                { 
                    bar.Background = Brushes.Green;
                    bar.Foreground = Brushes.Green;
                }
                else if (value >= 50)
                {
                    bar.Background = Brushes.Orange;
                    bar.Foreground = Brushes.Orange;
                }
                else
                {
                    bar.Background = Brushes.Red;
                    bar.Foreground = Brushes.Red;
                }

                // Bağlı TextBlock'u güncelle
                if (bar.Name == "pbEgzersiz")
                    txtEgzersizYuzde.Text = $"%{(int)value} Uyum (Son 30 gün)";
                else if (bar.Name == "pbDiyet")
                    txtDiyetYuzde.Text = $"%{(int)value} Uyum (Son 30 gün)";
                else if (bar.Name == "pbGenel")
                    txtGenelYuzde.Text = $"%{(int)value} Uyum (Son 30 gün)";
            }
        }



        private void cmbPatientId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            if (cmbPatientId.SelectedValue != null)
            {
                int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);
                LoadPatientData(hastaId);
                LoadBloodSugarWithFilters();
                LoadCombinedExerciseDietData(hastaId);

            }
        }
        //20.05.2025

        private List<HastaListeModel> tumHastalar = new List<HastaListeModel>();

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            // Filtreleme kriterlerini oku
            string searchText = txtSearch.Text.Trim().ToLower();
            double? minGlucose = double.TryParse(txtGlucoseMin.Text, out double min) ? min : (double?)null;
            double? maxGlucose = double.TryParse(txtGlucoseMax.Text, out double max) ? max : (double?)null;

            // Belirti filtreleri
            List<string> aktifSemptomlar = new List<string>();
            if (cbFilterPolyuria.IsChecked == true)
                aktifSemptomlar.Add("Poliüri");
            if (cbFilterPolydipsia.IsChecked == true)
                aktifSemptomlar.Add("Polidipsi");
            if (cbFilterPolyphagia.IsChecked == true)
                aktifSemptomlar.Add("Polifaji");
            if (cbFilterNeuropathy.IsChecked == true)
                aktifSemptomlar.Add("Nöropati");
            if (cbFilterWeightLoss.IsChecked == true)
                aktifSemptomlar.Add("Kilo kaybı");
            if (cbFilterFatigue.IsChecked == true)
                aktifSemptomlar.Add("Yorgunluk");
            if (cbFilterSlowHealing.IsChecked == true)
                aktifSemptomlar.Add("Yaraların yavaş iyileşmesi");
            if (cbFilterBlurredVision.IsChecked == true)
                aktifSemptomlar.Add("Bulanık görme");
            if (cbFilterFrequentInfections.IsChecked == true)
                aktifSemptomlar.Add("Sık enfeksiyonlar");

            // Hasta listesini yeniden yükle ve filtrele
            var tumHastalar = LoadAllPatients(); // Bu metod hastaları List<PatientModel> olarak döndürmeli

            var filtrelenmis = tumHastalar.Where(p =>
                (string.IsNullOrEmpty(searchText) || p.FullName.ToLower().Contains(searchText)) &&
                (!minGlucose.HasValue || p.GlucoseLevel >= minGlucose.Value) &&
                (!maxGlucose.HasValue || p.GlucoseLevel <= maxGlucose.Value) &&
                (!aktifSemptomlar.Any() || aktifSemptomlar.Any(s => p.Symptoms.Contains(s)))
            ).ToList();

            lvPatients.ItemsSource = filtrelenmis;
        }


        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            txtGlucoseMin.Text = "";
            txtGlucoseMax.Text = "";

            cbFilterPolyuria.IsChecked = false;
            cbFilterPolydipsia.IsChecked = false;
            cbFilterPolyphagia.IsChecked = false;
            cbFilterNeuropathy.IsChecked = false;
            cbFilterWeightLoss.IsChecked = false;
            cbFilterFatigue.IsChecked = false;
            cbFilterSlowHealing.IsChecked = false;
            cbFilterBlurredVision.IsChecked = false;
            cbFilterFrequentInfections.IsChecked = false;

            lvPatients.ItemsSource = tumHastalar;
        }

        private void ApplyFilters()
        {
            string searchTerm = txtSearch.Text.Trim().ToLower();

            double.TryParse(txtGlucoseMin.Text, out double min);
            double.TryParse(txtGlucoseMax.Text, out double max);

            var filtreli = tumHastalar.Where(h =>
                (string.IsNullOrEmpty(searchTerm) || h.FullName.ToLower().Contains(searchTerm)) &&
                (min == 0 || h.GlucoseLevel >= min) &&
                (max == 0 || h.GlucoseLevel <= max)
            ).ToList();

            // Belirti filtrelerine göre filtreleme (örnek: poliüri varsa sadece bu belirtiye sahip olanlar)
            List<string> seciliBelirtiler = new List<string>();
            if (cbFilterPolyuria.IsChecked == true)
                seciliBelirtiler.Add("Poliüri");
            if (cbFilterPolydipsia.IsChecked == true)
                seciliBelirtiler.Add("Polidipsi");
            if (cbFilterPolyphagia.IsChecked == true)
                seciliBelirtiler.Add("Polifaji");
            if (cbFilterNeuropathy.IsChecked == true)
                seciliBelirtiler.Add("Nöropati");
            if (cbFilterWeightLoss.IsChecked == true)
                seciliBelirtiler.Add("Kilo kaybı");
            if (cbFilterFatigue.IsChecked == true)
                seciliBelirtiler.Add("Yorgunluk");
            if (cbFilterSlowHealing.IsChecked == true)
                seciliBelirtiler.Add("Yaraların yavaş iyileşmesi");
            if (cbFilterBlurredVision.IsChecked == true)
                seciliBelirtiler.Add("Bulanık görme");
            if (cbFilterFrequentInfections.IsChecked == true)
                seciliBelirtiler.Add("Sık enfeksiyonlar");

            if (seciliBelirtiler.Any())
            {
                filtreli = filtreli.Where(h => HastaBelirtiKontrol(h.PatientId, seciliBelirtiler)).ToList();
            }

            lvPatients.ItemsSource = filtreli;
        }

        private bool HastaBelirtiKontrol(int hastaId, List<string> seciliBelirtiler)
        {
            List<string> hastaBelirtileri = new List<string>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT belirti_adi FROM Belirti WHERE hasta_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", hastaId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    hastaBelirtileri.Add(reader["belirti_adi"].ToString());
                }
            }

            // En az bir eşleşme yeterlidir
            return seciliBelirtiler.Any(b => hastaBelirtileri.Contains(b));
        }

        private List<HastaListeModel> LoadAllPatients()
        {
            var list = new List<HastaListeModel>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
            SELECT h.hasta_id, k.ad + ' ' + k.soyad AS FullName,
                   DATEDIFF(YEAR, k.dogum_tarihi, GETDATE()) AS Age,
                   k.email, k.cinsiyet,
                   ISNULL(ko.seviye_mg_dl, 0) AS GlucoseLevel
            FROM Hasta h
            JOIN Kullanici k ON h.hasta_id = k.kullanici_id
            LEFT JOIN (
                SELECT hasta_id, MAX(tarih) AS SonTarih
                FROM KanSekeriOlcumu
                GROUP BY hasta_id
            ) son ON h.hasta_id = son.hasta_id
            LEFT JOIN KanSekeriOlcumu ko 
                ON ko.hasta_id = son.hasta_id AND ko.tarih = son.SonTarih";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hastaId = Convert.ToInt32(reader["hasta_id"]);

                        var model = new HastaListeModel
                        {
                            PatientId = hastaId,
                            FullName = reader["FullName"].ToString(),
                            Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : 0,
                            Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "Bilinmiyor",
                            Gender = reader["cinsiyet"] != DBNull.Value ? reader["cinsiyet"].ToString() : "Belirtilmemiş",
                            GlucoseLevel = reader["GlucoseLevel"] != DBNull.Value ? Convert.ToDouble(reader["GlucoseLevel"]) : 0.0
                        };

                        // Belirtileri ekle
                        try
                        {
                            model.Symptoms = GetSymptoms(hastaId);
                        }
                        catch
                        {
                            model.Symptoms = new List<string>(); // hata durumunda boş liste
                        }

                        list.Add(model);
                    }


                }
            }

            return list;
        }

        private List<string> GetSymptoms(int hastaId)
        {
            var list = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = "SELECT belirti_adi FROM Belirti WHERE hasta_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", hastaId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["belirti_adi"].ToString());
                    }
                }
            }
            return list;
        }
        //burada kaldımm
       

        // Helper class for diet and exercise correlation data
        public class DiyetEgzersizSonuc
        {
            public string DiyetTuru { get; set; }
            public string EgzersizTuru { get; set; }
            public double OrtalamaSeker { get; set; }
        }
        private void LoadBloodSugarChart(int hastaId, string timeRange, string viewType)
        {
            var data = new List<KanSekeriOlcumu>();
            DateTime startDate;

            switch (timeRange)
            {
                case "Son 1 Ay":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "Son 3 Ay":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
                case "Son 6 Ay":
                    startDate = DateTime.Now.AddMonths(-6);
                    break;
                case "Son 1 Yıl":
                    startDate = DateTime.Now.AddYears(-1);
                    break;
                default:
                    startDate = new DateTime(1753, 1, 1);
                    break;
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT tarih, AVG(seviye_mg_dl) as Ortalama
FROM KanSekeriOlcumu
WHERE hasta_id = @hastaId AND tarih >= @startDate ";

                switch (viewType)
                {
                    case "Haftalık Ortalama":
                        query += "GROUP BY DATEPART(YEAR, tarih), DATEPART(WEEK, tarih)";
                        break;
                    case "Aylık Ortalama":
                        query += "GROUP BY DATEPART(YEAR, tarih), DATEPART(MONTH, tarih)";
                        break;
                    default:
                        query += "GROUP BY tarih";
                        break;
                }

                query += " ORDER BY tarih";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@startDate", startDate);

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

            if (data.Count == 0)
            {
                // Örnek veri
                for (int i = 7; i >= 1; i--)
                {
                    data.Add(new KanSekeriOlcumu
                    {
                        Tarih = DateTime.Now.AddDays(-i),
                        Seviye = 100 + i * 2
                    });
                }
            }

            string dateFormat = viewType == "Aylık Ortalama" ? "MM.yyyy" : "dd.MM";

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
                Labels = data.Select(d => d.Tarih.ToString(dateFormat)).ToArray(),
                Separator = new LiveCharts.Wpf.Separator { Step = Math.Max(1, data.Count / 10) }
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
            bloodSugarChart.DataTooltip = new DefaultTooltip { SelectionMode = TooltipSelectionMode.SharedYValues };
        }


        private void LoadDietExerciseChart(int hastaId, string diyet, string egzersiz)
        {
            // Here we'll create a visualization that shows the correlation between
            // diet, exercise and blood sugar levels

            var dietExerciseData = new List<DiyetEgzersizSonuc>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT 
    d.diyet_turu as DiyetTuru,
    e.egzersiz_turu as EgzersizTuru,
    AVG(k.seviye_mg_dl) AS OrtalamaSeker
FROM KanSekeriOlcumu k
JOIN DiyetTakibi d ON k.hasta_id = d.hasta_id AND k.tarih = d.tarih
JOIN EgzersizTakibi e ON k.hasta_id = e.hasta_id AND k.tarih = e.tarih
WHERE k.hasta_id = @hastaId
AND (@diyet = 'Tümü' OR d.diyet_turu = @diyet)
AND (@egzersiz = 'Tümü' OR e.egzersiz_turu = @egzersiz)
GROUP BY d.diyet_turu, e.egzersiz_turu
ORDER BY AVG(k.seviye_mg_dl)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@diyet", diyet);
                cmd.Parameters.AddWithValue("@egzersiz", egzersiz);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dietExerciseData.Add(new DiyetEgzersizSonuc
                        {
                            DiyetTuru = reader["DiyetTuru"].ToString(),
                            EgzersizTuru = reader["EgzersizTuru"].ToString(),
                            OrtalamaSeker = Convert.ToDouble(reader["OrtalamaSeker"])
                        });
                    }
                }
            }

            // Check if data is empty and add sample data if needed
            if (dietExerciseData.Count == 0)
            {
                // Add sample data for visualization testing
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Az Şekerli Diyet", EgzersizTuru = "Haftada 1-2", OrtalamaSeker = 130 });
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Şekersiz Diyet", EgzersizTuru = "Haftada 3-4", OrtalamaSeker = 120 });
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Dengeli Beslenme", EgzersizTuru = "Haftada 5+", OrtalamaSeker = 110 });
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Akdeniz", EgzersizTuru = "Haftada 1-2", OrtalamaSeker = 135 });
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Akdeniz", EgzersizTuru = "Haftada 3-4", OrtalamaSeker = 125 });
                dietExerciseData.Add(new DiyetEgzersizSonuc { DiyetTuru = "Akdeniz", EgzersizTuru = "Haftada 5+", OrtalamaSeker = 115 });
            }

            // Clear previous series
            dietExcerciseChart.Series = new SeriesCollection();

            // Group by diet type
            var exerciseTypes = dietExerciseData.Select(d => d.EgzersizTuru).Distinct().ToList();
            var dietTypes = dietExerciseData.Select(d => d.DiyetTuru).Distinct().ToList();

            foreach (var diet in dietTypes)
            {
                var seriesValues = new ChartValues<double>();

                foreach (var exercise in exerciseTypes)
                {
                    var dataPoint = dietExerciseData.FirstOrDefault(d =>
                        d.DiyetTuru == diet && d.EgzersizTuru == exercise);

                    if (dataPoint != null)
                        seriesValues.Add(dataPoint.OrtalamaSeker);
                    else
                        seriesValues.Add(0); // Empty value for this combination
                }

                dietExcerciseChart.Series.Add(new ColumnSeries
                {
                    Title = diet,
                    Values = seriesValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F0")
                });
            }

            dietExcerciseChart.AxisX.Clear();
            dietExcerciseChart.AxisX.Add(new Axis
            {
                Title = "Egzersiz Sıklığı",
                Labels = exerciseTypes.ToArray(),
                Separator = new LiveCharts.Wpf.Separator { Step = 1 }
            });

            dietExcerciseChart.AxisY.Clear();
            dietExcerciseChart.AxisY.Add(new Axis
            {
                Title = "Ortalama Kan Şekeri (mg/dL)",
                MinValue = 0,
                // Calculate a reasonable max value based on the data
                MaxValue = Math.Ceiling(dietExerciseData.Max(d => d.OrtalamaSeker) / 50) * 50, // Round up to nearest 50
                LabelFormatter = value => value.ToString("F0") // No decimal places
            });

            // Improve chart appearance
            dietExcerciseChart.DisableAnimations = true;
            dietExcerciseChart.LegendLocation = LegendLocation.Top;
        }

        private void btnGuncelleChart_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);
            string timeRange = (cbTimeRange.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tüm Zamanlar";
            string viewType = (cbChartView.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Günlük Ortalama";

            LoadBloodSugarChart(hastaId, timeRange, viewType);
        }


        private void AnalyzeDietExerciseEffect(int hastaId, string diyet, string egzersiz)
        {
            List<double> sonucListesi = new List<double>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                string query = @"
SELECT AVG(k.seviye_mg_dl) AS Ortalama
FROM KanSekeriOlcumu k
JOIN DiyetTakibi d ON k.hasta_id = d.hasta_id AND k.tarih = d.tarih
JOIN EgzersizTakibi e ON k.hasta_id = e.hasta_id AND k.tarih = e.tarih
WHERE k.hasta_id = @hastaId
AND (@diyet = 'Tümü' OR d.diyet_turu = @diyet)
AND (@egzersiz = 'Tümü' OR e.egzersiz_turu = @egzersiz)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hastaId", hastaId);
                cmd.Parameters.AddWithValue("@diyet", diyet);
                cmd.Parameters.AddWithValue("@egzersiz", egzersiz);

                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    double ort = Convert.ToDouble(result);
                    MessageBox.Show($"Seçilen kriterlere göre ortalama kan şekeri: {ort:F1} mg/dL");
                }
                else
                {
                    MessageBox.Show("Veri bulunamadı.");
                }

                // Also update the chart visualization
                LoadDietExerciseChart(hastaId, diyet, egzersiz);
            }
        }

        private void btnAnalizEt_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPatientId.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir hasta seçiniz.");
                return;
            }

            int hastaId = Convert.ToInt32(cmbPatientId.SelectedValue);
            string diyet = (cbDietType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tümü";
            string egzersiz = (cbExerciseFreq.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tümü";

            AnalyzeDietExerciseEffect(hastaId, diyet, egzersiz);
        }

        // Model
        public class BloodSugarMeasurement
        {
            public TimeSpan Saat { get; set; }
            public double Seviye { get; set; }
            public string MeasurementTime { get; set; }
            public string Time => Saat.ToString(@"hh\:mm");
            public string Value => $"{Seviye} mg/dL";
            public string Status =>
                Seviye < 70 ? "Hipoglisemi" :
                Seviye > 200 ? "Hiperglisemi" :
                Seviye <= 110 ? "Normal" : "Prediyabet";
            public Brush StatusColor =>
                Seviye < 70 || Seviye > 200 ? Brushes.Red :
                Seviye <= 110 ? Brushes.Green : Brushes.Orange;
        }

       

        // Listeler ve değişkenler
        private List<BloodSugarMeasurement> dailyMeasurements = new List<BloodSugarMeasurement>();
        private List<Uyari> warningItems = new List<Uyari>();
        private System.Windows.Threading.DispatcherTimer dayEndTimer;
        private DateTime lastCheckDate = DateTime.Today;

        private void LoadWarningsFromDatabase()
        {
            try
            {
                warningItems.Clear();
                using (var connection = new SqlConnection(_connStr))
                {
                    connection.Open();

                    string query = @"SELECT uyari_id, hasta_id, tarih, saat, uyari_tipi, mesaj 
                           FROM Uyari 
                           WHERE tarih >= @today 
                           ORDER BY tarih DESC, saat DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@today", DateTime.Today.AddDays(-7));

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                warningItems.Add(new Uyari
                                {
                                    UyariId = Convert.ToInt32(reader["uyari_id"]),
                                    HastaId = Convert.ToInt32(reader["hasta_id"]),
                                    Tarih = Convert.ToDateTime(reader["tarih"]),
                                    Saat = TimeSpan.Parse(reader["saat"].ToString()),
                                    UyariTipi = reader["uyari_tipi"].ToString(),
                                    Mesaj = reader["mesaj"].ToString()
                                });
                            }
                        }
                    }
                }
                RefreshWarningsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uyarılar yüklenirken hata oluştu: {ex.Message}");
            }
        }

        private void InitializeDayEndTimer()
        {
            dayEndTimer = new System.Windows.Threading.DispatcherTimer();
            dayEndTimer.Interval = TimeSpan.FromMinutes(30);
            dayEndTimer.Tick += CheckEndOfDay;
            dayEndTimer.Start();
        }

        private void CheckEndOfDay(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Today;

            if (currentDate > lastCheckDate)
            {
                CheckPreviousDayMeasurements();
                lastCheckDate = currentDate;
                dailyMeasurements.Clear();
                RefreshMeasurementsList();
            }

            if (DateTime.Now.Hour >= 23)
            {
                CheckCurrentDayMeasurements();
            }
        }

        private void CheckPreviousDayMeasurements()
        {
            int patientId = GetCurrentPatientId();
            int countFromDb = GetMeasurementCountFromDatabase(patientId, DateTime.Today.AddDays(-1));

            if (countFromDb == 0)
            {
                CreateWarning(patientId, "Ölçüm Eksik Uyarısı",
                    "Hasta gün boyunca kan şekeri ölçümü yapmamıştır. Acil takip önerilir.");
            }
            else if (countFromDb < 3)
            {
                CreateWarning(patientId, "Ölçüm Yetersiz Uyarısı",
                    $"Hastanın günlük kan şekeri ölçüm sayısı yetersiz ({countFromDb} < 3). Durum izlenmelidir.");
            }
        }

        private void CheckCurrentDayMeasurements()
        {
            int patientId = GetCurrentPatientId();
            int countFromDb = GetMeasurementCountFromDatabase(patientId, DateTime.Today);

            bool hasWarningToday = warningItems.Any(w =>
                w.HastaId == patientId &&
                w.Tarih.Date == DateTime.Today &&
                (w.UyariTipi == "Ölçüm Eksik Uyarısı" || w.UyariTipi == "Ölçüm Yetersiz Uyarısı"));

            if (!hasWarningToday)
            {
                if (countFromDb == 0)
                {
                    CreateWarning(patientId, "Ölçüm Eksik Uyarısı",
                        "Hasta gün boyunca kan şekeri ölçümü yapmamıştır. Acil takip önerilir.");
                }
                else if (countFromDb < 3)
                {
                    CreateWarning(patientId, "Ölçüm Yetersiz Uyarısı",
                        $"Hastanın günlük kan şekeri ölçüm sayısı yetersiz ({countFromDb} < 3). Durum izlenmelidir.");
                }
            }
        }

        private int GetMeasurementCountFromDatabase(int patientId, DateTime date)
        {
            int count = 0;
            try
            {
                using (var connection = new SqlConnection(_connStr))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM KanSekeriOlcumu WHERE hasta_id = @hastaId AND CAST(tarih AS DATE) = @date";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@hastaId", patientId);
                        cmd.Parameters.AddWithValue("@date", date.Date);
                        count = (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanından ölçüm sayısı alınamadı: " + ex.Message);
            }
            return count;
        }

        private int GetCurrentPatientId()
        {
            try
            {
                if (cmbPatientId?.SelectedValue != null)
                    return Convert.ToInt32(cmbPatientId.SelectedValue);
            }
            catch { }
            return 1;
        }

        private void CreateWarning(int patientId, string warningType, string message)
        {
            var warning = new Uyari
            {
                HastaId = patientId,
                Tarih = DateTime.Today,
                Saat = DateTime.Now.TimeOfDay,
                UyariTipi = warningType,
                Mesaj = message
            };

            SaveWarningToDatabase(warning);
            warningItems.Add(warning);
            RefreshWarningsList();
        }

        private void SaveWarningToDatabase(Uyari warning)
        {
            try
            {
                using (var connection = new SqlConnection(_connStr))
                {
                    connection.Open();
                    string query = @"INSERT INTO Uyari (hasta_id, tarih, saat, uyari_tipi, mesaj) 
                           VALUES (@hasta_id, @tarih, @saat, @uyari_tipi, @mesaj)";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@hasta_id", warning.HastaId);
                        cmd.Parameters.AddWithValue("@tarih", warning.Tarih);
                        cmd.Parameters.AddWithValue("@saat", warning.Saat);
                        cmd.Parameters.AddWithValue("@uyari_tipi", warning.UyariTipi);
                        cmd.Parameters.AddWithValue("@mesaj", warning.Mesaj);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uyarı kaydedilemedi: " + ex.Message);
            }
        }


        //burada bir sorun var 
        private void SaveMeasurementToDatabase(int hastaId, BloodSugarMeasurement measurement)
        {
            try
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    conn.Open();

                    string getMaxIdQuery = "SELECT ISNULL(MAX(olcum_id), 0) FROM KanSekeriOlcumu";
                    SqlCommand getMaxIdCmd = new SqlCommand(getMaxIdQuery, conn);
                    int currentMaxId = (int)getMaxIdCmd.ExecuteScalar();


                    string query = @"INSERT INTO KanSekeriOlcumu (olcum_id,hasta_id, tarih, saat, seviye_mg_dl) 
                           VALUES (@currentMaxId,@hastaId, @tarih, @saat, @seviye)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@currentMaxId", currentMaxId);
                        cmd.Parameters.AddWithValue("@hastaId", hastaId);
                        cmd.Parameters.AddWithValue("@tarih", DateTime.Today);
                        cmd.Parameters.AddWithValue("@saat", measurement.Saat);
                        cmd.Parameters.AddWithValue("@seviye", measurement.Seviye);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ölçüm veritabanına kaydedilemedi: " + ex.Message);
            }
        }

        private void RefreshMeasurementsList()
        {
            lstDailyMeasurements.ItemsSource = null;
            lstDailyMeasurements.ItemsSource = dailyMeasurements;
        }

        private void RefreshWarningsList()
        {
            lstWarnings.ItemsSource = null;
            lstWarnings.ItemsSource = warningItems.OrderByDescending(w => w.Tarih).ThenByDescending(w => w.Saat);
        }

        private void BtnAddMeasurement_Click(object sender, RoutedEventArgs e)
        {
            if (dayEndTimer == null)
                InitializeDayEndTimer();

            if (warningItems.Count == 0)
                LoadWarningsFromDatabase();

            if (!double.TryParse(txtBloodSugarValues.Text, out double value))
            {
                MessageBox.Show("Geçerli bir şeker değeri giriniz.");
                return;
            }

            if (cmbMeasurementTime.SelectedItem == null)
            {
                MessageBox.Show("Ölçüm zamanı seçiniz.");
                return;
            }

            string measurementTime = (cmbMeasurementTime.SelectedItem as ComboBoxItem)?.Content.ToString();
            int patientId = GetCurrentPatientId();

            var measure = new BloodSugarMeasurement
            {
                Saat = DateTime.Now.TimeOfDay,
                Seviye = value,
                MeasurementTime = measurementTime
            };

            dailyMeasurements.Add(measure);

            // Veritabanına ölçümü kaydet
            SaveMeasurementToDatabase(patientId, measure);

            RefreshMeasurementsList();

            // Kritik seviye kontrolü
            CheckCriticalLevels(value, patientId, measurementTime);

            txtBloodSugarValues.Clear();
            cmbMeasurementTime.SelectedIndex = -1;
        }

        private void CheckCriticalLevels(double value, int patientId, string measurementTime)
        {
            string warningMessage = null;
            string uyariTipi = null;

            if (value < 70)
            {
                warningMessage = "Hastanın kan şekeri seviyesi 70 mg/dL'nin altına düştü. Hipoglisemi riski! Hızlı müdahale gerekebilir.";
                uyariTipi = "Acil Uyarı";
            }
            else if (value >= 70 && value <= 110)
            {
                // Normal seviye - hiçbir uyarı oluşturulmaz
                return;
            }
            else if (value >= 111 && value <= 150)
            {
                warningMessage = "Hastanın kan şekeri 111–150 mg/dL arasında. Durum izlenmeli.";
                uyariTipi = "Takip Uyarısı";
            }
            else if (value >= 151 && value <= 200)
            {
                warningMessage = "Hastanın kan şekeri 151–200 mg/dL arasında. Diyabet kontrolü gereklidir.";
                uyariTipi = "İzleme Uyarısı";
            }
            else if (value > 200)
            {
                warningMessage = "Hastanın kan şekeri 200 mg/dL'nin üzerinde. Hiperglisemi durumu. Acil müdahale gerekebilir.";
                uyariTipi = "Acil Müdahale Uyarısı";
            }

            if (warningMessage != null)
            {
                CreateWarning(patientId, uyariTipi, warningMessage);
            }
        }

        private void StopTimer()
        {
            dayEndTimer?.Stop();
        }

        private void BtnUpdateTreatment_Click(object sender, RoutedEventArgs e)
        {

        }


        private void BtnUpdateSymptoms_Click(object sender, RoutedEventArgs e)
        {

        }

      


        private void BtnShareWithPatient_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSaveTreatmentPlan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {

        }



        private void BtnViewHistory_Click(object sender, RoutedEventArgs e)
        {

        }


        private void LvPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

       
        private void cmbBelirtiTuru_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dgPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
