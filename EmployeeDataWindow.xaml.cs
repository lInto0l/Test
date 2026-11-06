using CinemaStudio.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CinemaStudio
{
    /// <summary>
    /// Логика взаимодействия для EmployeeAddWindow.xaml
    /// </summary>
    public partial class EmployeeDataWindow : Window, INotifyPropertyChanged
    {
        private string _emailPattern = @"^[a-zA-Z0-9!@#$%^&*()_+=-]{2,}@[a-zA-Z0-9!@#$%^&*()_+=-]{2,}\.[a-zA-Z0-9!@#$%^&*()_+=-]{2,}$";
        private string _phonePattern = @"^(7|8)\d{10}$";
        private string _loginPattern = @"^[a-zA-Z0-9!@#$%^&*()_+=-]{2,}$";
        private string _passPattern = @"^[a-zA-Z0-9!@#$%^&*()_+=-]{4,}$";
        private string _rawPhone = @"[()\s-+]";
        private string _avatar;
        private Employee? _employee;

        public event PropertyChangedEventHandler? PropertyChanged;
        public string Avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                OnPropertyChanged();
            }
        }

        public EmployeeDataWindow(int? id = null)
        {
            InitializeComponent();

            using (var db = new CinemaDbContext())
            {
                _employee = db.Employees.FirstOrDefault(e => e.Id == id);
                SpecialityCombo.ItemsSource = db.Specialities.ToList(); 
            }
            
            if (_employee != null)
            {
                SubstituteFields();
            }

            //#if DEBUG
            //NameField.Text = "fsdf";
            //SurnameField.Text = "sdfsd";
            //LoginField.Text = "login";
            //PasswordField.Text = "password";
            //EmailField.Text = "sdfs@dsf.sdfs";
            //PhoneField.Text = "72345678901";
            //BidField.Text = "100";
            //Avatar = "admin.jpg";
            //SpecialityCombo.SelectedIndex = 0;

            //#endif

            DataContext = this;

        }

        private void SubstituteFields()
        {
            NameField.Text = _employee?.Name;
            SurnameField.Text = _employee?.Surname;
            LoginField.Text = _employee?.Login;
            EmailField.Text = _employee?.Email;
            PhoneField.Text = _employee?.Phone;
            Avatar = _employee?.Avatar;
            BidField.Text = _employee?.Bid.ToString();
            SpecialityCombo.SelectedItem = _employee?.Speciality;
        }
        
        private void ClearForm()
        {
            NameField.Text = null;
            SurnameField.Text = null;
            LoginField.Text = null;
            PasswordField.Text = null;
            EmailField.Text = null;
            PhoneField.Text = null;
            BidField.Text = null;
            SpecialityCombo.SelectedIndex = -1;
            Avatar = "";

            _employee = null;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = NameField.Text.Trim();
            string surname = SurnameField.Text.Trim();
            string login = LoginField.Text.Trim();
            string pass = PasswordField.Text.Trim();
            string email = EmailField.Text.Trim();
            string phone = PhoneField.Text.Trim();
            Speciality speciality = (Speciality)SpecialityCombo.SelectedItem;
            double bid;
            bool isLoginBusy = false;

            using (var db = new CinemaDbContext())
            {
                isLoginBusy = db.Employees.Any(e => e.Login == login);
            }

            List<string> warnings = new List<string>();


            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
            {
                warnings.Add("Заполните имя и фамилию");
            }

            if (!double.TryParse(BidField.Text.Trim(), out bid) && bid > 0)
            {
                warnings.Add("Ставка должна быть числом больше нуля");
            }

            if (!Regex.IsMatch(email, _emailPattern))
            {
                warnings.Add("Почта не соответствует формату (name@domain.domain2)");
            }

            phone = Regex.Replace(phone, _rawPhone, "");

            if (!Regex.IsMatch(phone, _phonePattern))
            {
                warnings.Add("Телефон не соответствует формату (+7xxx xxx xx xx)");
            }

            if (!Regex.IsMatch(login, _loginPattern))
            {
                warnings.Add("Логин должен состоять из латиницы и чисел, мин длина 2");
            }

            if (isLoginBusy && _employee == null)
            {
                warnings.Add("Логин занят");
            }

            if (_employee != null && string.IsNullOrWhiteSpace(pass))
            {
                pass = _employee.Password;
            }
            else if (!Regex.IsMatch(pass, _passPattern))
            {
                warnings.Add("Пароль должен состоять из латиницы и чисел, мин длина 4");
            }

            if (speciality == null)
            {
                warnings.Add("Выберите специальность");
            }

            if (warnings.Count > 0)
            {
                MessageBox.Show("Ошибки:\n\n" + string.Join("\n", warnings));
                return;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(pass);

            try
            {
                using (var db = new CinemaDbContext())
                {
                    if (_employee != null)
                    {
                        _employee.Name = name;
                        _employee.Surname = surname;
                        _employee.Login = login;
                        _employee.Password = passwordHash;
                        _employee.Bid = bid;
                        _employee.SpecialityId = speciality.Id;
                        _employee.Email = email;
                        _employee.Phone = phone;
                        _employee.Avatar = Avatar;

                        db.Employees.Update(_employee);
                    }
                    else
                    {
                        Employee newEmployee = new Employee()
                        {
                            Name = name,
                            Surname = surname,
                            Login = login,
                            Password = passwordHash,
                            Bid = bid,
                            SpecialityId = speciality.Id,
                            Email = email,
                            Phone = phone,
                            Avatar = Avatar
                        };

                        db.Employees.Add(newEmployee);
                    }

                   db.SaveChanges();
                        
                    MessageBox.Show("Данные успешно сохранены");

                    ClearForm();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в бд: {ex.InnerException?.Message}");
            }

        }

        private void OpenDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string sourceFilePath = openFileDialog.FileName;

                string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                if (!System.IO.Directory.Exists(targetFolder))
                {
                    System.IO.Directory.CreateDirectory(targetFolder);
                }

                string destinationPath = System.IO.Path.Combine(targetFolder, Avatar);


                try
                {
                    System.IO.File.Copy(sourceFilePath, destinationPath, overwrite: true);
                    Avatar = openFileDialog.SafeFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}");
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
