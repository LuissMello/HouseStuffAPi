# HouseStuff — instruções permanentes

Este é o repositório oficial do backend e da documentação global. `HouseStuffFront` é o frontend oficial. `BolaoChappoApi` e `BolaoChappoFront` são apenas referências e nunca devem ser alterados por tarefas do HouseStuff.

## Contexto obrigatório

Antes de alterar código, ler `PROJECT.md`, `ARCHITECTURE.md`, `BUSINESS_RULES.md`, `STANDARDS.md`, `STATUS.md`, a tarefa `HOUSE-XXX` autorizada e o código relacionado.

## Trabalho por tarefas

- executar uma tarefa `HOUSE-XXX` por vez;
- subtarefas `HOUSE-XXX-YY` podem dividir implementação e commits;
- não iniciar automaticamente a tarefa seguinte;
- tarefa técnica pode ser exclusivamente backend/frontend, mas precisa de resultado verificável;
- tarefa funcional só termina quando banco, API, interface e integração aplicáveis podem ser executados e testados;
- endpoint sem tela ou tela com mock é progresso parcial, não funcionalidade concluída;
- não inventar regra de negócio nem ampliar escopo silenciosamente.

## Arquitetura backend

- .NET 10 e ASP.NET Core Web API;
- monólito modular com `Api`, `Application`, `Domain` e `Infrastructure`;
- Controllers finos; Application Services orquestram; Domain protege invariantes; Infrastructure persiste;
- módulos não acessam repository, Entity ou tabela de outro módulo sem contrato explícito;
- falhas esperadas usam Result Pattern; exceptions ficam para falhas inesperadas;
- PostgreSQL/EF Core e migrations entram com a primeira necessidade de persistência;
- todo comportamento backend nasce com teste; repositories recebem testes contra PostgreSQL real.

## Tracking

- `docs/tracking/project.json` é a única fonte editável;
- `ROADMAP.md`, `STATUS.md` e `HouseStuffFront/app/data/project.generated.json` são gerados;
- nunca editar manualmente arquivos gerados;
- IDs usam somente `HOUSE-XXX` e `HOUSE-XXX-YY`.

## Gates

```powershell
dotnet restore HouseStuff.slnx
dotnet format HouseStuff.slnx --verify-no-changes --no-restore
dotnet build HouseStuff.slnx --configuration Release --no-restore
dotnet test HouseStuff.slnx --configuration Release --no-build --no-restore
```

## Fechamento

Informar resultado observável, arquivos, testes, comandos, pendências e roteiro para o usuário executar. Parar após a tarefa atual.
