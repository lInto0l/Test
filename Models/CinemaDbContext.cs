using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CinemaStudio.Models;

public partial class CinemaDbContext : DbContext
{
    public CinemaDbContext()
    {
    }

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeInFilmCrew> EmployeeInFilmCrews { get; set; }

    public virtual DbSet<FilmCrew> FilmCrews { get; set; }

    public virtual DbSet<FilmSet> FilmSets { get; set; }

    public virtual DbSet<FilmSetType> FilmSetTypes { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectCost> ProjectCosts { get; set; }

    public virtual DbSet<ProjectFilmSet> ProjectFilmSets { get; set; }

    public virtual DbSet<ProjectStage> ProjectStages { get; set; }

    public virtual DbSet<ProjectStatus> ProjectStatuses { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<ShootingSchedule> ShootingSchedules { get; set; }

    public virtual DbSet<Speciality> Specialities { get; set; }

    public virtual DbSet<Street> Streets { get; set; }

    public virtual DbSet<TargetAudience> TargetAudiences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("host=localhost;database=cinemaStudio;username=postgres;password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Address_pkey");

            entity.ToTable("Address");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Address_address_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Building)
                .HasMaxLength(10)
                .HasColumnName("building");
            entity.Property(e => e.StreetId).HasColumnName("street_id");

            entity.HasOne(d => d.Street).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.StreetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Address_street_id_fkey");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("City_pkey");

            entity.ToTable("City");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"City_city_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Employee_pkey");

