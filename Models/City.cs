using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class City
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Street> Streets { get; set; } = new List<Street>();
}
