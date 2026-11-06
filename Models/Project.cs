using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class Project
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double? Budget { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int TargetAudienceId { get; set; }

    public int StatusId { get; set; }

    public int GenreId { get; set; }

    public virtual ICollection<FilmCrew> FilmCrews { get; set; } = new List<FilmCrew>();

    public virtual Genre Genre { get; set; } = null!;

    public virtual ProjectCost? ProjectCost { get; set; }

    public virtual ICollection<ProjectFilmSet> ProjectFilmSets { get; set; } = new List<ProjectFilmSet>();

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    public virtual ProjectStatus Status { get; set; } = null!;

    public virtual TargetAudience TargetAudience { get; set; } = null!;
}
