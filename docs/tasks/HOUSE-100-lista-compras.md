# HOUSE-100 — Lista de compras por categorias

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Uma pessoa da casa mantém categorias e um catálogo de itens, escolhe gerar uma lista de compras e monta a lista agrupando e ordenando por categoria, com seleção individual ou da categoria inteira.

## Subtarefas

- [x] `HOUSE-100-01` — domínio, persistência e isolamento de categorias e itens;
- [>] `HOUSE-100-02` — CRUD de categorias e itens na API;
- [ ] `HOUSE-100-03` — cadastro responsivo de categorias e itens;
- [ ] `HOUSE-100-04` — geração, ordenação e seleção da lista;
- [ ] `HOUSE-100-05` — integração, testes, evidência visual e documentação.

## Escopo

- CRUD básico de categorias da residência;
- CRUD básico de itens vinculados a uma categoria da mesma residência;
- acesso separado para “Cadastrar item” e “Gerar lista de compras”;
- geração da lista a partir dos itens cadastrados;
- visualização agrupada e ordenável por categoria;
- selecionar ou desselecionar uma categoria inteira;
- selecionar ou desselecionar um único item sem alterar os demais;
- estados claros para categoria parcialmente selecionada;
- isolamento completo entre residências;
- interface mobile-first adequada ao uso durante as compras.

## Fora do escopo

- integração com supermercados, preços ou estoque;
- leitura de código de barras;
- comparação de valores;
- compartilhamento público da lista;
- histórico de listas, até aprovação explícita.

## Regras propostas

- qualquer morador vinculado pode manter categorias e itens de compra da própria residência;
- categorias pertencem a uma residência e têm nome único dentro dela;
- cada item pertence a exatamente uma categoria da mesma residência;
- nomes de itens são únicos dentro da categoria, ignorando maiúsculas e espaços externos;
- selecionar categoria seleciona todos os seus itens visíveis; desselecionar remove todos;
- seleção individual atualiza o estado total, parcial ou vazio da categoria;
- ordenação por categoria deve ser persistida ou derivada de uma ordem estável definida na casa;
- nenhuma operação aceita `ResidenceId` enviado pelo cliente.
- a lista gerada é uma seleção temporária do catálogo e não cria histórico ou múltiplas listas salvas;
- marcar um item significa incluí-lo na compra, não registrar que já foi comprado;
- categorias e itens usam exclusão real; categoria com itens precisa ser esvaziada antes da exclusão.

## Decisões aprovadas para a implementação

- não haverá lista persistida ou múltiplas listas salvas nesta entrega;
- a seleção serve apenas para compor a lista atual; estado de item comprado fica fora do escopo.

## Critérios de aceite

- [ ] categorias e itens possuem CRUD real em PostgreSQL e API;
- [ ] vínculos cruzados entre residências são recusados;
- [ ] usuário alterna entre cadastro e geração da lista;
- [ ] lista pode ser ordenada por categoria;
- [ ] categoria inteira pode ser selecionada ou desselecionada;
- [ ] item individual pode ser selecionado ou desselecionado;
- [ ] seleção parcial da categoria é apresentada corretamente;
- [ ] estados de loading, vazio, erro e sucesso estão cobertos;
- [ ] fluxo completo funciona no celular e desktop;
- [ ] testes, builds, documentação e tracking passam.

## Fechamento

- Resultado:
- Arquivos principais:
- Testes e comandos:
- Roteiro para testar:
- Evidência visual:
- Pendências fora do escopo:
