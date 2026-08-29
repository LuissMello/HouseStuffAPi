# HOUSE-160 — Prateleira de potes no mural de tarefas

Status: concluída.

Tipo: funcional de frontend.

Repositório: front.

## Resultado observável

No mural de tarefas, o seletor tradicional é substituído por uma prateleira de potes. O nome do filtro atual aparece entre `‹‹` e `››`; as ações levam ao pote anterior ou seguinte e atualizam imediatamente os post-its exibidos.

## Subtarefas

- [x] `HOUSE-160-01` — interação, acessibilidade e critérios de aceite;
- [x] `HOUSE-160-02` — prateleira e navegação circular entre potes;
- [x] `HOUSE-160-03` — responsividade, testes, publicação e fechamento.

## Regras de interação

- “Todos os potes” participa da navegação como visão geral;
- a ordem segue a ordem já retornada pela API;
- anterior no primeiro item leva ao último, e próximo no último leva ao primeiro;
- o pote central representa o filtro ativo e os potes vizinhos ajudam a indicar continuidade;
- botões possuem nomes acessíveis com o destino e área de toque adequada;
- no celular, o pote ativo permanece legível sem rolagem horizontal;
- movimento reduzido remove transições, sem alterar a navegação.

## Critérios de aceite

- [x] dropdown deixa de ser a navegação principal do mural;
- [x] prateleira exibe o filtro atual e seus vizinhos;
- [x] `‹‹` e `››` percorrem todos os filtros circularmente;
- [x] cartões exibidos correspondem ao pote selecionado;
- [x] teclado e leitor de tela identificam ação e destino;
- [x] layout funciona em celular e desktop;
- [x] lint, testes e build do Pages passam;
- [x] versão pública pode ser testada pelo usuário.
