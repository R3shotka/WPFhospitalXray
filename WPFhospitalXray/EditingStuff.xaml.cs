
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
    /// Interaction logic for EditingStuff.xaml
    /// </summary>
    public partial class EditingStuff : Window
    {
        //private readonly MedStuffService _medStuffService;
        //private readonly MedStuffEditDto _medStuffToEdit;
        //private readonly Action _refreshCallback;
        //public EditingStuff(MedStuffService medStuffService, MedStuffEditDto medStuffToEdit, Action refreshCallback = null)
        //{
        //    InitializeComponent();
        //    _medStuffService = medStuffService;
        //    _medStuffToEdit = medStuffToEdit;
        //    _refreshCallback = refreshCallback;

        //    LoadMedStuffData();
        //}

        private void LoadMedStuffData()
        {
            //FullName_textbox.Text = _medStuffToEdit.FullName;
            //Login_textbox.Text = _medStuffToEdit.Login;
            //Pass_textbox.Text = _medStuffToEdit.Password;

            //foreach (ComboBoxItem item in Job_combobox.Items)
            //{
            //    if (item.Content.ToString() == GetPositionDisplayName(_medStuffToEdit.Position))
            //    {
            //        Job_combobox.SelectedItem = item;
            //        break;
            //    }
            //}

            //foreach (ComboBoxItem item in Sex_combobox.Items)
            //{
            //    if (item.Content.ToString() == GetSexDisplayName(_medStuffToEdit.Sex))
            //    {
            //        Sex_combobox.SelectedItem = item;
            //        break;
            //    }
            //}
        }
        //private string GetPositionDisplayName(PositionStuff position)
        //{
        //    return position switch
        //    {
        //        PositionStuff.Administrator => "Адміністратор",
        //        PositionStuff.Nurse => "Медсестра",
        //        PositionStuff.Orthopedist => "Ортопед",
        //        _ => position.ToString()
        //    };
        //}

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

        //private string GetSexDisplayName(string sex)
        //{
        //    return sex switch
        //    {
        //        "чоловіча" => "Чоловіча",
        //        "Чоловік" => "Чоловіча",
        //        "жіноча" => "Жіноча",
        //        "жінка" => "Жіноча",
        //        _ => sex.ToString()
        //    };
        //}
        //private string GetSexFromDisplayName(string displayName)
        //{
        //    return displayName switch
        //    {
        //        "Чоловіча" => "Чоловіча",
        //        "Жіноча" => "Жіноча",
        //        _ => displayName
        //    };
        //}

        // Кнопка "Зберегти"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    // Оновлюємо дані з форми
            //    _medStuffToEdit.FullName = FullName_textbox.Text;
            //    _medStuffToEdit.Login = Login_textbox.Text;

            //    if (!string.IsNullOrEmpty(Pass_textbox.Text))
            //    {
            //        _medStuffToEdit.Password = Pass_textbox.Text;
            //    }

            //    // Оновлюємо посаду
            //    if (Job_combobox.SelectedItem is ComboBoxItem selectedJob)
            //    {
            //        _medStuffToEdit.Position = GetPositionFromDisplayName(selectedJob.Content.ToString());
            //    }

            //    // Оновлюємо стать
            //    if (Sex_combobox.SelectedItem is ComboBoxItem selectedSex)
            //    {
            //        _medStuffToEdit.Sex = GetSexFromDisplayName(selectedSex.Content.ToString());
            //    }

            //    // Зберігаємо зміни
            //    _medStuffService.UpdateMedStuff(_medStuffToEdit);

            //    MessageBox.Show("Дані успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            //    // Оновлюємо список у головному вікні
            //    _refreshCallback?.Invoke();

            //    this.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        // Кнопка "Вийти"
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}

