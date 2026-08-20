# HOUSE-060 — Conclusão e recorrência

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O morador conclui sua tarefa ativa pela área da casa e pode sortear novamente conforme o tipo: tarefa única não retorna, reutilizável volta imediatamente e recorrente volta somente depois do intervalo configurado.

## Subtarefas

- [x] `HOUSE-060-01` — regras de conclusão e disponibilidade;
- [x] `HOUSE-060-02` — domínio, persistência e migration;
- [x] `HOUSE-060-03` — caso de uso e endpoint isolado;
- [>] `HOUSE-060-04` — experiência responsiva de conclusão;
- [ ] `HOUSE-060-05` — integração, testes e documentação.

## Decisões do recorte

- somente o usuário da atribuição ativa pode concluí-la;
- o instante de conclusão é definido pelo backend;
- tarefa única é arquivada ao concluir e nunca volta ao sorteio;
- tarefa reutilizável fica disponível imediatamente após a conclusão;
- tarefa recorrente recebe `nextAvailableAt = completedAt + recurrenceDays`;
- não existe job de liberação: sorteio e aceite comparam `nextAvailableAt` com o relógio do backend;
- a atribuição concluída permanece persistida como histórico, mas deixa de bloquear usuário e tarefa;
- calendário e tela de histórico permanecem na `HOUSE-070`.

## Critérios de aceite

- [ ] migration persiste a próxima disponibilidade da tarefa;
- [ ] conclusão registra o instante e encerra a atribuição ativa;
- [ ] tarefa única deixa de ser elegível definitivamente;
- [ ] tarefa reutilizável volta ao sorteio imediatamente;
- [ ] tarefa recorrente fica indisponível até o intervalo configurado;
- [ ] outro usuário ou outra residência não conclui a atribuição;
- [ ] interface cobre confirmação, processamento, erro e sucesso;
- [ ] fluxo é responsivo e utilizável por toque;
- [ ] builds e testes passam nos dois repositórios.
