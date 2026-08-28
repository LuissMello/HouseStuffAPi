# Projeto HouseStuff

Atualizado em 28/08/2026.

## Objetivo

Organizar tarefas domésticas por potes, sorteios, aceite, conclusão, recorrência e calendário. Uma casa reúne vários usuários; cada usuário pertence a somente uma casa e visualiza apenas o contexto dessa residência.

## Fluxo principal

`Login -> Escolher pote -> Sortear tarefa elegível -> Aceitar ou pedir outra -> Concluir -> Atualizar disponibilidade -> Consultar calendário/histórico`

## Escopo inicial

- criação direta de usuários pelo administrador;
- autenticação web;
- uma residência por usuário e vários usuários por residência;
- potes, tarefas únicas, reutilizáveis e recorrentes;
- sorteio com aceite ou troca;
- conclusão, calendário e histórico;
- acompanhamento do desenvolvimento dentro do frontend.

## Fora do escopo atual

- aplicativos publicados nas lojas;
- usuário em múltiplas residências;
- convites e cadastro público;
- jobs para liberar recorrência;
- microserviços ou infraestrutura distribuída.

## Princípio de entrega

Uma funcionalidade de produto só termina quando banco, API, interface e integração podem ser executados e testados pelo usuário. Partes internas podem virar subtarefas e commits menores, mas não transformam progresso parcial em funcionalidade concluída.
