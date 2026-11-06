using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class FilmSet
{
    public int Id { get; set; }

    public int AddressId { get; set; }

    public double? DayRent { get; set; }

    public int? Capacity { get; set; }

    public int FilmSetTypeId { get; set; }

    public virtual Address Address { get; set; } = null!;

    public string FullAddressName => $"г. {Address?.Street?.City?.Name} ул. {Address?.Street?.Name}, {Address?.Building}";

    public virtual FilmSetType FilmSetType { get; set; } = null!;

    public virtual ICollection<ProjectFilmSet> ProjectFilmSets { get; set; } = new List<ProjectFilmSet>();
}
