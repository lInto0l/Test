using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class FilmSetType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<FilmSet> FilmSets { get; set; } = new List<FilmSet>();
}
