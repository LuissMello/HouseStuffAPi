# Entrega local

O HouseStuff é entregue como aplicação web local responsiva. Não há publicação em lojas ou hospedagem externa neste recorte.

## Pré-requisitos

- Windows com PowerShell;
- Docker Desktop com Docker Compose;
- .NET SDK compatível com `global.json`;
- Node.js `>=22.13.0` e npm;
- `HouseStuffAPi` e `HouseStuffFront` lado a lado no mesmo diretório.

## Iniciar

No diretório `HouseStuffAPi`:

```powershell
.\scripts\start-local.ps1
```

O comando inicia o PostgreSQL, restaura dependências quando necessário, inicia somente API/frontend ausentes e espera `http://localhost:5049/health/ready` e `http://localhost:3000` responderem. Execuções repetidas preservam os processos já registrados.

## Acessar

- aplicação: `http://localhost:3000`;
- liveness: `http://localhost:5049/health/live`;
- readiness com PostgreSQL: `http://localhost:5049/health/ready`;
- administrador: `admin@housestuff.local` / `HouseStuff#2026`;
- Luis: `luis@housestuff.local` / `LuisHouse#2026`.

Em um banco Development vazio, a aplicação aplica migrations e cria de forma idempotente a Casa do Luis, os dois usuários, três potes e tarefas dos três tipos.

## Validar

```powershell
.\scripts\validate-local.ps1
```

O roteiro verifica formatação, build e testes do backend; lint, build e testes do frontend; readiness; resposta da aplicação; login, residência, potes e rotina do Luis. O smoke não sorteia, aceita ou conclui tarefas.

Use `-SkipStart` somente quando os serviços já estiverem em execução.

## Parar

```powershell
.\scripts\stop-local.ps1
```

O comando encerra apenas API/frontend registrados em `.local/processes.json`, validando também o instante de início para evitar PID reutilizado. O PostgreSQL permanece disponível por padrão. Para pará-lo junto:

```powershell
.\scripts\stop-local.ps1 -StopDatabase
```

## Resolução de problemas

- consulte `.local/api.stderr.log`, `.local/api.stdout.log`, `.local/frontend.stderr.log` e `.local/frontend.stdout.log`;
- confirme que as portas `3000`, `5049` e `54329` estão livres ou pertencem aos serviços esperados;
- execute `docker compose ps` para conferir a saúde do PostgreSQL;
- se dependências mudarem, execute novamente sem `-SkipInstall`;
- readiness indisponível com liveness ativo indica que a API iniciou, mas não alcança o PostgreSQL.
