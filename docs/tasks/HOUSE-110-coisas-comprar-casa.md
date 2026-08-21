# HOUSE-110 — Coisas para comprar para a casa

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Uma pessoa da casa cadastra coisas que pretende comprar no futuro, reorganiza a prioridade arrastando os itens para cima ou para baixo e pode guardar um link opcional da loja para consultar depois.

## Subtarefas

- [x] `HOUSE-110-01` — domínio, persistência e isolamento dos desejos;
- [x] `HOUSE-110-02` — CRUD e ordenação persistida na API;
- [x] `HOUSE-110-03` — cadastro responsivo com link opcional;
- [x] `HOUSE-110-04` — lista com prioridade por arrastar e alternativa acessível;
- [x] `HOUSE-110-05` — integração, testes, evidência visual e documentação.

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

- [x] desejos possuem CRUD real em PostgreSQL e API;
- [x] vínculos e leituras de outra residência são recusados;
- [x] link de loja é opcional e validado;
- [x] usuário reordena por arrastar com toque e mouse;
- [x] nova prioridade permanece após recarregar a página;
- [x] teclado e botões de subir/descer oferecem o mesmo resultado;
- [x] link externo abre com segurança;
- [x] estados de loading, vazio, erro e sucesso estão cobertos;
- [x] fluxo completo funciona no celular e desktop;
- [x] testes, builds, documentação e tracking passam.

## Fechamento

- Resultado: moradores mantêm uma lista compartilhada de coisas para comprar, com link opcional e prioridade persistida por arraste, teclado ou botões.
- Arquivos principais: `PurchaseWish.cs`, `PurchaseWishService.cs`, `PurchaseWishesController.cs`, `app/app/wishes/page.tsx` e `app/globals.css`.
- Testes e comandos: `scripts/validate-local.ps1 -SkipStart` aprovou build sem avisos, 70 testes de backend, 16 testes de frontend, readiness e sessão real; smoke no Neon cobriu criar, listar, editar, reordenar, recusar link inseguro e excluir com administrador e morador.
- Roteiro para testar: execute `scripts/start-local.ps1 -SkipInstall`, entre em `http://localhost:3000`, abra **Desejos**, cadastre duas ideias, arraste uma delas, recarregue a página, edite o link e exclua os itens.
- Evidência visual: a rota real `/app/wishes` respondeu `200`; a tela possui composição desktop e breakpoints dedicados em `900px` e `640px`, navegação móvel com rolagem segura e puxador com `touch-action: none`.
- Pendências fora do escopo: categorias, orçamento, histórico de itens comprados e consulta automática de preço continuam deliberadamente fora desta entrega.
