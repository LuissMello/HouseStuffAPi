using HouseStuff.Domain.Tasks;

namespace HouseStuff.Domain.UnitTests;

public sealed class HouseholdTaskTests
{
    [Fact]
    public void CreatesRecurringTaskWithNormalizedNameAndInterval()
    {
        var result = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "  Limpar geladeira ", " Todo mês ", HouseholdTaskKind.Recurring, 30, DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Limpar geladeira", result.Task!.Name);
        Assert.Equal("LIMPAR GELADEIRA", result.Task.NormalizedName);
        Assert.Equal("Todo mês", result.Task.Description);
        Assert.Equal(30, result.Task.RecurrenceDays);
    }

    [Fact]
    public void RecurringTaskRequiresValidInterval()
    {
        var result = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Limpar geladeira", null, HouseholdTaskKind.Recurring, null, DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("recurrence_days_invalid", result.Code);
    }

    [Fact]
    public void NonRecurringTaskRejectsInterval()
    {
        var result = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Lavar louça", null, HouseholdTaskKind.Reusable, 1, DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("recurrence_not_allowed", result.Code);
    }

    [Fact]
    public void ArchiveAndReactivatePreserveTask()
    {
        var task = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Lavar louça", null, HouseholdTaskKind.Reusable, null, DateTimeOffset.UtcNow).Task!;

        task.SetActive(false, DateTimeOffset.UtcNow);
        Assert.False(task.IsActive);
        task.SetActive(true, DateTimeOffset.UtcNow);
        Assert.True(task.IsActive);
    }

    [Fact]
    public void OneTimeTaskIsArchivedAfterCompletion()
    {
        var task = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Organizar despensa", null, HouseholdTaskKind.OneTime, null, DateTimeOffset.UtcNow).Task!;

        task.RegisterCompletion(DateTimeOffset.UtcNow);

        Assert.False(task.IsActive);
        Assert.Null(task.NextAvailableAt);
    }

    [Fact]
    public void ReusableTaskIsImmediatelyAvailableAfterCompletion()
    {
        var task = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Lavar louça", null, HouseholdTaskKind.Reusable, null, DateTimeOffset.UtcNow).Task!;

        task.RegisterCompletion(DateTimeOffset.UtcNow);

        Assert.True(task.IsActive);
        Assert.Null(task.NextAvailableAt);
    }

    [Fact]
    public void RecurringTaskCalculatesNextAvailability()
    {
        var completedAt = DateTimeOffset.UtcNow;
        var task = HouseholdTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Limpar geladeira", null, HouseholdTaskKind.Recurring, 30, completedAt).Task!;

        task.RegisterCompletion(completedAt);

        Assert.True(task.IsActive);
        Assert.Equal(completedAt.AddDays(30), task.NextAvailableAt);
    }
}
