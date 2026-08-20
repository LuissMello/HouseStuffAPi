# HOUSE-010 — Autenticação e administração de usuários

Status: concluída em 20/08/2026.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O administrador entra pela tela real, cria diretamente outro usuário e esse usuário consegue se autenticar e visualizar sua própria sessão.

## Subtarefas

- [x] `HOUSE-010-01` — persistência e identidade no backend;
- [x] `HOUSE-010-02` — endpoints de sessão e administração;
- [x] `HOUSE-010-03` — login e área autenticada no frontend;
- [x] `HOUSE-010-04` — criação de usuário pelo administrador;
- [x] `HOUSE-010-05` — integração, testes e documentação.

## Decisões do recorte

- ASP.NET Core Identity com cookie HTTP-only e PostgreSQL;
- administrador inicial criado somente no ambiente local;
- criação de usuários exclusivamente pelo administrador;
- sem cadastro público, convite, recuperação de senha ou provedor externo;
- associação e isolamento por residência permanecem na `HOUSE-020`.

## Critérios de aceite

- [x] banco sobe localmente e migration é aplicada;
- [x] login inválido é rejeitado sem revelar qual credencial falhou;
- [x] usuário autenticado consulta a própria sessão;
- [x] somente administrador lista e cria usuários;
- [x] frontend cobre loading, erro e sucesso;
- [x] fluxo administrador → novo usuário → login é testável no navegador;
- [x] builds e testes passam nos dois repositórios.

## Fechamento

- PostgreSQL 17 local iniciado e migration `InitialIdentity` aplicada automaticamente.
- Fluxo visual validado: erro de credencial, login administrativo, criação de `Luis`, logout, login do novo usuário e bloqueio da área administrativa.
- Backend: formatação e build aprovados; 7 testes aprovados, sem falhas.
- Frontend: lint e build aprovados; 3 testes de renderização aprovados.
- Próxima tarefa proposta: `HOUSE-020`, ainda não autorizada.
