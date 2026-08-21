# HOUSE-070 — Calendário e histórico

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O usuário abre uma área responsiva de rotina para consultar as próximas tarefas recorrentes disponíveis na própria casa e suas 50 conclusões mais recentes.

## Subtarefas

- [x] `HOUSE-070-01` — regras e contratos de consulta;
- [x] `HOUSE-070-02` — consultas e endpoints isolados;
- [x] `HOUSE-070-03` — tela responsiva e navegação;
- [x] `HOUSE-070-04` — integração, estados e isolamento;
- [x] `HOUSE-070-05` — testes e documentação.

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

- [x] consulta deriva usuário e residência exclusivamente da sessão;
- [x] calendário retorna somente recorrências futuras da própria casa;
- [x] histórico retorna somente conclusões do usuário autenticado;
- [x] resultados possuem ordem determinística;
- [x] interface cobre loading, vazio, erro e dados reais;
- [x] navegação autenticada inclui a nova área;
- [x] fluxo é responsivo e utilizável por toque;
- [x] builds e testes passam nos dois repositórios.

## Validação funcional

- Luis visualizou `Limpar a geladeira` disponível em 19/09/2026;
- o histórico de Luis mostrou quatro conclusões reais em ordem decrescente;
- a conta da Casa Independente recebeu calendário e histórico vazios, sem dados da Casa do Luis;
- viewport de 390 × 844 validado sem rolagem horizontal;
- navegação inferior permitiu alternar entre Casa e Rotina no celular;
- próxima entrega: `HOUSE-080`, ainda não autorizada.
