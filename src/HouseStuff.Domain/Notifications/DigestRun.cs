namespace HouseStuff.Domain.Notifications;

/// <summary>
/// Marca que o resumo diário já foi processado para uma residência num dia, para o gatilho
/// externo (GitHub Actions) poder ser chamado mais de uma vez sem duplicar notificações.
/// </summary>
public sealed class DigestRun
{
    private DigestRun(Guid residenceId, DateOnly date)
    {
        ResidenceId = residenceId;
        Date = date;
    }

    private DigestRun()
    {
    }

    public Guid ResidenceId { get; private set; }
    public DateOnly Date { get; private set; }

    public static DigestRun Create(Guid residenceId, DateOnly date) => new(residenceId, date);
}
