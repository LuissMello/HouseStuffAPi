# Arquitetura

## Visão geral

```text
HouseStuffFront (React/Vite)
          |
          v
HouseStuff.Api (.NET 10) -> Application -> Domain
          |                    ^
          +-> Infrastructure --+
                    |
               PostgreSQL
```

## Backend

O backend é um monólito modular com quatro projetos:

- `HouseStuff.Api`: HTTP, autenticação, autorização, contratos e composição;
- `HouseStuff.Application`: casos de uso e contratos internos;
- `HouseStuff.Domain`: entidades, value objects e regras;
- `HouseStuff.Infrastructure`: EF Core, PostgreSQL, repositories e integrações.

Dependências apontam para dentro: Domain não referencia outras camadas; Application referencia Domain; Infrastructure implementa contratos internos; Api compõe Application e Infrastructure.

Módulos planejados: Identity, Residences, Pots, Tasks, Draws, Completions, Calendar e Administration.

## Frontend

Aplicação web responsiva única, sem apps de loja. A base usa React, TypeScript strict e Vite/Vinext. O código será organizado por feature, com cliente de API centralizado, TanStack Query para estado remoto e React Hook Form/Zod para formulários quando as features começarem.

## Dados e autenticação

Recomendação aprovada: solução mais simples possível. A direção inicial é ASP.NET Core Identity no backend, autenticação por cookie seguro em implantação same-origin e PostgreSQL via EF Core. A implementação pertence a `HOUSE-010`.

Em `HOUSE-020`, `Residences` passou a ser o agregado de contexto residencial. O usuário do Identity possui uma única chave estrangeira opcional `ResidenceId`; toda leitura de residência parte do identificador da sessão, sem aceitar um ID de casa fornecido pelo cliente.

Em `HOUSE-030`, `Pots` adotou o mesmo limite: o contexto residencial é resolvido pela sessão em `ICurrentResidenceContext`, e nenhum endpoint recebe `ResidenceId`. A unicidade do nome por residência e a chave estrangeira também são garantidas no PostgreSQL. O frontend é mobile-first, usa cartões e controles de ordem por botões, sem depender de arrastar em telas touch.

Em `HOUSE-040`, `HouseholdTasks` pertence ao pote por uma chave estrangeira composta `{PotId, ResidenceId}`. Além de o serviço derivar a casa da sessão, essa composição impede no banco que uma tarefa declare uma residência e aponte para o pote de outra. O tipo é persistido como texto e a recorrência em dias só existe para tarefas recorrentes.

Em `HOUSE-050`, `TaskAssignments` registra a tarefa e o usuário que a aceitou. Índices únicos parciais garantem no PostgreSQL no máximo uma atribuição ativa por usuário e por tarefa. Sorteio, consulta atual e aceite derivam usuário e residência da sessão; o aceite repete a validação de elegibilidade porque a proposta não cria reserva.

Em `HOUSE-060`, a própria atribuição recebe `CompletedAt` e permanece como registro histórico. A tarefa guarda `NextAvailableAt`: nulo para reutilizáveis, data calculada para recorrentes e arquivamento para únicas. Não há processo agendado; sorteio e aceite avaliam a disponibilidade contra o relógio do backend.

Em `HOUSE-070`, a consulta de rotina combina duas projeções somente leitura: próximas recorrências da residência e atribuições concluídas pelo usuário. O identificador da casa e do usuário vem da sessão; a API não recebe filtros de identidade do cliente.

Em `HOUSE-100`, `ShoppingCategories` possui ordem e unicidade por residência, enquanto `ShoppingItems` usa chave estrangeira composta `{CategoryId, ResidenceId}`. Essa composição impede no PostgreSQL que um item de uma casa seja ligado à categoria de outra; os nomes normalizados garantem unicidade por categoria.

Em `HOUSE-110`, `PurchaseWishes` pertence diretamente à residência e mantém uma prioridade inteira estável dentro dela. A chave estrangeira com exclusão em cascata impede desejos órfãos, enquanto todo acesso funcional deriva a casa da sessão e nunca aceita `ResidenceId` do cliente.

## Entrega local

Em `HOUSE-080`, o ambiente Development ganhou um cenário demonstrável idempotente da Casa do Luis. A API separa liveness (`/health/live`) de readiness (`/health/ready`), que inclui a conexão PostgreSQL. Scripts PowerShell no backend orquestram PostgreSQL, API e frontend, registram somente os processos que iniciam e oferecem um smoke autenticado não mutável junto aos gates dos dois repositórios.

## Acompanhamento

`docs/tracking/project.json` é a única fonte editável. Ele gera `ROADMAP.md` e `STATUS.md`, é copiado para o frontend durante desenvolvimento/build e também é exposto por `GET /api/v1/project-tracking`.
