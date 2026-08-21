# HOUSE-130 — Gestão colaborativa da casa

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Qualquer morador vinculado à casa cadastra, edita, ordena, arquiva e reativa potes e tarefas pela interface, sem depender do perfil de administrador e sem acessar dados de outra residência.

## Subtarefas

- [x] `HOUSE-130-01` — regra colaborativa e contrato de autorização;
- [>] `HOUSE-130-02` — autorização de moradores na API;
- [ ] `HOUSE-130-03` — navegação e telas compartilhadas no frontend;
- [ ] `HOUSE-130-04` — integração com Luis, testes e fechamento.

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

- [ ] endpoints de manutenção aceitam morador autenticado;
- [ ] endpoints continuam recusando acesso sem autenticação;
- [ ] serviços continuam isolando registros pela residência da sessão;
- [ ] Luis acessa as telas de potes e tarefas;
- [ ] Luis consegue criar e manter um pote e uma tarefa reais;
- [ ] navegação desktop e celular expõe Potes e Tarefas para moradores;
- [ ] rotas antigas continuam utilizáveis durante a transição;
- [ ] usuário sem residência recebe erro orientativo;
- [ ] builds e testes passam nos dois repositórios;
- [ ] documentação e tracking são atualizados.
