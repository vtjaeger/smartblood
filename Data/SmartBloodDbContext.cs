using Microsoft.EntityFrameworkCore;
using SmartBlood.Models;

namespace SmartBlood.Data
{
    public class SmartBloodDbContext : DbContext
    {
        public SmartBloodDbContext(DbContextOptions<SmartBloodDbContext> options)
            : base(options)
        {
        }

        public DbSet<Measurement> Measurements { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<AIAnalysis> AIAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AIAnalysis>()
                .HasOne(x => x.Measurement)
                .WithOne(x => x.AIAnalysis)
                .HasForeignKey<AIAnalysis>(x => x.MeasurementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}