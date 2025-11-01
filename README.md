
# Diabetes Tracking System

A comprehensive WPF desktop application for diabetes management and patient monitoring, built with C# and .NET Framework 4.7.2.

## 🏥 Overview

The Diabetes Tracking System is a medical application designed to help doctors monitor their patients' diabetes conditions and enable patients to track their blood sugar levels, diet, exercise, and insulin intake. The system provides real-time data visualization, alerts, and personalized recommendations.

## ✨ Features

### For Doctors (DoctorPanel)
- **Patient Management**: View and manage patient profiles with detailed information
- **Blood Sugar Monitoring**: Track patient glucose levels with visual charts and statistics
- **Alert System**: Receive automated warnings for critical blood sugar levels
- **Treatment Planning**: Assign diet and exercise plans to patients
- **Insulin Recommendations**: Generate personalized insulin dosage recommendations
- **Symptom Tracking**: Monitor patient-reported symptoms over time
- **Compliance Monitoring**: Track patient adherence to diet and exercise plans

### For Patients (HastaPanel)
- **Blood Sugar Logging**: Quick data entry for glucose measurements
- **Visual Analytics**: Interactive charts showing blood sugar trends
- **Diet & Exercise Tracking**: Log daily compliance with prescribed plans
- **Insulin History**: View historical insulin recommendations
- **Profile Management**: Personal health information and statistics
- **Calendar Views**: Visual representation of diet and exercise adherence

## 🛠️ Technology Stack

- **Framework**: .NET Framework 4.7.2
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Language**: C# 7.3
- **Database**: SQL Server (LocalDB)
- **Data Visualization**: LiveCharts 0.9.7
- **Architecture**: MVVM-inspired with code-behind

## 📋 Prerequisites

- Windows 10 or later
- .NET Framework 4.7.2 or higher
- SQL Server 2016 or later / SQL Server LocalDB
- Visual Studio 2019 or later (for development)

