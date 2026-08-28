namespace HouseStuff.Infrastructure.Identity;

// A porta abre antes das migrations para o proxy do Fly não desistir da máquina no cold start,
// então o readiness precisa de um sinal próprio para só liberar tráfego depois da inicialização.
public sealed class StartupState
{
    private volatile bool ready;

    public bool IsReady => ready;

    public void MarkReady() => ready = true;
}
