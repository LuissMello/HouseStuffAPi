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
- o administrador cria, edita, ordena, arquiva e reativa potes da própria casa;
- moradores visualizam somente potes ativos da própria residência;
- arquivar preserva o pote para o histórico e para vínculos futuros;
- o usuário escolhe explicitamente um pote, como `Mensal`;
- o sorteio considera somente tarefas elegíveis daquele pote e da residência do usuário;
- uma tarefa pode ser única, reutilizável ou recorrente;
- tarefa única concluída nunca retorna ao pote;
- tarefa reutilizável pode voltar a ficar disponível conforme sua política;
- tarefa recorrente possui um intervalo configurado.

## BR-003 — Sorteio e aceite

- o resultado sorteado é uma proposta, não uma atribuição aceita automaticamente;
- o usuário pode aceitar a tarefa ou pedir outra;
- somente o aceite cria a atribuição ativa;
- as regras de nova tentativa, exclusão temporária e ausência de tarefas serão fechadas na tarefa `HOUSE-050`.

## BR-004 — Conclusão e recorrência

- concluir uma tarefa registra usuário e instante;
- uma tarefa recorrente recebe `nextAvailableAt = completedAt + recurrenceInterval`;
- não existe job para liberar recorrência;
- consultas e sorteios consideram a tarefa elegível quando `nextAvailableAt` for menor ou igual ao instante atual;
- o backend usa seu próprio relógio para decidir elegibilidade.

## Pendências para as tarefas correspondentes

- tratamento de uma tarefa recusada durante o mesmo sorteio;
- política de tarefas reutilizáveis sem intervalo;
- possibilidade e regras de mudança de residência;
- timezone usado para exibição do calendário.
