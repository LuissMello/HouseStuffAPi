# Ambientes

## Local

- frontend e API executam separadamente durante o desenvolvimento;
- nenhuma credencial real é versionada;
- PostgreSQL local executa via `docker compose` na porta `54329`;
- migrations são aplicadas pela API ao iniciar;
- administrador, Luis e o cenário demonstrável da Casa do Luis são criados de forma idempotente e somente em `Development`;
- `scripts/start-local.ps1`, `scripts/stop-local.ps1` e `scripts/validate-local.ps1` são o contrato operacional canônico;
- liveness verifica o processo e readiness inclui a conexão PostgreSQL;
- logs e PIDs locais ficam em `.local/`, ignorado pelo Git;
- o acompanhamento funciona localmente sem serviço externo.

## Staging e produção

Não serão provisionados na fundação. A hospedagem deve manter frontend e API sob a mesma origem ou proxy compatível para simplificar cookies seguros. A decisão final ocorrerá somente quando houver uma entrega remota autorizada.
