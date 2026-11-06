using CinemaStudio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
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

namespace CinemaStudio
{
    /// <summary>
    /// Логика взаимодействия для ProjectCostWindow.xaml
    /// </summary>
    public partial class ProjectCostWindow : Window, INotifyPropertyChanged
    {
        private static readonly Dictionary<CostType, string> DisplayTypes = new Dictionary<CostType, string>()
                {
                    { CostType.Salaries, "Зарплата"},
                    { CostType.RentCost, "Аренда" },
                    { CostType.Equipment, "Оборудование" },
                    { CostType.PostProcessing, "Постобработка" }
                };

        private List<CostTableRow> _allCosts;
        private ObservableCollection<CostTableRow> _costs;
        private int _filterProjectId;
        private CostType _filterType;
        private CostTableRow _editableCost = new CostTableRow(new ProjectCost());

        public DateTime MinDate { get; set; } = DateTime.Today.AddYears(-100);
        public DateTime MaxDate { get; set; } = DateTime.Today;
        public ObservableCollection<CostTableRow> Costs
        { 
            get => _costs; 
            set
            {
                _costs = value;
                OnPropertyChanged();
            }
        }
        public int FilterProjectId 
        { 
            get => _filterProjectId; 
            set
            {
                _filterProjectId = value;
                OnPropertyChanged();
            }
        }
        public CostType FilterType
        {
            get => _filterType;
            set
            {
                _filterType = value;
                OnPropertyChanged();
            }
        }
        public CostTableRow EditableCost
        {
            get => _editableCost;
            set
            {
                if (value != null)
                {
                    _editableCost = new CostTableRow(value.ProjectCost, value.CostType);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedDate));
                }
                else
                    _editableCost = new CostTableRow(new ProjectCost());
            }
        }
        public DateTime? SelectedDate
        {
            get => EditableCost?.ProjectCost?.Date?.ToDateTime(TimeOnly.MinValue);
            set
            {
                EditableCost.ProjectCost.Date = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public ProjectCostWindow()
        {
            InitializeComponent();

            OnStart();

            DataContext = this;
        }

        private void OnStart()
        {
            UpdateCosts();

            using (var db = new CinemaDbContext())
            {
                ProjectsCombo.ItemsSource = db.Projects.ToList();
                ProjectComboField.ItemsSource = ProjectsCombo.ItemsSource;
                CostTypeCombo.ItemsSource = DisplayTypes;
                CostTypesCombo.ItemsSource = DisplayTypes;
            }
        }

        private List<CostTableRow> GetCosts()
        {
            using (var db = new CinemaDbContext())
            {
                return db.ProjectCosts
                    .AsNoTracking()
                    .Include(c => c.Project)
                    .Select(c => new CostTableRow(c, CostType.Salaries))
                    .ToList();
            } 
        }
        private void UpdateCosts()
        {
            _allCosts = GetCosts();
            Costs = new ObservableCollection<CostTableRow>(_allCosts);
        }
        private void WipeForm()
        {
            CostsTable.SelectedIndex = -1;
            FilterType = CostType.Salaries;
            FilterProjectId = -1;
            EditableCost = new CostTableRow(new ProjectCost());

            Costs = new ObservableCollection<CostTableRow>(_allCosts);
        }

        private void CostsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCost = (CostsTable.SelectedItem as CostTableRow);

            if (selectedCost != null)
            {
                EditableCost = new CostTableRow(selectedCost.ProjectCost, selectedCost.CostType);

                OnPropertyChanged();
            }
            else
            {
                EditableCost = new CostTableRow(new ProjectCost());
            }

        }
        private void ProjectComboField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new CinemaDbContext())
            {
                var project = ProjectComboField.SelectedItem as Project;

                if (project == null)
                {
                    return;
                }

                var cost = db.ProjectCosts.FirstOrDefault(c => c.ProjectId == project.Id);

                if (cost != null)
                {
                    EditableCost.ProjectCost = cost;

                }
                else
                {
                    EditableCost.ProjectCost = new ProjectCost() 
                    { 
                        ProjectId = project.Id
                    };
                }

                OnPropertyChanged(nameof(EditableCost));
            }
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            var filteredCosts = _allCosts
                .Where(c => FilterProjectId <= 0 
                || c.ProjectCost.ProjectId == FilterProjectId)
                .ToList();
            
            filteredCosts.ForEach(c => c.CostType = FilterType);
            Costs = new ObservableCollection<CostTableRow>(filteredCosts);
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            WipeForm();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EditableCost.ProjectCost.Date == null
                    || EditableCost.ProjectCost.Date > DateOnly.FromDateTime(MaxDate)
                    || EditableCost.ProjectCost.Date < DateOnly.FromDateTime(MinDate))
                {
                    MessageBox.Show("Выбери корректную дату");
                    return;
                }

                using (var db = new CinemaDbContext())
                {
                    var costFromDb = db.ProjectCosts.FirstOrDefault(c => 
                            c.ProjectId == EditableCost.ProjectCost.ProjectId);

                    if (costFromDb == null)
                    {
                        EditableCost.ProjectCost.Project = null;
                        db.ProjectCosts.Add(EditableCost.ProjectCost);
                    }
                    else
                    {

                        costFromDb.Date = EditableCost.ProjectCost.Date;
                        costFromDb.Salaries = EditableCost.Salaries;
                        costFromDb.RentCost = EditableCost.RentCost;
                        costFromDb.Equipment = EditableCost.Equipment;
                        costFromDb.PostProcessing = EditableCost.PostProcessing;
                        costFromDb.Total = EditableCost.Total;
                    }

                    db.SaveChanges();

                    MessageBox.Show("Данные сохранены");

                    UpdateCosts();
                    WipeForm();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в бд: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public partial class CostTableRow : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            private CostType _costType;

            public CostType CostType 
            { 
                get => _costType;
                set
                {
                    _costType = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Cost));
                } 
            }
            public double? Cost 
            { 
                get => (double?)ProjectCost?.GetType().GetProperty(CostType.ToString())?.GetValue(ProjectCost)!; 
                set
                {
                    if (value is double cost && cost > 0)
                    {
                        var property = ProjectCost?.GetType().GetProperty(CostType.ToString());
                        property?.SetValue(ProjectCost, cost);

                        OnPropertyChanged();
                    }
                }

            }
            public ProjectCost ProjectCost { get; set; }
            public string DisplayType { get => DisplayTypes.GetValueOrDefault(CostType, "Неизвестно"); }

            public Project Project => ProjectCost.Project;
            public double? Salaries
            {
                get => ProjectCost.Salaries;
                set
                {
                    if (value is double salary && salary > 0)
                    {
                        ProjectCost.Salaries = salary;

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(Total));
                    }
                }
            }
            public double? RentCost
            {
                get => ProjectCost.RentCost;
                set
                {
                    if (value is double rent && rent > 0)
                    {
                        ProjectCost.RentCost = rent;

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(Total));
                    }
                }
            }
            public double? Equipment
            {
                get => ProjectCost.Equipment;
                set
                {
                    if (value is double equip && equip > 0)
                    {
                        ProjectCost.Equipment = equip;

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(Total));
                    }
                }
            }
            public double? PostProcessing
            {
                get => ProjectCost.PostProcessing;
                set
                {
                    if (value is double pp && pp > 0)
                    {
                        ProjectCost.PostProcessing = pp;

                        OnPropertyChanged();
                        OnPropertyChanged(nameof(Total));
                    }
                }
            }
            public double? Total => 
                (ProjectCost.Salaries ?? 0) + (ProjectCost.RentCost ?? 0) +
                (ProjectCost.Equipment ?? 0) + (ProjectCost.PostProcessing ?? 0);

            public CostTableRow(ProjectCost projectCost, CostType costType = CostType.Salaries)
            {
                CostType = costType;
                ProjectCost = new ProjectCost()
                {
                    RentCost = projectCost?.RentCost,
                    Date = projectCost?.Date,
                    Equipment = projectCost?.Equipment,
                    Total = projectCost?.Total,
                    ProjectId = projectCost?.ProjectId ?? 0,
                    Salaries = projectCost?.Salaries,
                    PostProcessing = projectCost?.PostProcessing,
                    Project = new Project { Name = projectCost?.Project?.Name }
                };

            }

            public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public enum CostType
        {
            Salaries,
            RentCost,
            Equipment,
            PostProcessing,
            Total
        }
    }
}
