# HOUSE-040 — Cadastro de tarefas

Status: concluída em 20/08/2026.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O administrador cadastra, edita, arquiva e reativa tarefas da própria residência, vinculando cada uma a um pote e definindo se é única, reutilizável ou recorrente.

## Subtarefas

- [x] `HOUSE-040-01` — domínio, tipos de tarefa e migration;
- [x] `HOUSE-040-02` — casos de uso e endpoints isolados;
- [x] `HOUSE-040-03` — manutenção administrativa responsiva;
- [x] `HOUSE-040-04` — integração com os potes da casa;
- [x] `HOUSE-040-05` — testes, isolamento e documentação.

## Decisões do recorte

- toda tarefa pertence a uma residência e a um pote daquela mesma residência;
- tarefa possui nome, descrição opcional, tipo e estado ativo/arquivado;
- os tipos são única, reutilizável e recorrente;
- tarefa recorrente exige intervalo em dias; os demais tipos não possuem intervalo;
- nome é único dentro do pote, ignorando maiúsculas e espaços externos;
- arquivamento preserva a tarefa para histórico futuro;
- conclusão, próxima disponibilidade e sorteio permanecem em `HOUSE-050` e `HOUSE-060`;
- não existe data-alvo no cadastro.

## Critérios de aceite

- [x] migration cria tarefas e garante vínculo consistente com residência e pote;
- [x] administrador cria, edita, arquiva e reativa tarefas da própria casa;
- [x] cadastro permite selecionar pote e os três tipos de tarefa;
- [x] recorrência exige intervalo válido em dias;
- [x] API não aceita `ResidenceId` informado pelo cliente;
- [x] pote e tarefa de outra residência não aparecem nem podem ser alterados;
- [x] interface cobre loading, vazio, erro e sucesso em celular e desktop;
- [x] builds e testes passam nos dois repositórios.

## Fechamento

- Migration `AddHouseholdTasks` aplicada no PostgreSQL local com FK composta para impedir vínculo entre casas diferentes.
- Administração integrada criada em `/admin/tasks`, com seleção de pote, edição, filtro, arquivamento e reativação.
- Tipos única, reutilizável e recorrente validados; recorrência exige intervalo de 1 a 3650 dias.
- Layout validado em viewport móvel de 390 × 844 sem rolagem horizontal, com formulário e cartões em uma coluna.
- `Casa do Luis` ficou preparada com as tarefas Lavar a louça, Limpar a geladeira e Organizar a despensa.
- Isolamento real validado contra `Casa Independente` pela API e protegido também no PostgreSQL.
- Backend: 31 testes aprovados, incluindo 4 contra PostgreSQL real.
- Frontend: lint, build e 6 testes de renderização aprovados.
- Próxima tarefa proposta: `HOUSE-050`, ainda não autorizada.