## 🚀 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/diabetes_tracking_system.git
   cd diabetes_tracking_system
   ```

2. **Database Setup**
   - Open SQL Server Management Studio
   - Execute the database creation script (included in `/Database` folder)
   - Update connection string in the code if needed:
     ```csharp
     string connectionString = "Server=localhost;Database=Diabetes_System;Trusted_Connection=True;";
     ```

3. **Restore NuGet Packages**
   ```bash
   nuget restore
   ```

4. **Build the Solution**
   - Open `Diabetes_Tracking_System_new.sln` in Visual Studio
   - Build the solution (Ctrl+Shift+B)

5. **Run the Application**
   - Press F5 or click Start

## 📊 Database Schema

### Core Tables
- **Kullanici**: User accounts (doctors and patients)
- **Hasta**: Patient-specific information
- **KanSekeriOlcumu**: Blood sugar measurements
- **DiyetTakibi**: Diet compliance tracking
- **EgzersizTakibi**: Exercise compliance tracking
- **InsulinOnerisi**: Insulin dosage recommendations
- **Belirti**: Patient symptoms
- **Uyari**: System alerts and warnings

## 🔐 Security Features

- **Password Hashing**: SHA-256 encryption for user passwords
- **Secure Login**: Role-based authentication (Doctor/Patient)
- **Timing Attack Prevention**: Constant-time password comparison
- **Session Management**: User-specific data access control

## 💡 Usage

### Login
1. Select user type (Doctor/Patient)
2. Enter TC Kimlik (Turkish ID number)
3. Enter password
4. Click "GİRİŞ YAP" (Login)

### Doctor Workflow
1. View patient list with health status indicators
2. Select a patient to view detailed information
3. Monitor blood sugar trends and generate reports
4. Create diet and exercise plans
5. Generate insulin recommendations
6. Review alerts and symptoms

### Patient Workflow
1. Log daily blood sugar measurements
2. Mark diet and exercise compliance
3. View personal health statistics
4. Check insulin recommendations
5. Monitor progress through charts

## 📱 User Interface

### Main Components
- **MainWindow**: Login screen with modern UI
- **DoctorPanel**: Comprehensive doctor dashboard
- **HastaPanel**: Patient management interface

### Key Features
- Responsive design with custom styles
- Real-time data visualization using LiveCharts
- Calendar-based tracking views
- Color-coded health status indicators
- Drag-and-drop window movement
- Professional medical theme

## 🔧 Configuration

### Connection String
Update in each `.xaml.cs` file:
```csharp
private string _connStr = "Server=localhost;Database=Diabetes_System;Trusted_Connection=True;";
```

### Blood Sugar Thresholds
Defined in `KanSekeriOlcumu.cs`:
- Low: < 70 mg/dL
- Normal: 70-110 mg/dL
- Medium High: 111-150 mg/dL
- High: 151-200 mg/dL
- Very High: > 200 mg/dL

## 📦 Dependencies

```xml
<package id="LiveCharts" version="0.9.7" targetFramework="net472" />
<package id="LiveCharts.Wpf" version="0.9.7" targetFramework="net472" />
```

## 🏗️ Project Structure

```
Diabetes_Tracking_System_new/
├── Models/
│   ├── Kullanici.cs
│   ├── Hasta.cs
│   ├── KanSekeriOlcumu.cs
│   ├── DiyetTakibi.cs
│   ├── EgzersizTakibi.cs
│   ├── InsulinOnerisi.cs
│   ├── Belirti.cs
│   └── Uyari.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── DoctorPanel.xaml
│   └── HastaPanel.xaml
├── Resources/
│   └── diabetes.png
└── App.xaml
```

## 🐛 Known Issues

- Profile images must be stored as VARBINARY in database
- Date filtering requires both start and end dates
- Some UI elements may not scale properly on high-DPI displays

## 🔮 Future Enhancements

- [ ] Mobile companion app
- [ ] Cloud synchronization
- [ ] Multi-language support
- [ ] Export reports to PDF
- [ ] Medication reminders
- [ ] Integration with glucose meters
- [ ] Machine learning predictions
- [ ] Telemedicine features

## Sample Images 

### --------------- For Doctor -----------------

### Login Screen 
<img width="1918" height="1044" alt="Ekran görüntüsü 2025-05-25 003951" src="https://github.com/user-attachments/assets/033a688e-153c-4dae-bdad-244f0a8d30a6" />

### Doctor Panel
<img width="1920" height="1080" alt="Ekran Görüntüsü (181)" src="https://github.com/user-attachments/assets/fbb507d2-9f1c-4e89-a470-0e67d7f8b40a" />

### patient follow-up
<img width="1727" height="1033" alt="Ekran görüntüsü 2025-05-25 004129" src="https://github.com/user-attachments/assets/2e8d17dc-7436-4581-bf2e-326d2dfe5953" />

### Blood Sugar tracking
<img width="955" height="691" alt="Ekran görüntüsü 2025-05-25 004203" src="https://github.com/user-attachments/assets/acab6d3e-e99c-46b1-8cfa-18a73c053f5a" />

### Exercise diet tracking
<img width="1013" height="797" alt="Ekran görüntüsü 2025-05-25 004223" src="https://github.com/user-attachments/assets/89814018-2fc1-4e73-987a-3931c61f10e7" />

### Filter by Blood and Diseases
<img width="1318" height="816" alt="Ekran görüntüsü 2025-05-25 004239" src="https://github.com/user-attachments/assets/ae7c3b87-f82c-4816-99cb-f696165538a1" />

### Laboratory results
<img width="1029" height="728" alt="Ekran görüntüsü 2025-05-25 004258" src="https://github.com/user-attachments/assets/129d9420-b626-4dc7-92f3-a1ab7b3ce526" />

<img width="1043" height="744" alt="Ekran görüntüsü 2025-05-25 004321" src="https://github.com/user-attachments/assets/4809a965-d370-47d4-80fc-8c045379d4d6" />

### Blood sugar alert tracking
<img width="1305" height="818" alt="Ekran görüntüsü 2025-05-25 004505" src="https://github.com/user-attachments/assets/3319f1be-023c-43ef-ac59-243840f5a14b" />

### --------------- For Patient -----------------

### Patient Panel
<img width="1918" height="1048" alt="Ekran görüntüsü 2025-05-25 004731" src="https://github.com/user-attachments/assets/1e0c4f66-c11b-4f49-b8de-fe2da0745b18" />

### Blood Sugar Tracking
<img width="1584" height="615" alt="Ekran görüntüsü 2025-05-25 004816" src="https://github.com/user-attachments/assets/1c96c64b-258a-42da-9390-e855e149c73f" />

### Diet - Exercise
<img width="1920" height="1080" alt="Ekran Görüntüsü (182)" src="https://github.com/user-attachments/assets/23d796b7-413b-4881-a27a-ef901956245d" />

### Insulin Tracking 
<img width="1585" height="611" alt="Ekran görüntüsü 2025-05-25 004909" src="https://github.com/user-attachments/assets/f8ac1436-0cea-438e-9e56-fb699b6efb0f" />


## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- **Kocaeli University** - Computer Engineering Department
   Merve Sevim 

## 🙏 Acknowledgments

- LiveCharts library for data visualization
- Microsoft for WPF framework
- Medical professionals for domain expertise

## 📞 Support

For support, email ankmrv044@gmail.com or open an issue in the repository.

## ⚠️ Disclaimer

This system is designed for educational and tracking purposes. It should not replace professional medical advice, diagnosis, or treatment. Always consult with qualified healthcare providers regarding diabetes management.

---

**© 2025 Kocaeli Üniversitesi Bilgisayar Mühendisliği**
