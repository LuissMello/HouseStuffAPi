# Registro de decisões

## DEC-001 — Dois repositórios e governança central

- Status: aceita em 20/08/2026.
- Decisão: backend em `HouseStuffAPi`, frontend em `HouseStuffFront`; documentação global e tarefas ficam no backend.

## DEC-002 — Backend .NET 10 em monólito modular

- Status: aceita em 20/08/2026.
- Decisão: ASP.NET Core Web API, Clean Architecture em quatro camadas e módulos por domínio.

## DEC-003 — Frontend web único

- Status: aceita em 20/08/2026.
- Decisão: uma aplicação React/TypeScript/Vite responsiva, sem Android/iOS de loja e sem aplicação administrativa separada.

## DEC-004 — Entrega funcional vertical

- Status: aceita em 20/08/2026.
- Decisão: uma funcionalidade só é concluída com backend, frontend, integração e roteiro reproduzível. Subtarefas podem ser concluídas sem encerrar a tarefa-pai.

## DEC-005 — Acompanhamento em fonte única

- Status: aceita em 20/08/2026.
- Decisão: `docs/tracking/project.json` gera Roadmap/Status e alimenta a tela do frontend.

## DEC-006 — Etapas sem datas artificiais

- Status: aceita em 20/08/2026.
- Decisão: o roadmap é organizado por resultados e dependências; datas só serão adicionadas quando houver compromisso real.

## DEC-007 — Modelo residencial inicial

- Status: aceita em 20/08/2026.
- Decisão: uma residência possui vários usuários e cada usuário pertence a apenas uma residência.

## DEC-008 — Autenticação local e criação administrativa

- Status: aceita em 20/08/2026.
- Decisão: ASP.NET Core Identity persiste usuários no PostgreSQL e mantém a sessão em cookie HTTP-only. Não existe cadastro público; o administrador cria os usuários diretamente. Um administrador inicial com credenciais conhecidas é criado apenas no ambiente local.

## DEC-009 — Residência derivada da sessão

- Status: aceita em 20/08/2026.
- Decisão: cada usuário possui no máximo uma `ResidenceId`. Endpoints de leitura não recebem o identificador da residência; eles o obtêm do usuário autenticado. Administradores podem ver sua própria residência e acessos ainda pendentes para associação, mas não usuários de outra casa.

## DEC-010 — Potes arquiváveis e ordenação touch

- Status: aceita em 20/08/2026.
- Decisão: potes possuem nome único por residência, descrição opcional, ordem e estado ativo/arquivado. Arquivamento preserva referências; moradores veem apenas ativos. A ordenação usa ações explícitas de subir/descer, adequadas ao uso majoritário em celular e acessíveis sem gesto de arrastar.

## DEC-011 — Catálogo de tarefas e vínculo composto

- Status: aceita em 20/08/2026.
- Decisão: tarefas são únicas, reutilizáveis ou recorrentes; somente recorrentes recebem intervalo em dias. Cada tarefa guarda `ResidenceId` e `PotId`, protegidos por FK composta, e seu nome é único dentro do pote. Arquivamento substitui exclusão física no fluxo administrativo inicial.
