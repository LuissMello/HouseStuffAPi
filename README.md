# HouseStuff API

Backend .NET 10 e fonte canônica das regras e do acompanhamento do HouseStuff.

## Estrutura

```text
src/
  HouseStuff.Api/
  HouseStuff.Application/
  HouseStuff.Domain/
  HouseStuff.Infrastructure/
tests/
  *.UnitTests/
  HouseStuff.IntegrationTests/
  HouseStuff.ArchitectureTests/
docs/
  tracking/project.json
  tasks/
```

## Executar o projeto completo

Com `HouseStuffAPi` e `HouseStuffFront` lado a lado, execute no backend:

```powershell
.\scripts\start-local.ps1
```

Acesse `http://localhost:3000`. O comando prepara o PostgreSQL, restaura o backend quando necessário, inicia API e frontend ausentes e aguarda a prontidão. Para encerrar somente os processos iniciados pelo HouseStuff:

```powershell
.\scripts\stop-local.ps1
```

Na configuração padrão, sem User Secrets, são criadas estas credenciais de demonstração:

- administrador: `admin@housestuff.local` / `HouseStuff#2026`;
- morador Luis: `luis@housestuff.local` / `LuisHouse#2026`.

Uma conexão PostgreSQL remota e uma conta administrativa podem substituir os valores de demonstração sem gravar segredos no Git:

```powershell
dotnet user-secrets set "ConnectionStrings:HouseStuff" "<connection-string>" --project src/HouseStuff.Api
dotnet user-secrets set "DevelopmentAdmin:Email" "<email>" --project src/HouseStuff.Api
dotnet user-secrets set "DevelopmentAdmin:Password" "<senha>" --project src/HouseStuff.Api
dotnet user-secrets set "DevelopmentDemo:Enabled" "false" --project src/HouseStuff.Api
```

O ambiente desta máquina está configurado dessa forma para usar o Neon; os valores reais permanecem somente no repositório de segredos do .NET.

Consulte [docs/LOCAL_DELIVERY.md](docs/LOCAL_DELIVERY.md) para pré-requisitos, logs e resolução de problemas.

## Executar somente a API

Use um SDK compatível com o `global.json`:

```powershell
docker compose up -d postgres
dotnet restore HouseStuff.slnx
dotnet run --project src/HouseStuff.Api
```

A API aplica migrations ao iniciar. Em desenvolvimento, cria o administrador `admin@housestuff.local` com a senha `HouseStuff#2026`; essas credenciais são locais e não são usadas fora do ambiente Development.

Endpoints iniciais:

- `GET /health/live`;
- `GET /health/ready`;
- `GET /api/v1/project-tracking`.
- `POST /api/v1/auth/login`;
- `POST /api/v1/auth/logout`;
- `GET /api/v1/auth/me`;
- `GET /api/v1/admin/users`;
- `POST /api/v1/admin/users`.
- `GET /api/v1/residences/current`;
- `POST /api/v1/residences`;
- `POST /api/v1/residences/current/members/{userId}`.
- `GET /api/v1/pots`;
- `GET|POST /api/v1/admin/pots`;
- `PUT /api/v1/admin/pots/{id}`;
- `PATCH /api/v1/admin/pots/{id}/status`;
- `POST /api/v1/admin/pots/{id}/move`.
- `GET|POST /api/v1/admin/tasks`;
- `PUT /api/v1/admin/tasks/{id}`;
- `PATCH /api/v1/admin/tasks/{id}/status`.
- `POST /api/v1/draws`;
- `GET /api/v1/assignments/current`;
- `POST /api/v1/assignments/accept`.
- `POST /api/v1/assignments/current/complete`.
- `GET /api/v1/routine`.

## Validar

Para validar backend, frontend, prontidão e a sessão administrativa configurada em um único roteiro:

```powershell
.\scripts\validate-local.ps1
```

Os gates isolados do backend continuam disponíveis:

```powershell
dotnet format HouseStuff.slnx --verify-no-changes --no-restore
dotnet build HouseStuff.slnx --configuration Release --no-restore
dotnet test HouseStuff.slnx --configuration Release --no-build --no-restore
```

## Atualizar acompanhamento

Edite somente `docs/tracking/project.json` e execute:

```powershell
.\scripts\generate-project-docs.ps1
```
