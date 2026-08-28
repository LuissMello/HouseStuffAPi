# HOUSE-001 — Fundação e documentação do HouseStuff

Status: concluída em 20/08/2026.

Tipo: técnica.

Repositório: ambos.

## Resultado observável

Disponibilizar uma API .NET 8 executável, um frontend web responsivo executável, documentação canônica e uma tela de acompanhamento derivada da mesma fonte do Roadmap e Status.

## Subtarefas

- [x] `HOUSE-001-01` — governança e decisões;
- [x] `HOUSE-001-02` — fundação backend;
- [x] `HOUSE-001-03` — fundação frontend e acompanhamento;
- [x] `HOUSE-001-04` — fonte única e geração documental;
- [x] `HOUSE-001-05` — validação reproduzível.

## Fora do escopo

- autenticação e usuários;
- banco e migrations;
- residência, potes, tarefas, sorteios e calendário;
- hospedagem externa.

## Critérios de aceite

- [x] repositórios oficiais receberam apenas código HouseStuff;
- [x] prefixo `HOUSE-` e entrega vertical registrados;
- [x] backend possui quatro camadas e suítes de testes;
- [x] frontend mostra etapas e tarefas de forma responsiva;
- [x] tracking possui uma única fonte editável;
- [x] API expõe o snapshot do acompanhamento;
- [x] documentação, builds e testes aplicáveis passam.

## Fechamento

- Resultado: fundações executáveis e acompanhamento visual entregues.
- Backend: build `Release` sem avisos ou erros, formatação verificada e 4 testes aprovados.
- Frontend: lint e build de produção aprovados, com teste do HTML renderizado aprovado.
- Integração: `/health/live` e `/api/v1/project-tracking` validados; tela conferida em desktop e mobile.
- Fonte única: JSON consumido pela API e cópia gerada do frontend semanticamente idênticos.
- Roteiro: consultar os READMEs dos dois repositórios.
- Próxima tarefa proposta: `HOUSE-010`, ainda não autorizada.
