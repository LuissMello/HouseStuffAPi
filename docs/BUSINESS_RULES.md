# Regras de negócio

## BR-001 — Residência e usuários

- uma residência pode conter vários usuários;
- um usuário pertence a somente uma residência;
- usuários são criados diretamente pelo administrador;
- após o login, o usuário acessa somente dados relacionados à própria residência e às próprias atribuições;
- isolamento residencial é validado pelo backend e reforçado no banco.
- o administrador sem residência cria uma casa e é associado automaticamente;
- um usuário criado por administrador que já possui casa entra diretamente naquela residência;
- acessos antigos sem casa podem ser associados pelo administrador;
- administradores listam membros da própria casa e acessos ainda sem residência, nunca membros de outra casa;
- mudança ou remoção de residência não está disponível no escopo inicial.

## BR-002 — Potes e tarefas

- cada pote pertence a uma única residência e seu nome é único dentro dela;
- qualquer morador vinculado cria, edita, ordena, arquiva e reativa potes da própria casa;
- moradores visualizam somente potes ativos da própria residência;
- arquivar preserva o pote para o histórico e para vínculos futuros;
- o usuário escolhe explicitamente um pote, como `Mensal`;
- o sorteio considera somente tarefas elegíveis daquele pote e da residência do usuário;
- uma tarefa pode ser única, reutilizável ou recorrente;
- toda tarefa pertence a exatamente um pote da mesma residência;
- o nome da tarefa é único dentro do pote, ignorando maiúsculas e espaços externos;
- qualquer morador vinculado cria, edita, move, arquiva e reativa tarefas entre potes ativos da própria casa;
- tarefas podem ser arquivadas e reativadas sem apagar seu registro;
- tarefa única concluída nunca retorna ao pote;
- tarefa reutilizável volta a ficar disponível imediatamente após a conclusão;
- tarefa recorrente possui um intervalo configurado.
- apenas tarefa recorrente possui intervalo, entre 1 e 3650 dias;
- tarefas não possuem data-alvo no cadastro inicial.

## BR-003 — Sorteio e aceite

- o resultado sorteado é uma proposta, não uma atribuição aceita automaticamente;
- o usuário pode aceitar a tarefa ou pedir outra;
- somente o aceite cria a atribuição ativa;
- pedir outra exclui no dispositivo as tarefas já vistas durante a rodada atual;
- ao esgotar uma rodada, o usuário pode reiniciá-la;
- propostas não são reservadas e podem ficar indisponíveis antes do aceite;
- o aceite revalida a tarefa e o vínculo com a residência autenticada;
- cada usuário e cada tarefa podem participar de no máximo uma atribuição ativa.

## BR-004 — Conclusão e recorrência

- concluir uma tarefa registra usuário e instante;
- uma tarefa recorrente recebe `nextAvailableAt = completedAt + recurrenceInterval`;
- não existe job para liberar recorrência;
- consultas e sorteios consideram a tarefa elegível quando `nextAvailableAt` for menor ou igual ao instante atual;
- o backend usa seu próprio relógio para decidir elegibilidade.

## BR-005 — Calendário e histórico

- o calendário lista recorrências futuras da residência autenticada sem identificar o morador que concluiu;
- tarefas já disponíveis permanecem no fluxo de sorteio e não aparecem como evento futuro;
- o histórico mostra somente as próprias conclusões do usuário autenticado;
- a consulta inicial retorna as 50 conclusões mais recentes;
- instantes são persistidos e transportados em UTC e exibidos no fuso do dispositivo.

## BR-006 — Categorias, itens e lista de compras

- qualquer morador vinculado mantém categorias e itens da própria residência;
- categorias possuem nome único e ordem persistida dentro da residência;
- cada item pertence a exatamente uma categoria da mesma residência;
- nomes de itens são únicos dentro da categoria, ignorando maiúsculas e espaços externos;
- nenhuma operação recebe `ResidenceId` do cliente; a residência é derivada da sessão;
- categoria com itens não pode ser excluída antes de ser esvaziada;
- a lista é gerada temporariamente a partir do catálogo, sem histórico ou múltiplas listas salvas;
- selecionar uma categoria seleciona todos os seus itens, e a seleção individual atualiza o estado total, parcial ou vazio da categoria;
- selecionar significa incluir na lista atual, não registrar a compra do item.

## BR-007 — Desejos de compra da casa

- qualquer morador vinculado cria, edita, exclui e reordena desejos da própria residência;
- cada desejo possui nome obrigatório, link HTTP/HTTPS opcional e prioridade representada por sua posição;
- a ordem é estável dentro da residência e uma reordenação persiste todas as posições afetadas;
- a API aceita somente a lista completa de identificadores da residência ao reordenar;
- nenhuma operação recebe `ResidenceId` do cliente;
- links externos abrem em nova aba sem acesso à janela de origem;
- item comprado pode ser excluído; estado adquirido, arquivamento e histórico ficam fora do escopo.

## BR-008 — Agenda e calendário da casa

- qualquer morador vinculado cria, edita e exclui eventos da própria residência;
- eventos podem ser datas de dia inteiro, aniversários anuais ou compromissos com horário;
- todo evento seleciona moradores da casa ou a opção exclusiva “Todos da casa”;
- todos os moradores veem a agenda compartilhada e para quem cada evento foi marcado;
- participantes de outra residência são recusados e nenhuma operação recebe `ResidenceId` do cliente;
- datas civis são persistidas sem conversão de fuso; compromissos usam instantes UTC e horário final posterior ao inicial;
- apenas aniversários se repetem nesta etapa;
- consultas recebem o intervalo visível e combinam eventos com próximas recorrências de tarefas, mantendo estilos distintos;
- o histórico de tarefas concluídas continua pessoal.

## Pendências para as tarefas correspondentes

- possibilidade e regras de mudança de residência;
