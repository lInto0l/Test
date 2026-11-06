using CinemaStudio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
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
    /// Логика взаимодействия для EmployeeWindow.xaml
    /// </summary>
    public partial class EmployeesWindow : Window, INotifyPropertyChanged
    {
        int? _employeeId;

        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<Employee> employees;

        public ObservableCollection<Employee> Employees
        {
            get
            {
                return employees;
            }
            set
            {
                if (employees != value)
                {
                    employees = value;
                    OnPropertyChanged(nameof(Employees));
                }
            }
        }

        public EmployeesWindow()
        {
            InitializeComponent();

            UpdateTable();

            DataContext = this;
        }

        private void UpdateTable()
        {
            using (var db = new CinemaDbContext())
            {
                Employees = new ObservableCollection<Employee>(db.Employees
                    .AsNoTracking()
                    .Include(e => e.Speciality)
                    .ToList());
            }
        }

        private void ShowEmployeeDataDialog(int? id = null)
        {
            EmployeeDataWindow empDataWindow = new EmployeeDataWindow(id);
            empDataWindow.Owner = this;
            empDataWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            empDataWindow.ShowDialog();
        }

        private void FindEmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            string surname = EmployeeToFind.Text.Trim();

            if (!string.IsNullOrEmpty(surname))
            {
                using (var db = new CinemaDbContext())
                {

                    Employees = new ObservableCollection<Employee>(db.Employees
                        .AsNoTracking()
                        .Include(e => e.Speciality)
                        .Where(e => e.Surname.ToLower().Contains(surname))
                        .ToList());
                }
            }
            else
            {
                UpdateTable();
            }
        }
        private void EditEmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_employeeId != null)
            {
                ShowEmployeeDataDialog(_employeeId);

                UpdateTable();
            }
        }


        private void AddEmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowEmployeeDataDialog();

            UpdateTable();
        }

        private void DeleteEmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_employeeId == null)
            {
                MessageBox.Show("Выберите сотрудника");
                return;
            }

            try
            {
                using (var db = new CinemaDbContext())
                {
                    db.Employees.Remove((Employee)EmployeesTable.SelectedItem);

                    db.SaveChanges();
                }

                employees.Remove((Employee)EmployeesTable.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении из бд: {ex?.InnerException.Message}");
            }
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _employeeId = ((Employee)EmployeesTable.SelectedItem)?.Id;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName && !string.IsNullOrEmpty(fileName))
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
