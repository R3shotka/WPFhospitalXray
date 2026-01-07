
using DAL.DBContext;
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
    /// Interaction logic for CreateMed.xaml
    /// </summary>
    public partial class CreateMed : Window
    {
        //private readonly MedStuffService _medStuffService;
        //private readonly CreateMedStuffRequestDto createMedStuffRequestDto;
        public CreateMed()
        {
            InitializeComponent();

            //var context = new ApplicationDBContext();
            //_medStuffService = new MedStuffService(context);

        }
        private void Save_btn(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var medStuffcreate = new CreateMedStuffRequestDto
            //    {
            //        FullName = FullName_textbox.Text,
            //        Position = GetPositionFromDisplayName(Job_combobox.Text),
            //        Sex = GetSexFromDisplayName(Sex_combobox.Text),
            //        Login = Login_textbox.Text,
            //        Password = Pass_textbox.Text
            //    };

            //    _medStuffService.CreateMedStuff(medStuffcreate);

            //    MessageBox.Show("Працівника успішно створено!", "Успіх",
            //           MessageBoxButton.OK, MessageBoxImage.Information);

            //    this.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Помилка при створенні: {ex.Message}", "Помилка",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //}

        }
        public void Exit_btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //private PositionStuff GetPositionFromDisplayName(string displayName)
        //{
        //    return displayName switch
        //    {
        //        "Адміністратор" => PositionStuff.Administrator,
        //        "Медсестра" => PositionStuff.Nurse,
        //        "Ортопед" => PositionStuff.Orthopedist,
        //        _ => PositionStuff.Administrator
        //    };
        //}

        //private string GetSexFromDisplayName(string displayName)
        //{
        //    return displayName switch
        //    {
        //        "Чоловіча" => "Чоловік",
        //        "Жіноча" => "Жінка",
        //        _ => displayName
        //    };
        //}

    }
}