            entity.ToTable("Employee");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Employee_employee_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Avatar)
               .HasColumnType("character varying")
               .HasColumnName("avatar");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Email)
                .HasMaxLength(300)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(75)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .HasColumnName("phone");
            entity.Property(e => e.SpecialityId).HasColumnName("speciality_id");
            entity.Property(e => e.Surname)
                .HasMaxLength(75)
                .HasColumnName("surname");

            entity.HasOne(d => d.Speciality).WithMany(p => p.Employees)
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Employee_speciality_id_fkey");
        });

        modelBuilder.Entity<EmployeeInFilmCrew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EmployeeInFilmCrew_pkey");

            entity.ToTable("EmployeeInFilmCrew");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FilmCrewId).HasColumnName("filmCrew_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeInFilmCrews)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EmployeeInFilmCrew_employee_id_fkey");

            entity.HasOne(d => d.FilmCrew).WithMany(p => p.EmployeeInFilmCrews)
                .HasForeignKey(d => d.FilmCrewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EmployeeInFilmCrew_filmCrew_id_fkey");
        });

        modelBuilder.Entity<FilmCrew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FilmCrew_pkey");

            entity.ToTable("FilmCrew");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"FilmCrew_filmCrew_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.Project).WithMany(p => p.FilmCrews)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FilmCrew_project_id_fkey");
        });

        modelBuilder.Entity<FilmSet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FilmSet_pkey1");

            entity.ToTable("FilmSet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"FilmSet_filmSet_id_seq1\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.DayRent).HasColumnName("dayRent");
            entity.Property(e => e.FilmSetTypeId).HasColumnName("filmSetType_id");

            entity.HasOne(d => d.Address).WithMany(p => p.FilmSets)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FilmSet_address_id_fkey");

            entity.HasOne(d => d.FilmSetType).WithMany(p => p.FilmSets)
                .HasForeignKey(d => d.FilmSetTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FilmSet_filmSetType_id_fkey");
        });

        modelBuilder.Entity<FilmSetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FilmSetType_pkey");

            entity.ToTable("FilmSetType");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"FilmSetType_filmSetType_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Genre_pkey");

            entity.ToTable("Genre");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Genre_genre_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Project_pkey");

            entity.ToTable("Project");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Project_project_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Budget).HasColumnName("budget");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TargetAudienceId).HasColumnName("targetAudience_id");

            entity.HasOne(d => d.Genre).WithMany(p => p.Projects)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Project_genre_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Projects)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Project_status_id_fkey");

            entity.HasOne(d => d.TargetAudience).WithMany(p => p.Projects)
                .HasForeignKey(d => d.TargetAudienceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Project_targetAudience_id_fkey");
        });

        modelBuilder.Entity<ProjectCost>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("ProjectCost_pkey");

            entity.ToTable("ProjectCost");

            entity.Property(e => e.ProjectId)
                .ValueGeneratedOnAdd()
                .HasColumnName("project_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Equipment).HasColumnName("equipment");
            entity.Property(e => e.PostProcessing).HasColumnName("post-processing");
            entity.Property(e => e.RentCost).HasColumnName("rentCost");
            entity.Property(e => e.Salaries).HasColumnName("salaries");
            entity.Property(e => e.Total).HasColumnName("total");

            entity.HasOne(d => d.Project).WithOne(p => p.ProjectCost)
                .HasForeignKey<ProjectCost>(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_projectcost_project");
        });

        modelBuilder.Entity<ProjectFilmSet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProjectFilmSet_pkey");

            entity.ToTable("ProjectFilmSet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FilmSetId).HasColumnName("filmSet_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");

            entity.HasOne(d => d.FilmSet).WithMany(p => p.ProjectFilmSets)
                .HasForeignKey(d => d.FilmSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProjectFilmSet_filmSet_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectFilmSets)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProjectFilmSet_project_id_fkey");
        });

        modelBuilder.Entity<ProjectStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProjectStage_pkey");

            entity.ToTable("ProjectStage");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"ProjectStage_projectStage_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProjectStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProjectStatus_pkey");

            entity.ToTable("ProjectStatus");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"ProjectStatus_projectStatus_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
        });

        //modelBuilder.Entity<Rental>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("FilmSet_pkey");

        //    entity.ToTable("Rental");

        //    entity.Property(e => e.Id)
        //        .HasDefaultValueSql("nextval('\"FilmSet_filmSet_id_seq\"'::regclass)")
        //        .HasColumnName("id");
        //    entity.Property(e => e.Charge).HasColumnName("charge");
        //    entity.Property(e => e.CinemaQuantity).HasColumnName("cinemaQuantity");
        //    entity.Property(e => e.Date).HasColumnName("date");
        //    entity.Property(e => e.ProjectId).HasColumnName("project_id");
        //    entity.Property(e => e.Rating).HasColumnName("rating");

        //    entity.HasOne(d => d.Project).WithMany(p => p.Rentals)
        //        .HasForeignKey(d => d.ProjectId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FilmSet_project_id_fkey");
        //});

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FilmSet_pkey");

            entity.ToTable("Rental");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"FilmSet_filmSet_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Charge).HasColumnName("charge");
            entity.Property(e => e.CinemaQuantity).HasColumnName("cinemaQuantity");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.Project).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FilmSet_project_id_fkey");
        });

        modelBuilder.Entity<ShootingSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ShootingSchedule_pkey");

            entity.ToTable("ShootingSchedule");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.FilmCrewId).HasColumnName("filmCrew_id");
            entity.Property(e => e.ProjectFilmSetId).HasColumnName("projectFilmSet_id");
            entity.Property(e => e.ProjectStageId).HasColumnName("projectStage_id");
            entity.Property(e => e.Time).HasColumnName("time");

            entity.HasOne(d => d.FilmCrew).WithMany(p => p.ShootingSchedules)
                .HasForeignKey(d => d.FilmCrewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ShootingSchedule_filmCrew_id_fkey");

            entity.HasOne(d => d.ProjectFilmSet).WithMany(p => p.ShootingSchedules)
                .HasForeignKey(d => d.ProjectFilmSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ShootingSchedule_projectFilmSet_id_fkey");

            entity.HasOne(d => d.ProjectStage).WithMany(p => p.ShootingSchedules)
                .HasForeignKey(d => d.ProjectStageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ShootingSchedule_projectStage_id_fkey");
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Speciality_pkey");

            entity.ToTable("Speciality");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Speciality_speciality_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Street>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Street_pkey");

            entity.ToTable("Street");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Street_street_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");

            entity.HasOne(d => d.City).WithMany(p => p.Streets)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Street_city_id_fkey");
        });

        modelBuilder.Entity<TargetAudience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TargetAudience_pkey");

            entity.ToTable("TargetAudience");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"TargetAudience_targetAudiece_id_seq\"'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(75)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
