using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class FilmCrew
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public virtual ICollection<EmployeeInFilmCrew> EmployeeInFilmCrews { get; set; } = new List<EmployeeInFilmCrew>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<ShootingSchedule> ShootingSchedules { get; set; } = new List<ShootingSchedule>();
}
