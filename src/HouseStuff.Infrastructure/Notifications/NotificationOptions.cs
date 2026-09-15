namespace HouseStuff.Infrastructure.Notifications;

/// <summary>Par de chaves VAPID usado para autenticar o servidor perante os provedores de push. Não são segredo de usuário — vêm de Fly secrets / user-secrets.</summary>
public sealed class VapidOptions
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

/// <summary>Chave compartilhada que autoriza o gatilho externo (GitHub Actions) a disparar o resumo diário.</summary>
public sealed class DigestTriggerOptions
{
    public string Key { get; set; } = string.Empty;
}
