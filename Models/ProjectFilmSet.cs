using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class ProjectFilmSet
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int FilmSetId { get; set; }

    public virtual FilmSet FilmSet { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<ShootingSchedule> ShootingSchedules { get; set; } = new List<ShootingSchedule>();
}
