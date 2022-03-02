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
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
        });

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
            entity.Property(i => i.Currency).HasMaxLength(3).IsRequired();
            entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(16);
            entity.HasIndex(i => i.Number).IsUnique();

            entity.HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(i => i.LineItems)
                .WithOne()
                .HasForeignKey(li => li.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.Property(li => li.Description).HasMaxLength(500).IsRequired();
            entity.Property(li => li.Quantity).HasPrecision(18, 2);
            entity.Property(li => li.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.StripePaymentIntentId).HasMaxLength(64).IsRequired();
            entity.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            entity.Property(p => p.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(p => p.StripePaymentIntentId).IsUnique();

            entity.HasOne(p => p.Invoice)
                .WithMany()
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
