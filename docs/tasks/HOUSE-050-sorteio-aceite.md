# HOUSE-050 — Sorteio, aceite e troca de tarefa

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O morador escolhe um pote da própria casa, recebe uma tarefa elegível, pode pedir outra e cria uma atribuição ativa somente quando aceita a proposta.

## Subtarefas

- [x] `HOUSE-050-01` — regras de elegibilidade e persistência da atribuição;
- [x] `HOUSE-050-02` — casos de uso e endpoints de sorteio e aceite;
- [x] `HOUSE-050-03` — experiência responsiva de escolha e proposta;
- [x] `HOUSE-050-04` — atribuição ativa integrada à área da casa;
- [x] `HOUSE-050-05` — testes, isolamento e documentação.

## Decisões do recorte

- o usuário escolhe explicitamente um pote ativo da própria residência;
- são elegíveis tarefas ativas daquele pote que não estejam em outra atribuição ativa;
- o sorteio é uma proposta e não reserva nem atribui a tarefa;
- pedir outra exclui as propostas já vistas durante aquela rodada no dispositivo;
- quando a rodada esgota as opções, o usuário pode reiniciá-la;
- o aceite revalida a elegibilidade e cria a atribuição persistida;
- cada usuário possui no máximo uma atribuição ativa;
- uma tarefa possui no máximo uma atribuição ativa entre todos os moradores;
- uma proposta pode deixar de ser elegível antes do aceite; nesse caso o usuário sorteia novamente;
- conclusão e liberação por recorrência permanecem na `HOUSE-060`.

## Critérios de aceite

- [x] migration cria atribuições com vínculos consistentes a usuário e tarefa, enquanto a residência é validada pela sessão e pelo vínculo da tarefa;
- [x] sorteio considera somente tarefas elegíveis do pote e da casa autenticada;
- [x] pedir outra não cria atribuição e evita repetir tarefa durante a rodada;
- [x] aceite cria atribuição ativa somente após nova validação;
- [x] usuário com atribuição ativa não sorteia nem aceita outra;
- [x] tarefa ativa já atribuída não pode ser atribuída a outra pessoa;
- [x] interface cobre loading, vazio, proposta, troca, aceite e atribuição ativa;
- [x] fluxo é responsivo e utilizável por toque;
- [x] builds e testes passam nos dois repositórios.

## Validação funcional

- login do morador Luis em viewport de 390 × 844;
- escolha do pote Mensal e sorteio de `Limpar a geladeira`;
- troca para `Organizar a despensa` sem atribuição antecipada;
- aceite e permanência da atribuição após recarregar a aplicação;
- tentativa de novo sorteio bloqueada para Luis;
- sorteio do administrador retornou somente a tarefa mensal ainda disponível;
- próxima entrega: `HOUSE-060`, ainda não autorizada.
