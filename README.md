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

## Executar

Use um SDK compatível com o `global.json`:

```powershell
docker compose up -d postgres
dotnet restore HouseStuff.slnx
dotnet run --project src/HouseStuff.Api
```

A API aplica migrations ao iniciar. Em desenvolvimento, cria o administrador `admin@housestuff.local` com a senha `HouseStuff#2026`; essas credenciais são locais e não são usadas fora do ambiente Development.

Endpoints iniciais:

- `GET /health/live`;
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

## Validar

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
