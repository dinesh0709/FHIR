using FHIR.Models;
using FHIR.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace FHIR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) 
        { }
        public DbSet<Patient> patients {  get; set; }
        public DbSet<PatientDto> patientsDto { get; set;}
       /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>().HasData(
                new Patient { RawResource = "awwesome",
                    Created=DateTime.Now,
                    LastUpdated=DateTime.Now });
        }*/
    }
}
