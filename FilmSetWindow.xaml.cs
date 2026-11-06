using System;
using CinemaStudio.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace CinemaStudio
{
    /// <summary>
    /// Логика взаимодействия для ProjectStagesWindow.xaml
    /// </summary>
    public partial class FilmSetWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<FilmSet> _filmSets;
        private FilmSet _selectedFilmSet = new FilmSet();
        private FilmSet _editableSet = new FilmSet() { Address = new()};
        
        public FilmSet EditableSet
        {
            get => _editableSet;
            set
            {
                _editableSet = value;

                OnPropertyChanged();
            }
        }
        public FilmSet SelectedFilmSet
        {
            get => _selectedFilmSet;
            set
            {
                if (value != null)
                {
                    EditableSet = new FilmSet
                    {
                        Id = value.Id,
                        DayRent = value.DayRent,
                        Capacity = value.Capacity,
                        FilmSetType = value.FilmSetType,
                        FilmSetTypeId = value.FilmSetTypeId,
                        Address = value.Address
                    };

                    _selectedFilmSet = value;
                }
                else
                {
                    EditableSet = new FilmSet();
                }

                OnPropertyChanged();
            }
        }
        public ObservableCollection<FilmSet> FilmSets
        {
            get => _filmSets;
            set
            {
                _filmSets = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public FilmSetWindow()
        {
            InitializeComponent();

            OnStart();


            DataContext = this;
            
            #if DEBUG
                EditableSet.FilmSetTypeId = 0;
                EditableSet.DayRent = 100;
                EditableSet.Capacity = 10;
                //EditableSet.Address.Street.CityId = 0;
                EditableSet.Address.StreetId = 0;
                EditableSet.Address.Building = "10";
            #endif
        }

        private void OnStart()
        {
            FilmSets = GetFilmSets();

            using (var db = new CinemaDbContext())
            {
                SetTypeCombo.ItemsSource = db.FilmSetTypes
                    .AsNoTracking()
                    .ToList();

                CityCombo.ItemsSource = db.Cities
                    .AsNoTracking()
                    .ToList();
            }
        }
        private ObservableCollection<FilmSet> GetFilmSets()
        {
            using (var db = new CinemaDbContext())
            {
                return new ObservableCollection<FilmSet>(db.FilmSets
                    .AsNoTracking()
                    .Include(f => f.Address)
                    .Include(f => f.FilmSetType)
                    .Include(f => f.Address.Street)
                    .Include(f => f.Address.Street.City)
                    .ToList());
            }
        }

        private void CityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StreetCombo.ItemsSource = null;
            StreetCombo.SelectedIndex = -1;

            var city = CityCombo.SelectedItem as City;

            if (city != null)
            {
                using (var db = new CinemaDbContext())
                {
                    StreetCombo.ItemsSource = db.Streets
                        .AsNoTracking()
                        .Where(s => s.CityId == city.Id)
                        .ToList();
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int capacity;
            double rent;
            string home = BuildingField.Text.Trim();
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(home))
            {
                errors.Add("Заполните поле 'Дом'");
            }

            if (!double.TryParse(RentField.Text.Trim(), out rent) && rent > 0)
            {
                errors.Add("Аренда должна быть числом больше нуля");
            }

            if (!int.TryParse(CapacityField.Text.Trim(), out capacity) && capacity >= 0)
            {
                errors.Add("Вместимость должна быть числом больше нуля");
            }

            FilmSetType type = SetTypeCombo.SelectedItem as FilmSetType;
            City city = CityCombo.SelectedItem as City;
            Street street = StreetCombo.SelectedItem as Street;

            if (type == null)
            {
                errors.Add("Выберите тип");
            }
            if (city == null)
            {
                errors.Add("Выберите город");
            }
            if (street == null)
            {
                errors.Add("Выберите улицу");
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
                    EditableSet.Address.Street = null;
                    EditableSet.FilmSetType = null;

                    EditableSet.Address.StreetId = (int)StreetCombo.SelectedValue;
                    EditableSet.FilmSetTypeId = (int)SetTypeCombo.SelectedValue;

                    if (EditableSet.Id != 0)
                    {
                        var filmSetFromDb = db.FilmSets
                            .Include(f => f.Address)
                            .FirstOrDefault(f => f.Id == EditableSet.Id);

                        if (filmSetFromDb != null)
                        {
                            filmSetFromDb.DayRent = rent;
                            filmSetFromDb.Address.Building = home;
                            filmSetFromDb.Capacity = capacity;

                            if (filmSetFromDb.Address != null)
                            {
                                filmSetFromDb.Address.StreetId = (int)StreetCombo.SelectedValue;
                                filmSetFromDb.FilmSetTypeId = (int)SetTypeCombo.SelectedValue;
                            }
                        }
                    }
                    else
                    {
                        db.FilmSets.Add(EditableSet);
                    }

                    db.SaveChanges();

                    MessageBox.Show("Данные сохранены");

                    FilmSets = GetFilmSets();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в бд: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            EditableSet = new FilmSet();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
