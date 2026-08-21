# HOUSE-130 — Gestão colaborativa da casa

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Qualquer morador vinculado à casa cadastra, edita, ordena, arquiva e reativa potes e tarefas pela interface, sem depender do perfil de administrador e sem acessar dados de outra residência.

## Subtarefas

- [x] `HOUSE-130-01` — regra colaborativa e contrato de autorização;
- [x] `HOUSE-130-02` — autorização de moradores na API;
- [x] `HOUSE-130-03` — navegação e telas compartilhadas no frontend;
- [x] `HOUSE-130-04` — integração com Luis, testes e fechamento.

## Escopo

- permitir que todo usuário vinculado a uma residência mantenha potes;
- permitir que todo usuário vinculado a uma residência mantenha tarefas dos potes;
- manter criação e administração de usuários exclusivas do administrador;
- apresentar Potes e Tarefas na navegação de qualquer morador;
- disponibilizar rotas de organização dentro da área da casa;
- substituir textos de “administração” por linguagem colaborativa nessas telas;
- manter isolamento por residência em todas as operações;
- registrar que categorias e itens da `HOUSE-100` também serão colaborativos.

## Fora do escopo

- implementar categorias ou lista de compras da `HOUSE-100`;
- permitir que moradores criem usuários ou associem pessoas à casa;
- papéis intermediários ou permissões configuráveis;
- auditoria de quem alterou cada pote ou tarefa.

## Regras

- qualquer morador autenticado e vinculado pode criar e manter potes e tarefas da própria residência;
- o backend continua derivando a residência exclusivamente da sessão;
- usuário sem residência não pode manter potes ou tarefas;
- administração de usuários permanece restrita ao papel `Administrator`;
- as futuras categorias e itens de compra seguirão a mesma regra colaborativa.

## Critérios de aceite

- [x] endpoints de manutenção aceitam morador autenticado;
- [x] endpoints continuam recusando acesso sem autenticação;
- [x] serviços continuam isolando registros pela residência da sessão;
- [x] Luis acessa as telas de potes e tarefas;
- [x] Luis consegue criar e manter um pote e uma tarefa reais;
- [x] navegação desktop e celular expõe Potes e Tarefas para moradores;
- [x] rotas antigas continuam utilizáveis durante a transição;
- [x] usuário sem residência recebe erro orientativo;
- [x] builds e testes passam nos dois repositórios;
- [x] documentação e tracking são atualizados.

## Evidência de conclusão

- API de potes e tarefas protegida por autenticação, sem exigir o papel de administrador;
- rotas compartilhadas `/app/pots` e `/app/tasks`, com aliases administrativos preservados;
- navegação desktop e inferior móvel disponível para todo morador autenticado;
- smoke local com `luis@housestuff.local`: criação, leitura e arquivamento de pote e tarefa concluídos;
- acesso de Luis a `/api/v1/admin/users` continuou retornando `403`;
- lint, build e 12 testes do frontend aprovados, além dos gates do backend.
