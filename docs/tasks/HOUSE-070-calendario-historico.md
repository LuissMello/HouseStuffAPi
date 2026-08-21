# HOUSE-070 — Calendário e histórico

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O usuário abre uma área responsiva de rotina para consultar as próximas tarefas recorrentes disponíveis na própria casa e suas 50 conclusões mais recentes.

## Subtarefas

- [x] `HOUSE-070-01` — regras e contratos de consulta;
- [x] `HOUSE-070-02` — consultas e endpoints isolados;
- [x] `HOUSE-070-03` — tela responsiva e navegação;
- [x] `HOUSE-070-04` — integração, estados e isolamento;
- [>] `HOUSE-070-05` — testes e documentação.

## Decisões do recorte

- o calendário lista somente tarefas recorrentes da residência autenticada com `nextAvailableAt` no futuro;
- tarefas disponíveis agora continuam no fluxo de sorteio e não ocupam o calendário;
- o calendário da casa não revela qual morador concluiu a tarefa;
- o histórico lista somente atribuições concluídas pelo usuário autenticado;
- o histórico inicial contém as 50 conclusões mais recentes, em ordem decrescente;
- instantes são armazenados e enviados em UTC e exibidos no fuso do dispositivo;
- não há data-alvo nem edição pelo calendário;
- filtros avançados e paginação ficam fora deste recorte inicial.

## Critérios de aceite

- [ ] consulta deriva usuário e residência exclusivamente da sessão;
- [ ] calendário retorna somente recorrências futuras da própria casa;
- [ ] histórico retorna somente conclusões do usuário autenticado;
- [ ] resultados possuem ordem determinística;
- [ ] interface cobre loading, vazio, erro e dados reais;
- [ ] navegação autenticada inclui a nova área;
- [ ] fluxo é responsivo e utilizável por toque;
- [ ] builds e testes passam nos dois repositórios.
