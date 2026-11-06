using CinemaStudio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectsWindow : Window, INotifyPropertyChanged
    {
        public DateTime MinStartDate { get; set; } = DateTime.Today.AddYears(-100);
        public DateTime MaxStartDate { get; set; } = DateTime.Today;
        public DateTime MinEndDate { get; set; } = DateTime.Today.AddYears(-100);
        public DateTime MaxEndDate { get; set; } = DateTime.Today.AddYears(10);


        private ObservableCollection<Project> _projects;
        private Project _selectedProject;
        private Project _editableProject = new Project();

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (value != null)
                {
                    _selectedProject = value;
                    EditableProject = new Project
                    {
                        Id = value.Id,
                        Name = value.Name,
                        Budget = value.Budget,
                        TargetAudienceId = value.TargetAudienceId,
                        GenreId = value.GenreId,
                        StatusId = value.StatusId
                    };

                    UIStartDate = value.StartDate?.ToDateTime(TimeOnly.MinValue);
                    UIEndDate = value.EndDate?.ToDateTime(TimeOnly.MinValue);
                }

                OnPropertyChanged();
            }
        }
        public Project EditableProject
        {
            get => _editableProject;
            set
            {
                _editableProject = value;
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
        public DateTime? UIStartDate
        {
            get => EditableProject?.StartDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EditableProject.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

                OnPropertyChanged();
            }
        }
        public DateTime? UIEndDate
        {
            get => EditableProject?.EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EditableProject.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProjectsWindow()
        {
            InitializeComponent();

            OnStart();

            DataContext = this;
        }

        private ObservableCollection<Project> GetProjects(ProjectStatus? filter = null)
        {
            using (var db = new CinemaDbContext())
            {
                return new ObservableCollection<Project>(db.Projects
                    .AsNoTracking()
                    .Include(p => p.TargetAudience)
                    .Include(p => p.Status)
                    .Include(p => p.Genre)
                    .Where(p => filter == null || p.StatusId == filter.Id)
                    .ToList());
            }
        }

        private void WipeForm()
        {
            EditableProject = new Project();
            Projects = GetProjects();
            StatusFilterCombo.SelectedIndex = -1;

            UIStartDate = null;
            UIEndDate = null;
        }

        private void OnStart()
        {
            try
            {
                using (var db = new CinemaDbContext())
                {
                    Projects = GetProjects();

                    GenreCombo.ItemsSource = db.Genres
                        .AsNoTracking()
                        .ToList();

                    StatusCombo.ItemsSource = db.ProjectStatuses
                        .AsNoTracking()
                        .ToList();

                    AudienceCombo.ItemsSource = db.TargetAudiences
                        .AsNoTracking()
                        .ToList();

                    StatusFilterCombo.ItemsSource = StatusCombo.ItemsSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении проектов: {ex.InnerException?.Message}");
            }
        }


        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            var filter = StatusFilterCombo.SelectedItem as ProjectStatus;

            if (filter != null)
            {
                using (var db = new CinemaDbContext())
                {
                    Projects = GetProjects(filter);
                }
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            WipeForm();
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EditableProject == null)
            {
                return;
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(EditableProject.Name))
            {
                errors.Add("Введите название");
            }

            if (EditableProject.GenreId == 0)
            {
                errors.Add("Выберите жанр");
            }

            if (EditableProject.Budget <= 0)
            {
                errors.Add("Бюджет должен быть больше 0");
            }

            if (EditableProject.StartDate == null 
                || EditableProject.StartDate < DateOnly.FromDateTime(MinStartDate)
                || EditableProject.StartDate > DateOnly.FromDateTime(MaxStartDate))
            {
                errors.Add("Выберите корректную начальную дату");
            }

            if (EditableProject.EndDate == null
                || EditableProject.EndDate < DateOnly.FromDateTime(MinEndDate)
                || EditableProject.EndDate > DateOnly.FromDateTime(MaxEndDate))
            {
                errors.Add("Выберите корректную конечную дату");
            }

            if (EditableProject.EndDate <= EditableProject.StartDate)
            {
                errors.Add("Дата конца съемок не может быть раньше начала");
            }

            if (EditableProject.StatusId == 0)
            {
                errors.Add("Выберите статус проекта");
            }

            if (EditableProject.TargetAudienceId == 0)
            {
                errors.Add("Выберите аудиторию проекта");
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
                    if (EditableProject.Id == 0)
                    {
                        db.Projects.Add(EditableProject);
                    }
                    else
                    {
                        var projectFromDb = db.Projects.First(p => EditableProject.Id == p.Id);

                        projectFromDb.Name = EditableProject.Name;
                        projectFromDb.Budget = EditableProject.Budget;
                        projectFromDb.StartDate = EditableProject.StartDate;
                        projectFromDb.EndDate = EditableProject.EndDate;
                        projectFromDb.GenreId = EditableProject.GenreId;
                        projectFromDb.StatusId = EditableProject.StatusId;
                        projectFromDb.TargetAudienceId = EditableProject.TargetAudienceId;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные сохранены");

                    WipeForm();
                    Projects = GetProjects();
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
