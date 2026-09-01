# HOUSE-310 — Caderno de compras com histórico

## Objetivo

Substituir a geração temporária da lista por um caderno compartilhado: as pendências aparecem agrupadas por categoria, podem ser marcadas e riscadas durante a compra e somente saem da lista após a confirmação em `Finalizar compra`.

## Regras

- categorias continuam persistidas e ordenáveis dentro da residência;
- cada item cadastrado representa uma pendência atual;
- marcar ou desmarcar um item antes da finalização é um estado visual local;
- finalizar exige ao menos um item marcado e é confirmado pela API;
- a API registra uma compra com data, morador e cópias dos nomes dos itens e categorias;
- os itens finalizados são removidos das pendências, sem apagar o histórico;
- itens não marcados permanecem no caderno;
- o histórico é compartilhado somente entre moradores da mesma residência e mostra as compras mais recentes primeiro.

## Critérios de aceite

- a tela abre diretamente como um caderno intitulado `Compras`;
- categorias aparecem como seções, com checkbox ao lado de cada item;
- um item marcado permanece visível e com nome riscado até a finalização;
- `Finalizar compra` envia todos os itens marcados em uma única operação;
- após sucesso, somente os itens finalizados saem das pendências;
- o histórico mostra data, morador e itens agrupados por categoria;
- API e banco impedem finalizar item inexistente ou pertencente a outra casa;
- o fluxo cabe e permanece utilizável no Safari do iPhone 12.

## Fora do escopo

- preço, quantidade e estabelecimento;
- múltiplas listas simultâneas;
- desfazer ou editar uma compra já finalizada;
- sincronizar marcações ainda não finalizadas entre aparelhos.
