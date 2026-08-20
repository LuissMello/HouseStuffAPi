using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Tasks;

namespace HouseStuff.Infrastructure.Identity;

public sealed class HouseStuffDbContext(DbContextOptions<HouseStuffDbContext> options)
    : IdentityDbContext<HouseStuffUser>(options)
{
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<Pot> Pots => Set<Pot>();
    public DbSet<HouseholdTask> HouseholdTasks => Set<HouseholdTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Residence>(entity =>
        {
            entity.ToTable("Residences");
            entity.HasKey(residence => residence.Id);
            entity.Property(residence => residence.Name).HasMaxLength(80).IsRequired();
            entity.Property(residence => residence.CreatedByUserId).HasMaxLength(450).IsRequired();
        });

        builder.Entity<HouseStuffUser>(entity =>
        {
            entity.HasIndex(user => user.ResidenceId);
            entity.HasOne<Residence>()
                .WithMany()
                .HasForeignKey(user => user.ResidenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Pot>(entity =>
        {
            entity.ToTable("Pots");
            entity.HasKey(pot => pot.Id);
            entity.Property(pot => pot.Name).HasMaxLength(60).IsRequired();
            entity.Property(pot => pot.NormalizedName).HasMaxLength(60).IsRequired();
            entity.Property(pot => pot.Description).HasMaxLength(200);
            entity.HasIndex(pot => new { pot.ResidenceId, pot.NormalizedName }).IsUnique();
            entity.HasIndex(pot => new { pot.ResidenceId, pot.DisplayOrder });
            entity.HasOne<Residence>().WithMany().HasForeignKey(pot => pot.ResidenceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasAlternateKey(pot => new { pot.Id, pot.ResidenceId });
        });

        builder.Entity<HouseholdTask>(entity =>
        {
            entity.ToTable("HouseholdTasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Name).HasMaxLength(100).IsRequired();
            entity.Property(task => task.NormalizedName).HasMaxLength(100).IsRequired();
            entity.Property(task => task.Description).HasMaxLength(300);
            entity.Property(task => task.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(task => new { task.ResidenceId, task.PotId, task.NormalizedName }).IsUnique();
            entity.HasIndex(task => new { task.ResidenceId, task.PotId, task.IsActive });
            entity.HasOne<Pot>().WithMany()
                .HasForeignKey(task => new { task.PotId, task.ResidenceId })
                .HasPrincipalKey(pot => new { pot.Id, pot.ResidenceId })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
