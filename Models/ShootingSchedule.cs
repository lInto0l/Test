using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class ShootingSchedule
{
    public int Id { get; set; }

    public DateOnly? Date { get; set; }

    public int ProjectStageId { get; set; }

    public TimeOnly? Time { get; set; }

    public int FilmCrewId { get; set; }

    public int ProjectFilmSetId { get; set; }

    public string? Description { get; set; }

    public virtual FilmCrew FilmCrew { get; set; } = null!;

    public virtual ProjectFilmSet ProjectFilmSet { get; set; } = null!;

    public virtual ProjectStage ProjectStage { get; set; } = null!;
}
