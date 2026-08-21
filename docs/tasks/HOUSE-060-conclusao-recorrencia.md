# HOUSE-060 — Conclusão e recorrência

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O morador conclui sua tarefa ativa pela área da casa e pode sortear novamente conforme o tipo: tarefa única não retorna, reutilizável volta imediatamente e recorrente volta somente depois do intervalo configurado.

## Subtarefas

- [x] `HOUSE-060-01` — regras de conclusão e disponibilidade;
- [x] `HOUSE-060-02` — domínio, persistência e migration;
- [x] `HOUSE-060-03` — caso de uso e endpoint isolado;
- [x] `HOUSE-060-04` — experiência responsiva de conclusão;
- [x] `HOUSE-060-05` — integração, testes e documentação.

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

- [x] migration persiste a próxima disponibilidade da tarefa;
- [x] conclusão registra o instante e encerra a atribuição ativa;
- [x] tarefa única deixa de ser elegível definitivamente;
- [x] tarefa reutilizável volta ao sorteio imediatamente;
- [x] tarefa recorrente fica indisponível até o intervalo configurado;
- [x] outro usuário ou outra residência não conclui a atribuição;
- [x] interface cobre confirmação, processamento, erro e sucesso;
- [x] fluxo é responsivo e utilizável por toque;
- [x] builds e testes passam nos dois repositórios.

## Validação funcional

- login do morador Luis em viewport de 390 × 844;
- cancelamento e confirmação da conclusão de `Organizar a despensa`;
- tarefa única removida dos sorteios após concluir;
- `Limpar a geladeira` concluída com retorno exibido em 19/09/2026 e bloqueada até essa data;
- `Lavar a louça` concluída e sorteada novamente de imediato;
- administrador recebeu `active_assignment_not_found` ao tentar concluir a tarefa ativa de Luis;
- recarga confirmou ausência de atribuição ativa e liberação para novo sorteio;
- próxima entrega: `HOUSE-070`, ainda não autorizada.
