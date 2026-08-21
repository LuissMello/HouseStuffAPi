# HOUSE-080 — Qualidade e entrega local

Status: em andamento.

Tipo: técnico.

Repositório: ambos.

## Resultado observável

Uma pessoa prepara e inicia o HouseStuff localmente por um comando, recebe dados demonstráveis, verifica a saúde do sistema e executa a validação completa por um roteiro único.

## Subtarefas

- [x] `HOUSE-080-01` — contrato de entrega e critérios operacionais;
- [>] `HOUSE-080-02` — prontidão e dados locais reproduzíveis;
- [ ] `HOUSE-080-03` — comandos de iniciar, parar e validar;
- [ ] `HOUSE-080-04` — smoke autenticado e auditoria responsiva;
- [ ] `HOUSE-080-05` — gates, documentação e fechamento.

## Decisões do recorte

- a entrega permanece local, sem publicação em loja ou hospedagem externa;
- PostgreSQL continua em Docker e API/frontend usam os runtimes locais já definidos;
- um ambiente Development novo recebe dados demonstráveis idempotentes da Casa do Luis;
- `/health/live` verifica o processo e `/health/ready` verifica também o PostgreSQL;
- os comandos locais gravam somente logs e identificadores de processo ignorados pelo Git;
- o smoke autenticado não altera dados de negócio;
- a validação final cobre rotas públicas, morador e administração em desktop e celular.

## Critérios de aceite

- [ ] banco vazio inicia com administrador, Luis, residência, potes e tarefas de demonstração;
- [ ] readiness falha quando o banco não pode ser acessado;
- [ ] um comando inicia banco, API e frontend e aguarda prontidão;
- [ ] um comando encerra somente os processos registrados pelo HouseStuff;
- [ ] um comando executa gates e smoke autenticado;
- [ ] documentação contém pré-requisitos, credenciais e resolução de problemas;
- [ ] rotas críticas passam em auditoria responsiva;
- [ ] builds e testes passam nos dois repositórios.
