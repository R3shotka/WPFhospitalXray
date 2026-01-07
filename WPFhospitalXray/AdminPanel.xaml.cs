
using DAL.DBContext;
using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
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


namespace WPFhospitalXray
{
    /// <summary>
    /// Interaction logic for AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : Window
    {
        //public List<PatientDto> Patients = new List<PatientDto>();
        //private readonly PatientService _patientService;


        public AdminPanel()
        {
            InitializeComponent();

            // Створення DbContext та сервісу
            //var context = new ApplicationDBContext();// або через DI, якщо налаштовано
            //_patientService = new PatientService(context);

            //// Отримуємо пацієнтів з бази
            //Patients = _patientService.GetAll();

            //// Привʼязуємо до DataGrid
            //DBGrid.ItemsSource = Patients;



        }

        private void CreateStuff_btn(object sender, RoutedEventArgs e)
        {
            //var createWindow = new CreateMed();
            //createWindow.ShowDialog();
        }
        private void EditStuff_btn(object sender, RoutedEventArgs e)
        {
            //var editWindow = new EditMed();
            //editWindow.ShowDialog();
        }
    }
}
