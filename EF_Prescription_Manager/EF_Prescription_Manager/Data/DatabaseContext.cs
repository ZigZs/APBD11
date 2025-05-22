using EF_Prescription_Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_Prescription_Manager.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    // Made protected constructor more explicit
    protected DatabaseContext() : base()
    {
    }

    // Added <DatabaseContext> for better type safety
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePrescriptionMedicament(modelBuilder);
        SeedInitialData(modelBuilder);
    }

    private static void ConfigurePrescriptionMedicament(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrescriptionMedicament>()
            .HasKey(pm => new { pm.IdMedicament, pm.IdPrescription });

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Prescription)
            .WithMany(p => p.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdPrescription);

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Medicament)
            .WithMany(m => m.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdMedicament);
    }

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicament>().HasData(
            new Medicament
            {
                IdMedicament = 1,
                Name = "Paracetamol",
                Description = "Przeciwbólowy",
                Type = "Tabletka"
            },
            new Medicament
            {
                IdMedicament = 2,
                Name = "Ranigast",
                Description = "NaZgage",
                Type = "Tabletka"
            }
        );

        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                IdDoctor = 1,
                FirstName = "Julia",
                LastName = "Zugaj",
                Email = "Zugajki@gmail.com"
            }
        );
    }
}