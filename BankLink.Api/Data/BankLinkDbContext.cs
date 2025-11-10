using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BankLink.Api.Domain;

namespace BankLink.Api.Data;

public class BankLinkDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public BankLinkDbContext(DbContextOptions<BankLinkDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<ExternalBank> ExternalBanks => Set<ExternalBank>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Client>().HasIndex(x => x.Dni).IsUnique();

        mb.Entity<Account>().HasIndex(x => x.AccountNumber).IsUnique();
        mb.Entity<Account>().Property(x => x.Balance).HasColumnType("decimal(18,2)");
        mb.Entity<Account>()
            .HasOne(a => a.Client)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Movement>().Property(m => m.Amount).HasColumnType("decimal(18,2)");
        mb.Entity<Movement>()
            .HasOne(m => m.Account)
            .WithMany(a => a.Movements)
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Movement>().Property(m => m.OccurredAt).HasDefaultValueSql("GETUTCDATE()");

        // Configuración de ExternalBank
        mb.Entity<ExternalBank>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();

            b.Property(x => x.Name)
                .HasMaxLength(80)
                .IsRequired();

            b.Property(x => x.Code)
                .HasMaxLength(30)
                .IsRequired();

            b.Property(x => x.BaseUrl)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.TransferEndpoint)
                .HasMaxLength(120)
                .IsRequired();

            b.Property(x => x.ValidationEndpoint)
                .HasMaxLength(120); // NUEVO: opcional

            b.Property(x => x.ApiKey)
                .HasMaxLength(120)
                .IsRequired();

            b.Property(x => x.IsActive)
                .HasDefaultValue(true); // NUEVO: por defecto activo

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()"); // NUEVO: fecha automática
        });
    }
}
