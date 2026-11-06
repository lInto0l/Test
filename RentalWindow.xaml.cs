using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
using CinemaStudio.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaStudio
{
    /// <summary>
    /// Логика взаимодействия для RentalWindow.xaml
    /// </summary>
    public partial class RentalWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Rental> _rentals;
        private Rental _editableRental = new Rental() { Project = new Project()};

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime MinDate { get; set; } = DateTime.Today.AddYears(-100);
        public DateTime MaxDate { get; set; } = DateTime.Today;
        public ObservableCollection<Rental> Rentals
        {
            get => _rentals;
            set
            {
                _rentals = value;
                OnPropertyChanged();
            }
        }
        public Rental EditableRental
        {
            get => _editableRental;
            set
            {
                _editableRental = value;
                _editableRental.Project = new Project()
                {
                    Id = value.ProjectId,
                    Name = value.Project.Name
                };
                SelectedDate = value.Date?.ToDateTime(TimeOnly.MinValue);

                OnPropertyChanged();
            }
        }
        public DateTime? SelectedDate
        {
            get => EditableRental.Date?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EditableRental.Date = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
                OnPropertyChanged();
            }
        }

        public RentalWindow()
        {
            InitializeComponent();

            OnStart();

            DataContext = this;
        }

        public void OnStart()
        {
            using (var db = new CinemaDbContext())
            {
                Rentals = new ObservableCollection<Rental>(GetRentals());

                ProjectCombo.ItemsSource = db.Projects.AsNoTracking().ToList();

            }
        }

        private List<Rental> GetRentals()
        {
            using (var db = new CinemaDbContext())
            {
                return db.Rentals
                    .AsNoTracking()
                    .Include(r => r.Project)
                    .ToList();
            }
        }
        public void WipeForm()
        {
            RentalsTable.SelectedIndex = -1;
            EditableRental = new Rental() { Project = new Project() };
            Rentals = new ObservableCollection<Rental>(GetRentals());
        }

        private void RentalsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRental = (RentalsTable.SelectedItem as Rental);

            if (selectedRental != null)
            {
                EditableRental = selectedRental;
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            WipeForm();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> errors = new List<string>();

            if (EditableRental == null)
            {
                MessageBox.Show("Выберите прокат");
                return;
            }

            if (EditableRental.ProjectId == 0)
            {
                errors.Add("Выберите проект");
            }
            if (EditableRental.Date == null
                || EditableRental.Date > DateOnly.FromDateTime(MaxDate)
                || EditableRental.Date < DateOnly.FromDateTime(MinDate))
            {
                errors.Add("Выберите корректную дату");
            }
            if (EditableRental.CinemaQuantity <= 0)
            {
                errors.Add("Кол-во кинотеатров должно быть больше 0");
            }
            if (EditableRental.Rating < 0 || EditableRental.Rating > 10)
            {
                errors.Add("Рейтинг должен быть от 0-10");
            }
            if (EditableRental.Charge <= 0)
            {
                errors.Add("Выручка должна быть больше 0");
            }


            if (errors.Count > 0)
            {
                string message = string.Join("\n", errors);
                MessageBox.Show(message);
                return;
            }

            try
            {
                using (var db = new CinemaDbContext())
                {
                    if (EditableRental.Id == 0)
                    {
                        EditableRental.Project = null;
                        db.Rentals.Add(EditableRental);
                    }
                    else
                    {
                        var rentalFromDb = db.Rentals.First(r => r.Id == EditableRental.Id);

                        rentalFromDb.ProjectId = EditableRental.ProjectId;
                        rentalFromDb.Charge = EditableRental.Charge;
                        rentalFromDb.Rating = EditableRental.Rating;
                        rentalFromDb.Date = EditableRental.Date;
                        rentalFromDb.CinemaQuantity = EditableRental.CinemaQuantity;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные сохранены");

                    WipeForm();
                    Rentals = new ObservableCollection<Rental>(GetRentals());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в бд: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
