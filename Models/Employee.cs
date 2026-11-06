using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public int SpecialityId { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public double? Bid { get; set; }

    public string? Phone { get; set; }

    public string Fullname => $"{Surname} {Name}";

    public string? Email { get; set; }
    public string? Avatar { get; set; }

    public virtual ICollection<EmployeeInFilmCrew> EmployeeInFilmCrews { get; set; } = new List<EmployeeInFilmCrew>();

    public virtual Speciality Speciality { get; set; } = null!;
}
