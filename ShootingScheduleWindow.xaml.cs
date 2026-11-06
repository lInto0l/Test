using CinemaStudio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security;
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
    /// Логика взаимодействия для ShootingScheduleWindow.xaml
    /// </summary>
    ///

    // Дописать логику с площадками для фильма. Сейчас выводятся все площадки, но они не записываются в объект
    // Либо оставить только площадки проекта, либо добавить возможность выбирать любую площадку и если она уже
    // Закреплена за проектом не создавать новую строку в ProjectFimlSet, если нет - создать

    // Редактирование объекта не работает, потому что ProjectFilmSet нужно переписать, в остальном оно гуд
    public partial class ShootingScheduleWindow : Window, INotifyPropertyChanged
    {
        public DateTime MinDate { get; set; } = DateTime.Today.AddYears(-100);
        public DateTime MaxDate { get; set; } = DateTime.Today;

        private TimeOnly StartTime = new TimeOnly(1, 0);
        private TimeOnly EndTime = new TimeOnly(23, 0);
        private int TimeStep = 30;

        private ObservableCollection<ShootingSchedule> _schedules;
        private ObservableCollection<TimeOnly> _timeList;
        private ShootingSchedule _editableSchedule = new ShootingSchedule() { ProjectFilmSet = new ProjectFilmSet() };
        private DateTime? _selectedDate;
        private FilmSet _filmSet;


        public ObservableCollection<ShootingSchedule> Schedules
        {
            get => _schedules;
            set
            {
                _schedules = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<TimeOnly> TimeList 
        { 
            get => _timeList; 
            set
            {
                _timeList = value;
                OnPropertyChanged();
            }
        }
        public ShootingSchedule EditableSchedule
        {
            get => _editableSchedule;
            set
            {
                _editableSchedule = value;
                SelectedDate = _editableSchedule?.Date?.ToDateTime(TimeOnly.MinValue);
                TimeList = new ObservableCollection<TimeOnly>(GetTimes(StartTime, EndTime, TimeStep));
                TimeList = new ObservableCollection<TimeOnly>(DeleteBusyTime(TimeList.ToList(), value.Id));

                OnPropertyChanged();
            }
        }
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;

                if (value != null)
                {
                    EditableSchedule.Date = DateOnly.FromDateTime((DateTime)_selectedDate);
                }
                else
                {
                    
                    EditableSchedule.Date = null;
                }

                OnPropertyChanged();
            }
        }
        public FilmSet FilmSet
        {
            get => _filmSet;
            set
            {
                _filmSet = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        public ShootingScheduleWindow()
        {
            InitializeComponent();

            OnStart();

            DataContext = this;
        }

        private void OnStart()
        {
            TimeList = new ObservableCollection<TimeOnly>(GetTimes(StartTime, EndTime, TimeStep));

            using (var db = new CinemaDbContext())
            {
                Schedules = new ObservableCollection<ShootingSchedule>(GetSchedules());

                ProjectCombo.ItemsSource = db.Projects
                    .AsNoTracking()
                    .ToList();

                FilmSetCombo.ItemsSource = db.FilmSets
                    .AsNoTracking()
                    .ToList();

                StageCombo.ItemsSource = db.ProjectStages
                    .AsNoTracking()
                    .ToList();

                FilmCrewCombo.ItemsSource = db.FilmCrews
                    .AsNoTracking()
                    .ToList();
            }
        }

        private List<ShootingSchedule> GetSchedules()
        {
            using (var db = new CinemaDbContext())
            {
                return db.ShootingSchedules
                    .AsNoTracking()
                    .Include(s => s.ProjectFilmSet)
                    .Include(s => s.ProjectStage)
                    .Include(s => s.FilmCrew)
                    .ToList();
            }
            
        }
        private List<TimeOnly> GetTimes(TimeOnly startTime, TimeOnly endTime, int timeStep)
        {
            var times = new List<TimeOnly>();
            var currentTime = startTime;

            while (currentTime <= endTime && currentTime >= startTime && timeStep > 0)
            {
                times.Add(currentTime);
                currentTime = currentTime.AddMinutes(timeStep);
            }

            return times;
        }
        private List<TimeOnly> DeleteBusyTime(List<TimeOnly> times, int scheduleId)
        {
            var clearedTimes = times;

            using (var db = new CinemaDbContext())
            {
                var busyTimes = db.ShootingSchedules
                    .Where(s => s.Id != scheduleId)
                    .Select(s => s.Time)
                    .Cast<TimeOnly>()
                    .ToHashSet();

                clearedTimes = times.Where(t => !busyTimes.Contains(t)).ToList();
            }

            return clearedTimes;
        }
        private void WipeForm()
        {
            SheduleTable.SelectedIndex = -1;
            EditableSchedule = new ShootingSchedule() { ProjectFilmSet = new ProjectFilmSet() };
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            WipeForm();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> errors = new List<string>();

            if (EditableSchedule == null)
            {
                return;
            }

            if (EditableSchedule.Date == null 
                || EditableSchedule.Date < DateOnly.FromDateTime(MinDate)
                || EditableSchedule.Date > DateOnly.FromDateTime(MaxDate))
            {
                errors.Add("Выберите корректную дату");
            }

            if (EditableSchedule.Time == null)
            {
                errors.Add("Выберите время");
            }

            if (EditableSchedule.ProjectFilmSet?.ProjectId == 0)
            {
                errors.Add("Выберите проект");
            }

            if (EditableSchedule.ProjectFilmSet?.FilmSetId == 0)
            {
                errors.Add("Выберите площадку");
            }

            if (EditableSchedule.FilmCrewId == 0)
            {
                errors.Add("Выберите съемочную группу");
            }

            if (EditableSchedule.ProjectStageId == 0)
            {
                errors.Add("Выберите сцену");
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

                    if (EditableSchedule.Id == 0)
                    {
                        db.ShootingSchedules.Add(EditableSchedule);
                    }
                    else
                    {
                        var scheduleFromDb = db.ShootingSchedules
                            .Include(s => s.FilmCrew)
                            .Include(s => s.ProjectFilmSet)
                            .First(s => s.Id == EditableSchedule.Id);

                        if (scheduleFromDb.ProjectFilmSetId == 0)
                        {
                            scheduleFromDb.ProjectFilmSet = new ProjectFilmSet()
                            {
                                FilmSetId = EditableSchedule.ProjectFilmSet.FilmSetId
                            };
                        }
                        else
                        {
                            scheduleFromDb.ProjectFilmSet.FilmSetId = EditableSchedule.ProjectFilmSet.FilmSetId;
                        }

                        scheduleFromDb.FilmCrewId = EditableSchedule.FilmCrewId;
                        scheduleFromDb.ProjectFilmSet.ProjectId = EditableSchedule.ProjectFilmSet.ProjectId;
                        scheduleFromDb.ProjectStageId = EditableSchedule.ProjectStageId;
                        scheduleFromDb.Date = EditableSchedule.Date;
                        scheduleFromDb.Time = EditableSchedule.Time;
                        scheduleFromDb.Description = EditableSchedule.Description;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные сохранены");

                    Schedules = new ObservableCollection<ShootingSchedule>(GetSchedules());
                    WipeForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в бд: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        private void SheduleTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = SheduleTable.SelectedItem as ShootingSchedule;
            if (selectedRow != null)
            {
                EditableSchedule = new ShootingSchedule()
                {
                    Id = selectedRow.Id,
                    Date = selectedRow.Date,
                    Time = selectedRow.Time,
                    Description = selectedRow.Description,
                    ProjectStageId = selectedRow.ProjectStageId,
                    FilmCrewId = selectedRow.FilmCrewId,
                   
                    ProjectFilmSet = new ProjectFilmSet()
                    {
                        Id = selectedRow.ProjectFilmSetId,
                        ProjectId = selectedRow.ProjectFilmSet.ProjectId,
                        FilmSetId = selectedRow.ProjectFilmSet.FilmSetId
                    }
                };
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
