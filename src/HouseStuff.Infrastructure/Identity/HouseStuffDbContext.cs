using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Tasks;
using HouseStuff.Domain.Assignments;
using HouseStuff.Domain.Shopping;
using HouseStuff.Domain.Purchases;
using HouseStuff.Domain.Calendar;

namespace HouseStuff.Infrastructure.Identity;

public sealed class HouseStuffDbContext(DbContextOptions<HouseStuffDbContext> options)
    : IdentityDbContext<HouseStuffUser>(options), IDataProtectionKeyContext
{
    // As chaves precisam ser compartilhadas: cada máquina do Fly geraria as suas e o cookie
    // emitido por uma seria rejeitado pela outra, derrubando a sessão de forma intermitente.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<Residence> Residences => Set<Residence>();
    public DbSet<Pot> Pots => Set<Pot>();
    public DbSet<HouseholdTask> HouseholdTasks => Set<HouseholdTask>();
    public DbSet<HouseholdTaskEligibleUser> HouseholdTaskEligibleUsers => Set<HouseholdTaskEligibleUser>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<ShoppingCategory> ShoppingCategories => Set<ShoppingCategory>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<PurchaseWish> PurchaseWishes => Set<PurchaseWish>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<CalendarEventParticipant> CalendarEventParticipants => Set<CalendarEventParticipant>();

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
            entity.Property(user => user.ProfileColor).HasMaxLength(7).HasDefaultValue(HouseStuff.Application.Identity.ProfileColors.Default).IsRequired();
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
            entity.Property(task => task.Difficulty).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(task => new { task.ResidenceId, task.PotId, task.NormalizedName }).IsUnique();
            entity.HasIndex(task => new { task.ResidenceId, task.PotId, task.IsActive, task.NextAvailableAt });
            entity.HasAlternateKey(task => new { task.Id, task.ResidenceId });
            entity.HasOne<Pot>().WithMany()
                .HasForeignKey(task => new { task.PotId, task.ResidenceId })
                .HasPrincipalKey(pot => new { pot.Id, pot.ResidenceId })
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(task => task.EligibleUsers).WithOne()
                .HasForeignKey(user => new { user.HouseholdTaskId, user.ResidenceId })
                .HasPrincipalKey(task => new { task.Id, task.ResidenceId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HouseholdTaskEligibleUser>(entity =>
        {
            entity.ToTable("HouseholdTaskEligibleUsers");
            entity.HasKey(user => new { user.HouseholdTaskId, user.UserId });
            entity.Property(user => user.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(user => new { user.ResidenceId, user.UserId });
            entity.HasOne<HouseStuffUser>().WithMany()
                .HasForeignKey(user => user.UserId)
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

        builder.Entity<PurchaseWish>(entity =>
        {
            entity.ToTable("PurchaseWishes");
            entity.HasKey(wish => wish.Id);
            entity.Property(wish => wish.Name).HasMaxLength(120).IsRequired();
            entity.Property(wish => wish.StoreUrl).HasMaxLength(500);
            entity.HasIndex(wish => new { wish.ResidenceId, wish.Priority });
            entity.HasOne<Residence>().WithMany().HasForeignKey(wish => wish.ResidenceId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("CalendarEvents");
            entity.HasKey(calendarEvent => calendarEvent.Id);
            entity.Property(calendarEvent => calendarEvent.Title).HasMaxLength(120).IsRequired();
            entity.Property(calendarEvent => calendarEvent.Description).HasMaxLength(500);
            entity.Property(calendarEvent => calendarEvent.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(calendarEvent => calendarEvent.AllDayDate).HasColumnType("date");
            entity.HasIndex(calendarEvent => new { calendarEvent.ResidenceId, calendarEvent.AllDayDate });
            entity.HasIndex(calendarEvent => new { calendarEvent.ResidenceId, calendarEvent.StartsAt });
            entity.HasOne<Residence>().WithMany().HasForeignKey(calendarEvent => calendarEvent.ResidenceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasAlternateKey(calendarEvent => new { calendarEvent.Id, calendarEvent.ResidenceId });
            entity.HasMany(calendarEvent => calendarEvent.Participants).WithOne()
                .HasForeignKey(participant => new { participant.CalendarEventId, participant.ResidenceId })
                .HasPrincipalKey(calendarEvent => new { calendarEvent.Id, calendarEvent.ResidenceId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CalendarEventParticipant>(entity =>
        {
            entity.ToTable("CalendarEventParticipants");
            entity.HasKey(participant => new { participant.CalendarEventId, participant.UserId });
            entity.Property(participant => participant.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(participant => new { participant.ResidenceId, participant.UserId });
            entity.HasOne<HouseStuffUser>().WithMany().HasForeignKey(participant => participant.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
