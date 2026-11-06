using CinemaStudio.Models;
using System;
using System.Collections.Generic;
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
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();

            #if DEBUG
            LoginField.Text = "admin";
            PasswordField.Password = "admin";

            ApplyBtn_Click(this, new RoutedEventArgs());
            #endif
        }

        private Employee GetAuthEmployee(string login, string password)
        {
            using (var db = new CinemaDbContext())
            {
                var user = db.Employees.FirstOrDefault(e => e.Login == login)!;

                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }
            }
            return null;
        }

        private void GoToMainWindow(int userId)
        {
            MainWindow mainWindow = new MainWindow(userId);
            mainWindow.Show();
            Close();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginField.Text = LoginField.Text.Trim();
            string password = PasswordField.Password.Trim();

            Employee user = GetAuthEmployee(login, password);

            if (user != null)
            {
                GoToMainWindow(user.Id);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
    }
}
