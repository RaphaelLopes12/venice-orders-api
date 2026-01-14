using Microsoft.EntityFrameworkCore;
using VeniceOrders.Domain.Entities;

namespace VeniceOrders.Infrastructure.Persistence.SqlServer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClienteId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Total).HasPrecision(18, 2);
        });
    }
}
