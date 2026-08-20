# Ambientes

## Local

- frontend e API executam separadamente durante o desenvolvimento;
- nenhuma credencial real é versionada;
- PostgreSQL local executa via `docker compose` na porta `54329`;
- migrations são aplicadas pela API ao iniciar;
- o administrador local inicial é documentado no README e nunca é criado fora de `Development`;
- o acompanhamento funciona localmente sem serviço externo.

## Staging e produção

Não serão provisionados na fundação. A hospedagem deve manter frontend e API sob a mesma origem ou proxy compatível para simplificar cookies seguros. A decisão final ocorrerá somente quando houver uma entrega remota autorizada.
