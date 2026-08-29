# Registro de decisões

## DEC-001 — Dois repositórios e governança central

- Status: aceita em 20/08/2026.
- Decisão: backend em `HouseStuffAPi`, frontend em `HouseStuffFront`; documentação global e tarefas ficam no backend.

## DEC-002 — Backend .NET 10 em monólito modular (substituída)

- Status: substituída pela DEC-021 em 28/08/2026.
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

## DEC-012 — Sorteio como proposta e aceite revalidado

- Status: aceita em 20/08/2026.
- Decisão: o sorteio não reserva a tarefa. A troca exclui propostas vistas apenas durante a rodada do cliente, e o aceite revalida a elegibilidade antes de persistir. Índices únicos parciais limitam uma atribuição ativa por usuário e por tarefa.

## DEC-013 — Disponibilidade calculada sem job

- Status: aceita em 20/08/2026.
- Decisão: a conclusão encerra a atribuição e atualiza a tarefa na mesma unidade de persistência. Tarefas únicas são arquivadas, reutilizáveis ficam disponíveis imediatamente e recorrentes guardam `NextAvailableAt`; sorteio e aceite fazem a comparação no momento da consulta.

## DEC-014 — Calendário residencial e histórico pessoal

- Status: aceita em 20/08/2026.
- Decisão: próximas recorrências são visíveis aos moradores da casa sem autoria; o histórico é pessoal e limitado às 50 conclusões mais recentes. A API trabalha em UTC e a interface apresenta datas no fuso do dispositivo.

## DEC-015 — Entrega local reproduzível

- Status: aceita em 20/08/2026.
- Decisão: a entrega permanece local e é orquestrada por scripts PowerShell versionados no backend. O início aguarda PostgreSQL, API e frontend; a parada usa PID e instante de início para encerrar somente processos registrados; a validação combina gates, readiness e smoke autenticado não mutável. Logs e registros de processo ficam em `.local/`, fora do Git.

## DEC-016 — Metáfora visual de post-its e potes

- Status: aceita em 20/08/2026.
- Decisão: tarefas são apresentadas como anotações em post-its e o sorteio usa potes construídos com HTML/CSS. A interação mantém textos, foco, estados da API e alternativa sem movimento; animações são uma camada de feedback e nunca uma regra de negócio.

## DEC-017 — Organização colaborativa por residência

- Status: aceita em 20/08/2026.
- Decisão: qualquer morador vinculado pode manter potes, tarefas e futuras categorias e itens de compras da própria residência. A residência continua derivada da sessão e somente a criação e administração de usuários exige o papel `Administrator`.

## DEC-018 — Lista de compras gerada sem histórico

- Status: aceita em 20/08/2026.
- Decisão: categorias e itens formam um catálogo residencial persistente, enquanto a lista atual é uma seleção temporária feita no frontend. Não há entidade de lista, estado de comprado ou histórico nesta entrega; a ordem das categorias é persistida.

## DEC-019 — Desejos colaborativos ordenados

- Status: aceita em 20/08/2026.
- Decisão: desejos de compra são compartilhados por todos os moradores e a prioridade é sua posição persistida na casa. A reordenação aceita arraste, toque, teclado e botões explícitos; itens comprados usam exclusão real, sem histórico nesta entrega.

## DEC-020 — Agenda residencial compartilhada

- Status: aceita em 20/08/2026.
- Decisão: qualquer morador mantém eventos visíveis para toda a casa. Datas e aniversários usam data civil, compromissos usam UTC e somente aniversários repetem anualmente. Participantes individuais são exclusivos em relação à opção “Todos da casa”.

## DEC-021 — Backend compatível com .NET 8

- Status: aceita em 28/08/2026.
- Decisão: o backend passa a usar .NET 8, EF Core 8 e imagens de runtime .NET 8 para permitir desenvolvimento no Visual Studio 2022 e nas máquinas atuais do projeto, preservando a arquitetura modular existente.

## DEC-022 — Elegibilidade dinâmica e dificuldade das tarefas

- Status: aceita em 28/08/2026.
- Decisão: toda tarefa possui dificuldade obrigatória e define elegibilidade para todos os moradores ou para usuários específicos. A opção “todos” é dinâmica e inclui moradores futuros; a seleção específica é persistida em vínculo próprio e validada novamente no aceite.

## DEC-023 — Administração de perfis dentro da residência

- Status: aceita em 28/08/2026.
- Decisão: um administrador pode promover ou rebaixar outro usuário vinculado à mesma residência. A operação não aceita o próprio usuário, acessos pendentes nem moradores de outra casa; o backend troca os papéis do ASP.NET Identity como unidade lógica e invalida as sessões do usuário alterado.

## DEC-024 — Prateleira como navegação do catálogo de tarefas

- Status: aceita em 28/08/2026.
- Decisão: o filtro do mural de tarefas usa a mesma metáfora visual dos potes. Uma prateleira apresenta o filtro atual entre ações anterior/próximo, com navegação circular, suporte a teclado e a opção “Todos os potes” como visão geral.

## DEC-025 — Visibilidade opcional de senhas

- Status: aceita em 29/08/2026.
- Decisão: campos de senha do login e da criação administrativa começam mascarados e oferecem ação explícita para mostrar ou ocultar o valor, sem alterar o conteúdo, autocomplete ou envio do formulário.

## DEC-026 — Execução conjunta com banco real no Visual Studio

- Status: aceita em 29/08/2026.
- Decisão: uma solução dedicada referencia a API e o frontend nos repositórios irmãos e compartilha um perfil de inicialização múltipla. O navegador permanece same-origin com o frontend, `/api` é encaminhado à API local e a API usa PostgreSQL configurado em User Secrets; o perfil desabilita o seed demonstrativo e não há mock nem segredo no repositório.
