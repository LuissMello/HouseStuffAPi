# HOUSE-040 — Cadastro de tarefas

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O administrador cadastra, edita, arquiva e reativa tarefas da própria residência, vinculando cada uma a um pote e definindo se é única, reutilizável ou recorrente.

## Subtarefas

- [>] `HOUSE-040-01` — domínio, tipos de tarefa e migration;
- [ ] `HOUSE-040-02` — casos de uso e endpoints isolados;
- [ ] `HOUSE-040-03` — manutenção administrativa responsiva;
- [ ] `HOUSE-040-04` — integração com os potes da casa;
- [ ] `HOUSE-040-05` — testes, isolamento e documentação.

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

- [ ] migration cria tarefas e garante vínculo consistente com residência e pote;
- [ ] administrador cria, edita, arquiva e reativa tarefas da própria casa;
- [ ] cadastro permite selecionar pote e os três tipos de tarefa;
- [ ] recorrência exige intervalo válido em dias;
- [ ] API não aceita `ResidenceId` informado pelo cliente;
- [ ] pote e tarefa de outra residência não aparecem nem podem ser alterados;
- [ ] interface cobre loading, vazio, erro e sucesso em celular e desktop;
- [ ] builds e testes passam nos dois repositórios.
