using HouseStuff.Application.Notifications;
using HouseStuff.Domain.Calendar;
using HouseStuff.Domain.Notifications;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Notifications;

internal sealed class DailyDigestService(HouseStuffDbContext database, IUserNotifier notifier, TimeProvider timeProvider) : IDailyDigestService
{
    // O Brasil não observa horário de verão desde 2019; o gatilho roda uma vez por dia num
    // horário fixo, então um offset constante é suficiente (sem precisar de um provedor de fuso).
    private static readonly TimeSpan BrazilOffset = TimeSpan.FromHours(-3);

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime((timeProvider.GetUtcNow() + BrazilOffset).UtcDateTime);
        var residenceIds = await database.Residences.Select(residence => residence.Id).ToListAsync(cancellationToken);

        var notified = 0;
        foreach (var residenceId in residenceIds)
        {
            var alreadyRun = await database.DigestRuns.AnyAsync(run => run.ResidenceId == residenceId && run.Date == today, cancellationToken);
            if (!alreadyRun)
            {
                var message = await BuildMessageAsync(residenceId, today, cancellationToken);
                if (message is not null)
                {
                    await SendToResidenceAsync(residenceId, message, cancellationToken);
                    notified++;
                }

                database.DigestRuns.Add(DigestRun.Create(residenceId, today));
                await database.SaveChangesAsync(cancellationToken);
            }
        }

        return notified;
    }

    private async Task<string?> BuildMessageAsync(Guid residenceId, DateOnly today, CancellationToken cancellationToken)
    {
        var dayOfWeek = (int)today.DayOfWeek;
        var weekEnd = today.AddDays(dayOfWeek == 0 ? 0 : 7 - dayOfWeek);
        var weekMonthDays = new HashSet<(int Month, int Day)>();
        for (var day = today; day <= weekEnd; day = day.AddDays(1))
        {
            weekMonthDays.Add((day.Month, day.Day));
        }

        var lines = new List<string>();

        var birthdays = await database.CalendarEvents
            .Where(item => item.ResidenceId == residenceId && item.Kind == CalendarEventKind.Birthday && item.AllDayDate != null)
            .Select(item => new { item.Title, item.AllDayDate })
            .ToListAsync(cancellationToken);
        var birthdaysToday = birthdays.Where(item => item.AllDayDate!.Value.Month == today.Month && item.AllDayDate.Value.Day == today.Day).Select(item => item.Title).ToList();
        var birthdaysThisWeekCount = birthdays.Count(item => weekMonthDays.Contains((item.AllDayDate!.Value.Month, item.AllDayDate.Value.Day)));
        if (birthdaysToday.Count > 0)
        {
            lines.Add($"🎂 Aniversário hoje: {string.Join(", ", birthdaysToday)}");
        }

        // Npgsql só aceita DateTimeOffset com offset zero para colunas timestamptz.
        var weekStartUtc = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), BrazilOffset).ToUniversalTime();
        var weekEndUtc = new DateTimeOffset(weekEnd.ToDateTime(TimeOnly.MaxValue), BrazilOffset).ToUniversalTime();
        var dateEventsThisWeek = await database.CalendarEvents.CountAsync(
            item => item.ResidenceId == residenceId && item.Kind == CalendarEventKind.Date && item.AllDayDate != null && item.AllDayDate >= today && item.AllDayDate <= weekEnd,
            cancellationToken);
        var appointmentsThisWeek = await database.CalendarEvents.CountAsync(
            item => item.ResidenceId == residenceId && item.Kind == CalendarEventKind.Appointment && item.StartsAt != null && item.StartsAt >= weekStartUtc && item.StartsAt <= weekEndUtc,
            cancellationToken);
        var totalThisWeek = dateEventsThisWeek + appointmentsThisWeek + birthdaysThisWeekCount;
        if (totalThisWeek > 0)
        {
            lines.Add($"📅 {totalThisWeek} {(totalThisWeek == 1 ? "compromisso" : "compromissos")} essa semana");
        }

        var pendingShoppingCount = await database.ShoppingItems.CountAsync(item => item.ResidenceId == residenceId, cancellationToken);
        if (pendingShoppingCount > 0)
        {
            lines.Add($"🛒 {pendingShoppingCount} {(pendingShoppingCount == 1 ? "item pendente" : "itens pendentes")} na lista de compras");
        }

        var weekAgo = timeProvider.GetUtcNow().AddDays(-7);
        var stalePostits = await (from assignment in database.TaskAssignments
                                  join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
                                  join user in database.Users on assignment.AssignedToUserId equals user.Id
                                  where task.ResidenceId == residenceId && assignment.CompletedAt == null && assignment.AcceptedAt <= weekAgo
                                  select user.Name)
            .ToListAsync(cancellationToken);
        if (stalePostits.Count > 0)
        {
            lines.Add($"📌 Post-its parados há mais de 1 semana: {string.Join(", ", stalePostits)}");
        }

        return lines.Count == 0 ? null : string.Join("\n", lines);
    }

    private async Task SendToResidenceAsync(Guid residenceId, string body, CancellationToken cancellationToken)
    {
        var userIds = await database.Users.Where(user => user.ResidenceId == residenceId).Select(user => user.Id).ToListAsync(cancellationToken);
        await notifier.NotifyAsync(userIds, "Resumo do dia", body, cancellationToken);
    }
}
