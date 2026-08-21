using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Tasks;
using HouseStuff.Domain.Assignments;
using HouseStuff.Domain.Shopping;

namespace HouseStuff.Infrastructure.Identity;

public sealed class HouseStuffDbContext(DbContextOptions<HouseStuffDbContext> options)
    : IdentityDbContext<HouseStuffUser>(options)
{
    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<Pot> Pots => Set<Pot>();
    public DbSet<HouseholdTask> HouseholdTasks => Set<HouseholdTask>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<ShoppingCategory> ShoppingCategories => Set<ShoppingCategory>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();

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
            entity.HasIndex(task => new { task.ResidenceId, task.PotId, task.IsActive, task.NextAvailableAt });
            entity.HasOne<Pot>().WithMany()
                .HasForeignKey(task => new { task.PotId, task.ResidenceId })
                .HasPrincipalKey(pot => new { pot.Id, pot.ResidenceId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TaskAssignment>(entity =>
        {
            entity.ToTable("TaskAssignments");
            entity.HasKey(assignment => assignment.Id);
            entity.Property(assignment => assignment.AssignedToUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(assignment => assignment.AssignedToUserId)
                .IsUnique()
                .HasFilter("\"CompletedAt\" IS NULL");
            entity.HasIndex(assignment => assignment.HouseholdTaskId)
                .IsUnique()
                .HasFilter("\"CompletedAt\" IS NULL");
            entity.HasOne<HouseholdTask>().WithMany().HasForeignKey(assignment => assignment.HouseholdTaskId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<HouseStuffUser>().WithMany().HasForeignKey(assignment => assignment.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ShoppingCategory>(entity =>
        {
            entity.ToTable("ShoppingCategories");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(60).IsRequired();
            entity.Property(category => category.NormalizedName).HasMaxLength(60).IsRequired();
            entity.HasIndex(category => new { category.ResidenceId, category.NormalizedName }).IsUnique();
            entity.HasIndex(category => new { category.ResidenceId, category.DisplayOrder });
            entity.HasOne<Residence>().WithMany().HasForeignKey(category => category.ResidenceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasAlternateKey(category => new { category.Id, category.ResidenceId });
        });

        builder.Entity<ShoppingItem>(entity =>
        {
            entity.ToTable("ShoppingItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.Property(item => item.NormalizedName).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => new { item.ResidenceId, item.CategoryId, item.NormalizedName }).IsUnique();
            entity.HasOne<ShoppingCategory>().WithMany()
                .HasForeignKey(item => new { item.CategoryId, item.ResidenceId })
                .HasPrincipalKey(category => new { category.Id, category.ResidenceId })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
