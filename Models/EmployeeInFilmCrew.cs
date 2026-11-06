using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class EmployeeInFilmCrew
{
    public int Id { get; set; }

    public int FilmCrewId { get; set; }

    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual FilmCrew FilmCrew { get; set; } = null!;
}
