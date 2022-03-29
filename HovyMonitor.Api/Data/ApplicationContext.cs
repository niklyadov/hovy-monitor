using HovyMonitor.Entity;
using Microsoft.EntityFrameworkCore;

namespace HovyMonitor.Api.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<SensorDetection> SensorDetections { get; private set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) => Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
