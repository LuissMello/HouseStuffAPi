# HOUSE-120 — Agenda e calendário da casa

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Uma pessoa cadastra datas, aniversários e compromissos, informa quais moradores estão envolvidos ou marca o evento para todos, e a casa consulta tudo em um calendário real nos modos diário, semanal e mensal.

## Subtarefas

- [x] `HOUSE-120-01` — domínio, participantes, datas e persistência;
- [x] `HOUSE-120-02` — CRUD de eventos e consultas por período na API;
- [x] `HOUSE-120-03` — cadastro responsivo com seleção de envolvidos;
- [x] `HOUSE-120-04` — calendário diário, semanal e mensal;
- [x] `HOUSE-120-05` — integração com recorrências, testes, evidência visual e documentação.

## Escopo

- CRUD de datas, aniversários e compromissos da residência;
- título, tipo, data e descrição opcional;
- eventos de dia inteiro e compromissos com horário de início e fim;
- seleção de um ou vários moradores envolvidos;
- opção exclusiva “Todos da casa”;
- identificação visual de quem está envolvido em cada evento;
- calendário verdadeiro com modos diário, semanal e mensal;
- navegação para período anterior, próximo e hoje;
- destaque do dia atual;
- consulta eficiente por intervalo visível;
- exibição conjunta dos eventos e das próximas recorrências de tarefas, com distinção visual;
- preservação do histórico pessoal já existente em `HOUSE-070`;
- isolamento completo entre residências;
- experiência mobile-first para consulta e cadastro.

## Fora do escopo

- integração com Google Calendar, Outlook ou calendário do aparelho;
- convites por e-mail;
- videoconferência, localização por mapa ou anexos;
- notificações e lembretes automáticos, até solicitação explícita;
- recorrências personalizadas complexas, além do aniversário anual.

## Regras propostas

- todo evento pertence a uma única residência;
- todos os moradores da residência podem visualizar os eventos da casa e quem está envolvido;
- o evento deve selecionar pelo menos um morador ou “Todos da casa”;
- “Todos da casa” não é combinado com participantes individuais;
- participantes só podem ser usuários vinculados à mesma residência do evento;
- aniversários são eventos de dia inteiro e se repetem anualmente;
- datas de dia inteiro são persistidas como data civil, sem conversão que possa trocar o dia;
- compromissos com horário são persistidos em UTC e exibidos no fuso do dispositivo;
- horário final, quando informado, precisa ser posterior ao horário inicial;
- consultas recebem apenas o intervalo visível; residência vem sempre da sessão;
- eventos e recorrências de tarefas usam estilos diferentes no calendário.

## Decisões aprovadas para a implementação

- qualquer morador vinculado pode criar, editar e excluir eventos da própria casa;
- somente aniversários possuem recorrência anual nesta etapa;
- todos os eventos são visíveis para a casa inteira e mostram claramente os moradores envolvidos.

## Critérios de aceite

- [x] eventos e participantes possuem persistência real no PostgreSQL;
- [x] CRUD e consulta por período derivam a residência da sessão;
- [x] participantes de outra residência são recusados;
- [x] cadastro permite data, aniversário e compromisso;
- [x] usuário seleciona indivíduos ou todos da casa;
- [x] calendário mostra claramente para quem é cada evento;
- [x] modos diário, semanal e mensal apresentam uma grade ou linha do tempo real;
- [x] navegação anterior, próxima e hoje funciona nos três modos;
- [x] eventos e recorrências aparecem no dia e horário corretos;
- [x] histórico pessoal de conclusões continua disponível;
- [x] estados de loading, vazio, erro e sucesso estão cobertos;
- [x] fluxo completo funciona no celular e desktop;
- [x] testes, builds, documentação e tracking passam.

## Fechamento

- Resultado: moradores mantêm datas, aniversários anuais e compromissos compartilhados e consultam eventos e recorrências em calendário diário, semanal ou mensal, sem perder o histórico pessoal.
- Arquivos principais: `CalendarEvent.cs`, `CalendarService.cs`, `CalendarController.cs`, `CalendarEventForm.tsx`, `CalendarBoard.tsx` e `app/app/routine/page.tsx`.
- Testes e comandos: `scripts/validate-local.ps1 -SkipStart` aprovou build sem avisos, 80 testes de backend, 18 testes de frontend, readiness e sessão; smoke no Neon cobriu os três tipos, aniversário anual, horário inválido, visibilidade e edição por dois moradores e limpeza final.
- Roteiro para testar: execute `scripts/start-local.ps1 -SkipInstall`, entre em `http://localhost:3000`, abra **Calendário**, cadastre uma data para todos, um aniversário para um morador e um compromisso com horário; alterne Dia/Semana/Mês, navegue pelos períodos, edite pelo cartão e exclua no `×`.
- Evidência visual: `/app/routine` respondeu `200`; mês usa grade de sete colunas, semana usa colunas diárias, dia usa linha do tempo de 24 horas e os breakpoints móveis mantêm controles touch, rolagem horizontal das grades e navegação inferior.
- Pendências fora do escopo: integrações externas, convites, lembretes automáticos, anexos e recorrências personalizadas continuam fora desta entrega.
