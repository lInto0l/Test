using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class ProjectCost
{
    public int ProjectId { get; set; }

    public double? Salaries { get; set; }

    public double? RentCost { get; set; }

    public double? Equipment { get; set; }

    public double? PostProcessing { get; set; }

    public double? Total { get; set; }

    public DateOnly? Date { get; set; }

    public virtual Project Project { get; set; } = null!;
}
