# HOUSE-030 — Cadastro e organização dos potes

Status: concluída em 20/08/2026.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O administrador cria, edita, ordena, arquiva e reativa potes da própria residência; moradores visualizam somente os potes ativos da sua casa.

## Subtarefas

- [x] `HOUSE-030-01` — domínio, persistência e migration;
- [x] `HOUSE-030-02` — casos de uso e endpoints isolados;
- [x] `HOUSE-030-03` — manutenção administrativa dos potes;
- [x] `HOUSE-030-04` — visualização dos potes pelos moradores;
- [x] `HOUSE-030-05` — integração, isolamento e documentação.

## Decisões do recorte

- pote pertence obrigatoriamente a uma residência;
- nome é único dentro da residência, ignorando maiúsculas e espaços externos;
- pote possui nome, descrição opcional, ordem e estado ativo/arquivado;
- moradores veem apenas potes ativos;
- arquivamento preserva o pote para tarefas e histórico futuros;
- cadastro de tarefas permanece na `HOUSE-040`.

## Critérios de aceite

- [x] migration cria potes com chave estrangeira para residência e unicidade local;
- [x] administrador cria e edita pote da própria casa;
- [x] administrador reordena, arquiva e reativa potes;
- [x] morador visualiza somente potes ativos da própria casa;
- [x] API não aceita `ResidenceId` informado pelo cliente;
- [x] pote de outra residência não aparece nem pode ser alterado;
- [x] telas cobrem vazio, loading, erro e sucesso;
- [x] builds e testes passam nos dois repositórios.

## Fechamento

- Migration `AddPots` aplicada no PostgreSQL local, com FK para residência e índice único por nome normalizado dentro da casa.
- Administração integrada criada em `/admin/pots`, com cadastro, edição, arquivamento, reativação e ordenação.
- Área `/app` exibe apenas potes ativos; arquivamento foi validado no navegador e ocultou imediatamente o pote da visão comum.
- Layout validado em viewport móvel de 390 × 844 sem rolagem horizontal; formulário, lista e cartões ficam em uma coluna e as ações têm alvos touch.
- `Casa do Luis` ficou preparada com os potes Diário, Semanal e Mensal para acompanhamento local.
- Backend: build e 23 testes aprovados, incluindo 3 contra PostgreSQL real.
- Frontend: lint, build e 5 testes de renderização aprovados.
- Próxima tarefa proposta: `HOUSE-040`, ainda não autorizada.
