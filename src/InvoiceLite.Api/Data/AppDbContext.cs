using InvoiceLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(320).IsRequired();
            entity.Property(c => c.Company).HasMaxLength(200);
            entity.Property(c => c.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(i => i.Number).HasMaxLength(32).IsRequired();
            entity.Property(i => i.Amount).HasPrecision(18, 2);
            entity.Property(i => i.Currency).HasMaxLength(3).IsRequired();

            entity.HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
