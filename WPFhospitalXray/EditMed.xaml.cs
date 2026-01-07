
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
    /// Interaction logic for EditMed.xaml
    /// </summary>
    public partial class EditMed : Window
    {
        //public List<MedStuffListDto> MedStuffs = new List<MedStuffListDto>();
        //private readonly MedStuffService _medStuffService;
        //private MedStuffEditDto _selectedMedStuff;
        public EditMed()
        {
            InitializeComponent();

            //var context = new ApplicationDBContext();// або через DI, якщо налаштовано
            //_medStuffService = new MedStuffService(context);



            LoadMedStuffs();


        }
        private void LoadMedStuffs()
        {
            //MedStuffs = _medStuffService.GetAllForList();
            //DBStuff.ItemsSource = MedStuffs;
        }
        private void EditStuff_btn(object sender, RoutedEventArgs e)
        {
            //if (DBStuff.SelectedItem is MedStuffListDto selected)
            //{
            //    // Тепер selected.Id буде доступний
            //    var medStuffToEdit = _medStuffService.GetMedStuffForEdit(selected.Id);

            //    if (medStuffToEdit != null)
            //    {
            //        var editWindow = new EditingStuff(_medStuffService, medStuffToEdit, LoadMedStuffs);
            //        editWindow.Owner = this;
            //        editWindow.ShowDialog();
            //    }
            //    else
            //    {
            //        MessageBox.Show("Оберіть працівника для редагування", "Увага",
            //            MessageBoxButton.OK, MessageBoxImage.Warning);
            //    }
            //}
        }

        private void DeleteStuff_btn(object sender, RoutedEventArgs e)
        {
            //if (DBStuff.SelectedItem is MedStuffListDto selected)
            //{
            //    // Додай підтвердження видалення
            //    var result = MessageBox.Show($"Ви впевнені, що хочете видалити {selected.FullName}?",
            //                               "Підтвердження видалення",
            //                               MessageBoxButton.YesNo,
            //                               MessageBoxImage.Warning);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        try
            //        {
            //            _medStuffService.DeleteMedStuff(selected.Id); // ← Передаємо ID!
            //            LoadMedStuffs(); // ← Ось це оновлює список на екрані!

            //            MessageBox.Show("Працівника успішно видалено!", "Успіх",
            //                           MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show($"Помилка при видаленні: {ex.Message}", "Помилка",
            //                           MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Оберіть працівника для видалення", "Увага",
            //                   MessageBoxButton.OK, MessageBoxImage.Warning);
            //}

        }
    }
}
