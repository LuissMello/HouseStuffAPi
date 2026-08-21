# HOUSE-120 — Agenda e calendário da casa

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Uma pessoa cadastra datas, aniversários e compromissos, informa quais moradores estão envolvidos ou marca o evento para todos, e a casa consulta tudo em um calendário real nos modos diário, semanal e mensal.

## Subtarefas

- [x] `HOUSE-120-01` — domínio, participantes, datas e persistência;
- [>] `HOUSE-120-02` — CRUD de eventos e consultas por período na API;
- [ ] `HOUSE-120-03` — cadastro responsivo com seleção de envolvidos;
- [ ] `HOUSE-120-04` — calendário diário, semanal e mensal;
- [ ] `HOUSE-120-05` — integração com recorrências, testes, evidência visual e documentação.

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

- [ ] eventos e participantes possuem persistência real no PostgreSQL;
- [ ] CRUD e consulta por período derivam a residência da sessão;
- [ ] participantes de outra residência são recusados;
- [ ] cadastro permite data, aniversário e compromisso;
- [ ] usuário seleciona indivíduos ou todos da casa;
- [ ] calendário mostra claramente para quem é cada evento;
- [ ] modos diário, semanal e mensal apresentam uma grade ou linha do tempo real;
- [ ] navegação anterior, próxima e hoje funciona nos três modos;
- [ ] eventos e recorrências aparecem no dia e horário corretos;
- [ ] histórico pessoal de conclusões continua disponível;
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
