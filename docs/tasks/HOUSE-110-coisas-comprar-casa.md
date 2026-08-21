# HOUSE-110 — Coisas para comprar para a casa

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Uma pessoa da casa cadastra coisas que pretende comprar no futuro, reorganiza a prioridade arrastando os itens para cima ou para baixo e pode guardar um link opcional da loja para consultar depois.

## Subtarefas

- [x] `HOUSE-110-01` — domínio, persistência e isolamento dos desejos;
- [x] `HOUSE-110-02` — CRUD e ordenação persistida na API;
- [x] `HOUSE-110-03` — cadastro responsivo com link opcional;
- [x] `HOUSE-110-04` — lista com prioridade por arrastar e alternativa acessível;
- [>] `HOUSE-110-05` — integração, testes, evidência visual e documentação.

## Escopo

- CRUD básico de coisas para comprar para a residência;
- nome obrigatório e link de loja opcional;
- prioridade representada pela posição do item na lista;
- reordenação no frontend arrastando para cima ou para baixo;
- suporte a toque no celular e mouse no desktop;
- alternativa acessível por teclado e ações explícitas de subir/descer;
- persistência da nova ordem na API;
- abertura segura do link externo cadastrado;
- isolamento completo entre residências.

## Fora do escopo

- consulta de preço, imagem ou estoque no Mercado Livre ou em outras lojas;
- afiliados, carrinho ou compra dentro do HouseStuff;
- monitoramento automático de preços;
- categorias e orçamento para desejos, até solicitação explícita.

## Regras propostas

- cada desejo pertence a uma única residência;
- prioridade é uma ordem estável, sem empates, definida dentro da residência;
- reordenar um item atualiza as posições afetadas na mesma operação;
- link é opcional e, quando informado, aceita somente endereço HTTP ou HTTPS válido;
- links externos abrem em nova aba com proteção contra acesso à janela de origem;
- nenhuma operação aceita `ResidenceId` enviado pelo cliente.
- qualquer morador vinculado pode criar, editar, excluir e reordenar desejos da própria casa;
- um item comprado é removido pelo CRUD; não há estado adquirido ou histórico nesta entrega.

## Decisões aprovadas para a implementação

- qualquer morador vinculado mantém os desejos compartilhados da casa;
- item comprado pode ser excluído; arquivamento e histórico ficam fora do escopo.

## Critérios de aceite

- [ ] desejos possuem CRUD real em PostgreSQL e API;
- [ ] vínculos e leituras de outra residência são recusados;
- [ ] link de loja é opcional e validado;
- [ ] usuário reordena por arrastar com toque e mouse;
- [ ] nova prioridade permanece após recarregar a página;
- [ ] teclado e botões de subir/descer oferecem o mesmo resultado;
- [ ] link externo abre com segurança;
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
