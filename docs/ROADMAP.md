# Roadmap

> Arquivo gerado por `scripts/generate-project-docs.ps1`. Edite somente `docs/tracking/project.json`.

Atualizado em: 2026-09-01.

## M0 — Fundação

Projetos executáveis, governança e acompanhamento em fonte única

- [x] `HOUSE-001` — Fundação e documentação do HouseStuff — API .NET 8 e frontend executáveis, documentação canônica e tela de acompanhamento conectada à fonte única.

## M1 — Acesso

Administrador cria usuários e cada pessoa consegue entrar

- [x] `HOUSE-010` — Autenticação e administração de usuários — Administrador cria um usuário e a pessoa entra pela tela real.

## M2 — Residência

Uma casa reúne vários usuários com isolamento seguro

- [x] `HOUSE-020` — Residência e isolamento dos dados — Usuários veem apenas os dados da própria casa.

## M3 — Potes

Potes podem ser criados e organizados

- [x] `HOUSE-030` — Cadastro e organização dos potes — Administrador mantém potes e usuários visualizam os potes disponíveis.

## M4 — Tarefas

Tarefas únicas, reutilizáveis e recorrentes podem ser cadastradas

- [x] `HOUSE-040` — Cadastro de tarefas — Tarefas únicas, reutilizáveis e recorrentes são mantidas pela interface.

## M5 — Sorteio

Usuário escolhe um pote, aceita a tarefa sorteada ou pede outra

- [x] `HOUSE-050` — Sorteio, aceite e troca de tarefa — Usuário escolhe um pote, vê uma tarefa elegível, aceita ou pede outra.

## M6 — Rotina

Conclusões e próximas disponibilidades são registradas

- [x] `HOUSE-060` — Conclusão e recorrência — Conclusão encerra tarefa única ou calcula a próxima disponibilidade.

## M7 — Calendário

Usuário acompanha disponibilidade e histórico da própria casa

- [x] `HOUSE-070` — Calendário e histórico — Usuário consulta calendário e histórico reais da própria casa.

## M8 — Entrega

Ambiente local reproduzível, responsivo e validado ponta a ponta

- [x] `HOUSE-080` — Qualidade e entrega local — Projeto completo inicia por roteiro único e possui validação ponta a ponta.

## M9 — Experiência

Tarefas em post-its e sorteio animado tornam a rotina mais leve e divertida

- [x] `HOUSE-090` — Experiência lúdica e sorteio animado — Usuário anota tarefas em post-its e acompanha o pote escolhido entregar a proposta em uma animação.

## M10 — Compras

A casa organiza compras recorrentes e desejos futuros por prioridade

- [x] `HOUSE-100` — Lista de compras por categorias — Usuário mantém categorias e itens e gera uma lista com seleção individual ou por categoria.
- [x] `HOUSE-110` — Coisas para comprar para a casa — Usuário mantém desejos da casa, ordena por prioridade e guarda um link opcional de loja.

## M11 — Agenda

A casa visualiza datas, aniversários e compromissos em um calendário diário, semanal ou mensal

- [x] `HOUSE-120` — Agenda e calendário da casa — Usuário cadastra eventos com participantes e consulta a agenda real nos modos diário, semanal e mensal.

## M12 — Colaboração

Todos os moradores organizam juntos os potes, tarefas e futuras listas da casa

- [x] `HOUSE-130` — Gestão colaborativa da casa — Qualquer morador mantém potes e tarefas da própria casa pela interface.

## M13 — Distribuição

Tarefas respeitam esforço desejado e quem pode realizá-las

- [x] `HOUSE-140` — Dificuldade e moradores elegíveis nas tarefas — Morador cadastra post-it com dificuldade e pessoas elegíveis e sorteia somente tarefas adequadas para si.

## M14 — Administração

Perfis administrativos podem ser ajustados com segurança dentro da casa

- [x] `HOUSE-150` — Alteração de perfil dos moradores — Administrador promove outro morador ou o rebaixa pela tela de pessoas da casa.

