using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class ProjectStage
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ShootingSchedule> ShootingSchedules { get; set; } = new List<ShootingSchedule>();
}
