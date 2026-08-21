# HOUSE-080 — Qualidade e entrega local

Status: em andamento.

Tipo: técnico.

Repositório: ambos.

## Resultado observável

Uma pessoa prepara e inicia o HouseStuff localmente por um comando, recebe dados demonstráveis, verifica a saúde do sistema e executa a validação completa por um roteiro único.

## Subtarefas

- [x] `HOUSE-080-01` — contrato de entrega e critérios operacionais;
- [x] `HOUSE-080-02` — prontidão e dados locais reproduzíveis;
- [x] `HOUSE-080-03` — comandos de iniciar, parar e validar;
- [x] `HOUSE-080-04` — smoke autenticado e auditoria responsiva;
- [>] `HOUSE-080-05` — gates, documentação e fechamento.

## Decisões do recorte

- a entrega permanece local, sem publicação em loja ou hospedagem externa;
- PostgreSQL continua em Docker e API/frontend usam os runtimes locais já definidos;
- um ambiente Development novo recebe dados demonstráveis idempotentes da Casa do Luis;
- `/health/live` verifica o processo e `/health/ready` verifica também o PostgreSQL;
- os comandos locais gravam somente logs e identificadores de processo ignorados pelo Git;
- o smoke autenticado não altera dados de negócio;
- a validação final cobre rotas públicas, morador e administração em desktop e celular.

## Critérios de aceite

- [x] banco vazio inicia com administrador, Luis, residência, potes e tarefas de demonstração;
- [x] readiness falha quando o banco não pode ser acessado;
- [x] um comando inicia banco, API e frontend e aguarda prontidão;
- [x] um comando encerra somente os processos registrados pelo HouseStuff;
- [x] um comando executa gates e smoke autenticado;
- [ ] documentação contém pré-requisitos, credenciais e resolução de problemas;
- [x] rotas críticas passam em auditoria responsiva;
- [ ] builds e testes passam nos dois repositórios.

## Evidências da validação

- banco PostgreSQL temporário vazio recebeu migrations, administrador, Luis, Casa do Luis, três potes e tarefas; o banco foi removido após o teste;
- `start-local.ps1` preservou processos já registrados e iniciou somente os serviços ausentes;
- `stop-local.ps1` encerrou a API registrada e manteve o frontend que já estava em execução;
- `validate-local.ps1 -SkipStart` aprovou os gates e o smoke autenticado não mutável do Luis;
- `/`, `/login`, `/app`, `/app/routine`, `/admin/users`, `/admin/pots` e `/admin/tasks` foram verificadas em 390 × 844 e 1280 × 720, sem overflow horizontal e sem erros de aplicação no console.
