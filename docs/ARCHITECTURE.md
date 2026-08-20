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

## Acompanhamento

`docs/tracking/project.json` é a única fonte editável. Ele gera `ROADMAP.md` e `STATUS.md`, é copiado para o frontend durante desenvolvimento/build e também é exposto por `GET /api/v1/project-tracking`.
