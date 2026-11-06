using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class Rental
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public DateOnly? Date { get; set; }

    public int? CinemaQuantity { get; set; }

    public double? Rating { get; set; }

    public double? Charge { get; set; }

    public virtual Project Project { get; set; } = null!;
}
