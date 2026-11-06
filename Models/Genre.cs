using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class Genre
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
