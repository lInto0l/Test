using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class Address
{
    public int Id { get; set; }

    public int StreetId { get; set; }

    public string? Building { get; set; }

    public virtual ICollection<FilmSet> FilmSets { get; set; } = new List<FilmSet>();

    public virtual Street Street { get; set; } = null!;
}
