# HOUSE-020 — Residência e isolamento dos dados

Status: concluída em 20/08/2026.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

O administrador cria a residência, associa usuários pendentes e cada pessoa autenticada visualiza somente a própria casa e seus moradores.

## Subtarefas

- [x] `HOUSE-020-01` — domínio, vínculo único e migration;
- [x] `HOUSE-020-02` — casos de uso e endpoints isolados;
- [x] `HOUSE-020-03` — criação e visualização da residência;
- [x] `HOUSE-020-04` — associação administrativa de moradores;
- [x] `HOUSE-020-05` — integração, isolamento e documentação.

## Decisões do recorte

- cada usuário possui no máximo um `ResidenceId`, protegido por chave estrangeira;
- o administrador sem residência cria a própria casa e é associado automaticamente;
- usuários criados depois disso entram diretamente na casa do administrador;
- usuários antigos ainda sem casa aparecem como pendentes e podem ser associados;
- administradores enxergam membros da própria casa e usuários ainda pendentes, nunca membros de outra residência;
- mudança ou remoção de residência não faz parte desta entrega.

## Critérios de aceite

- [x] migration cria residência e vínculo opcional no usuário;
- [x] criar uma segunda residência para o mesmo usuário é rejeitado;
- [x] associar usuário já vinculado é rejeitado;
- [x] leitura da residência deriva exclusivamente da sessão autenticada;
- [x] interface cobre usuário sem casa, criação, associação e casa existente;
- [x] dois usuários vinculados visualizam a mesma residência;
- [x] outro administrador não visualiza membros de residência alheia;
- [x] builds e testes passam nos dois repositórios.

## Fechamento

- Migration `AddResidences` aplicada sobre o PostgreSQL local sem perda dos usuários existentes.
- `Casa do Luis` criada com Administrador e Luis; `Casa Independente` criada com outra administradora e uma moradora.
- Isolamento validado no navegador e na API: a segunda administradora visualiza somente os dois acessos de sua casa.
- Segunda residência para o mesmo usuário rejeitada com HTTP 400.
- Backend: formatação e build aprovados; 15 testes aprovados, incluindo 2 contra PostgreSQL real.
- Frontend: lint e build aprovados; 4 testes de renderização aprovados.
- Próxima tarefa proposta: `HOUSE-030`, ainda não autorizada.
