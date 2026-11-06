using System;
using CinemaStudio.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.DirectoryServices;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CinemaStudio
{
    /// <summary>
    /// Логика взаимодействия для FilmCrewWindow.xaml
    /// </summary>
    public partial class FilmCrewWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<EmployeeInFilmCrew> _crewMembers;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Speciality> _specialities;
        private ObservableCollection<Project> _projects;
        private EmployeeInFilmCrew _selectedCrew;
        private EmployeeInFilmCrew _editableCrew = new EmployeeInFilmCrew() 
        { FilmCrew = new FilmCrew() };
             
        public ObservableCollection<EmployeeInFilmCrew> CrewMembers
        {
            get => _crewMembers;
            set
            {
                _crewMembers = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Speciality> Specialities
        {
            get => _specialities;
            set
            {
                _specialities = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }
        public EmployeeInFilmCrew SelectedCrew
        {
            get => _selectedCrew;
            set
            {
                _selectedCrew = value;
                
                if (value != null)
                {
                    EditableCrew = new EmployeeInFilmCrew()
                    {
                        Id = _selectedCrew.Id,
                        FilmCrew = _selectedCrew.FilmCrew,
                        EmployeeId = _selectedCrew.EmployeeId,
                        Employee = _selectedCrew.Employee
                    };
                }

                OnPropertyChanged();
            }
        }
        public EmployeeInFilmCrew EditableCrew
        {
            get => _editableCrew;
            set
            {
                _editableCrew = value;
                OnPropertyChanged();
            }
        }

        public FilmCrewWindow()
        {
            InitializeComponent();

            using (var db = new CinemaDbContext())
            {
                //CrewMembers = new ObservableCollection<FilmCrewTableRow>(
                //    db.FilmCrews
                //    .AsNoTracking()
                //    .Include(fc => fc.Project)
                //    .Include(fc => fc.EmployeeInFilmCrews)
                //        .ThenInclude(efc => efc.Employee) 
                //    .ToList()
                //    .Select(fc => new FilmCrewTableRow(
                //        fc.Id,
                //        fc.Project,
                //        fc.EmployeeInFilmCrews.Select(efc => efc.Employee).ToList(),
                //        fc.Id
                //    ))
                //    .ToList());

                CrewMembers = GetCrewMembers();

                Employees = new ObservableCollection<Employee>(db.Employees
                    .AsNoTracking()
                    .Include(e => e.Speciality)
                    .ToList());

                Specialities = new ObservableCollection<Speciality>(db.Specialities
                    .AsNoTracking()
                    .ToList());

                Projects = new ObservableCollection<Project>(db.Projects
                    .AsNoTracking()
                    .Include(p => p.FilmCrews)
                    .ToList());
            } 

            DataContext = this;
        }

        private ObservableCollection<EmployeeInFilmCrew> GetCrewMembers()
        {
            using (var db = new CinemaDbContext())
            {
                return CrewMembers = new ObservableCollection<EmployeeInFilmCrew>(db.EmployeeInFilmCrews
                    .AsNoTracking()
                    .Include(cm => cm.Employee)
                        .ThenInclude(e => e.Speciality)
                    .Include(cm => cm.FilmCrew)
                        .ThenInclude(fc => fc.Project)
                    .ToList());
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            EditableCrew = new EmployeeInFilmCrew() { FilmCrew = new FilmCrew()};
            CrewMembersTable.SelectedIndex = -1;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

            if (EditableCrew?.FilmCrew?.ProjectId == null)
            {
                MessageBox.Show("Выберите проект");
                return;
            }

            if (EditableCrew?.EmployeeId == null)
            {
                MessageBox.Show("Выберите сотрудника");
                return;
            }

            try
            {
                using (var db = new CinemaDbContext())
                {
                    var targetCrew = db.FilmCrews.FirstOrDefault(fc => fc.ProjectId == EditableCrew.FilmCrew.ProjectId);

                    if (targetCrew == null)
                    {
                        targetCrew = new FilmCrew { ProjectId = EditableCrew.FilmCrew.ProjectId };
                        db.FilmCrews.Add(targetCrew);
                        db.SaveChanges();
                    }

                    if (EditableCrew.Id == 0)
                    {
                        var newLink = new EmployeeInFilmCrew
                        {
                            FilmCrewId = targetCrew.Id,
                            EmployeeId = EditableCrew.EmployeeId
                        };

                        db.EmployeeInFilmCrews.Add(newLink);
                    }
                    else
                    {
                        var crewFromDb = db.EmployeeInFilmCrews.First(e => e.Id == EditableCrew.Id);

                        crewFromDb.FilmCrewId = targetCrew.Id;
                        crewFromDb.EmployeeId = EditableCrew.EmployeeId;
                    }

                    db.SaveChanges();

                    MessageBox.Show("Данные успешно сохранены в бд");

                    CrewMembers = GetCrewMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных в бд: {ex.InnerException?.Message ?? ex.Message}");
            }
            
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
