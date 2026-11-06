using CinemaStudio.Models;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CinemaStudio
{
    public partial class MainWindow : Window
    {
        public Employee User { get; set; }

        public MainWindow(int employeeId)
        {
            InitializeComponent();

            using (var db = new CinemaDbContext())
            {
                User = db.Employees.FirstOrDefault(e => e.Id == employeeId)!;
            }

            User.Avatar = string.IsNullOrWhiteSpace(User.Avatar) ? "admin.jpg" : User.Avatar;

            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", User.Avatar);

            if (File.Exists(filePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                UserAvatar.Source = bitmap;
            }

            DataContext = this;
        }

        private void ShowDialogWindow(Window window)
        {
            Hide();

            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            Show();
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();
            Close();
        }

        private void EmployeeBtn_Click(object sender, RoutedEventArgs e)
        {
            EmployeesWindow employeesWindow = new EmployeesWindow();
            ShowDialogWindow(employeesWindow);
        }

        private void ProjectCostBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new ProjectCostWindow());
        }

        private void RentalBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new RentalWindow());
        }

        private void ShootingScheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new ShootingScheduleWindow());
        }

        private void FilmCrewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new FilmCrewWindow());
        }

        private void FilmSetBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new FilmSetWindow());
        }

        private void ProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogWindow(new ProjectsWindow());
        }
    }
}