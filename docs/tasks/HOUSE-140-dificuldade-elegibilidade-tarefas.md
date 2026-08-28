# HOUSE-140 — Dificuldade e moradores elegíveis nas tarefas

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Ao cadastrar ou editar um post-it, o morador escolhe sua dificuldade e define se todos ou somente pessoas específicas da casa podem pegá-lo. No sorteio, cada pessoa escolhe a dificuldade desejada e recebe somente tarefas permitidas para ela, com a animação do novo post-it dobrando e entrando no pote após o cadastro.

## Subtarefas

- [x] `HOUSE-140-01` — regras, contrato e critérios de aceite;
- [>] `HOUSE-140-02` — domínio, persistência e migration;
- [ ] `HOUSE-140-03` — API de cadastro e sorteio filtrado;
- [ ] `HOUSE-140-04` — cadastro responsivo e animação do post-it;
- [ ] `HOUSE-140-05` — integração, testes, documentação e fechamento.

## Regras

- toda tarefa possui dificuldade obrigatória: fácil, média ou difícil;
- novas tarefas começam em dificuldade média quando um cliente antigo não informar o campo;
- a tarefa pode ser disponibilizada para todos os moradores ou para uma seleção não vazia de moradores da mesma casa;
- “todos os moradores” inclui pessoas que forem adicionadas à casa futuramente;
- a API recusa moradores de outra residência e nunca recebe `ResidenceId` do cliente;
- o sorteio aceita dificuldade opcional; sem filtro, considera qualquer dificuldade;
- sorteio e aceite confirmam que o usuário autenticado está entre os moradores elegíveis;
- troca de post-it mantém pote e filtro de dificuldade durante a rodada;
- o cadastro concluído anima o post-it dobrando e entrando no pote, respeitando `prefers-reduced-motion`.

## Critérios de aceite

- [ ] banco persiste dificuldade e elegibilidade por usuário com isolamento residencial;
- [ ] cadastro e edição retornam dificuldade e moradores elegíveis;
- [ ] tarefa específica nunca é sorteada nem aceita por morador não selecionado;
- [ ] seleção “todos” funciona para moradores atuais e futuros;
- [ ] sorteio permite qualquer dificuldade, fácil, média ou difícil;
- [ ] formulário funciona por toque, teclado e leitor de tela;
- [ ] animação ocorre somente após confirmação real da API;
- [ ] telas continuam responsivas no celular;
- [ ] builds e testes dos dois repositórios passam;
- [ ] fluxo integrado pode ser executado pelo usuário.