## M15 — Navegação

O mural usa os próprios potes para navegar entre as tarefas

- [x] `HOUSE-160` — Prateleira de potes no mural de tarefas — Morador troca o filtro do mural navegando visualmente pelos potes da prateleira.

## M16 — Acessibilidade

Senhas podem ser conferidas antes do envio sem perder a proteção padrão

- [x] `HOUSE-170` — Mostrar e ocultar senha — Usuário confere a senha digitada no login e na criação de acessos.

## M17 — Desenvolvimento

API e frontend iniciam juntos no Visual Studio contra PostgreSQL real

- [x] `HOUSE-180` — Execução conjunta no Visual Studio — Desenvolvedor abre uma solução, inicia API e frontend juntos e testa dados persistidos em PostgreSQL real.

## M18 — Roteamento

A navegação por hash funciona no GitHub Pages e no desenvolvimento local

- [x] `HOUSE-190` — Navegação por hash no ambiente local — Links e redirecionamentos por hash abrem as telas corretas no Visual Studio sem quebrar o GitHub Pages.

## M19 — Sessão móvel

O login publicado funciona mesmo quando o navegador bloqueia cookies entre sites

- [x] `HOUSE-200` — Login compatível com navegadores móveis — Morador entra pelo GitHub Pages no celular sem depender da aceitação de cookies de terceiros.

## M20 — Interface móvel

Cadastros são mais simples e o calendário cabe integralmente na largura do celular

- [x] `HOUSE-210` — Cadastro simplificado e calendário móvel compacto — Morador cadastra uma tarefa sem escolher comportamento e visualiza o mês inteiro sem rolagem lateral no celular.

## M21 — Página inicial

A página da casa mostra somente conteúdo útil para a rotina atual

- [x] `HOUSE-220` — Remoção dos atalhos da página da casa — Morador acessa a página da casa sem o bloco redundante de atalhos administrativos.

## M22 — Navegação móvel

Os menus usam nomes claros e consistentes em todas as telas

- [x] `HOUSE-230` — Calendário no menu móvel — Morador encontra o calendário pelo mesmo nome nos menus desktop e mobile.

## M23 — Identidade visual

Cada morador escolhe uma cor que o identifica em toda a casa

- [x] `HOUSE-240` — Cor pessoal dos moradores — Cada morador escolhe uma cor persistida e é reconhecido por ela em avatares, tarefas e calendário.

## M24 — Consulta do calendário

Eventos abrem primeiro para leitura e só entram em edição por uma ação explícita

- [x] `HOUSE-250` — Visualização de eventos antes da edição — Morador consulta os detalhes de um evento em modo somente leitura e escolhe explicitamente quando quer alterá-lo.

## M25 — Documentação da API

A API publicada oferece documentação OpenAPI interativa com suporte a autenticação

- [x] `HOUSE-260` — Swagger público da API — Desenvolvedor acessa a documentação interativa da API publicada e autentica chamadas protegidas com token Bearer.

## M26 — Post-its em andamento

Moradores acumulam post-its reservados, acompanham todos os seus itens e concluem cada um separadamente

- [x] `HOUSE-270` — Reserva e acompanhamento de múltiplos post-its — Morador aceita vários post-its, cada item fica indisponível para toda a casa e todos os itens em andamento aparecem juntos.

## M27 — Compras em andamento

A lista da ida ao mercado mostra somente o que ainda falta comprar

- [x] `HOUSE-280` — Baixa de itens comprados na lista — Morador marca itens comprados e a lista mantém visíveis somente os itens que ainda faltam.

## M28 — Compatibilidade iPhone

As telas principais permanecem utilizáveis no Safari do iPhone 12 sem estouros, zoom ou sobreposição

- [x] `HOUSE-290` — Compatibilidade responsiva com iPhone 12 — Andressa utiliza as telas principais no Safari do iPhone 12 com navegação, formulários e conteúdo ajustados ao viewport de 390 por 844 pixels.

