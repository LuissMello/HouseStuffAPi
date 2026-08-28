# Padrões de desenvolvimento

## Governança

- tarefas usam `HOUSE-XXX`;
- uma tarefa autorizada por vez;
- subtarefas `HOUSE-XXX-YY` permitem mudanças e commits menores;
- tarefa funcional permanece aberta até demonstração ponta a ponta;
- mudança de regra exige registro em `BUSINESS_RULES.md` ou `DECISIONS.md`.

## Backend

- .NET 8, nullable, warnings como erros e APIs assíncronas com `CancellationToken` para I/O;
- Controllers finos, Application Services orquestrando e Domain protegendo invariantes;
- repositories específicos como fronteira de persistência;
- falhas esperadas por Result Pattern e HTTP Problem Details com código estável;
- endpoints versionados em `/api/v1`;
- PostgreSQL e migrations versionadas quando a persistência entrar;
- testes unitários para comportamentos e integração PostgreSQL para repositories.

## Frontend

- TypeScript strict e organização por feature;
- componentes acessíveis, responsivos e com estados de loading, vazio, erro e sucesso;
- regra crítica confirmada pela API;
- testes de componente e integração para comportamento; E2E para fluxos críticos;
- nenhuma tela funcional concluída com mocks permanentes.

## Definição de pronto funcional

- banco/migration, API e interface aplicáveis concluídos;
- integração real e isolamento de residência demonstrados;
- fluxo executável com roteiro de teste;
- testes e builds passando;
- tracking e documentação atualizados;
- evidência visual registrada no fechamento.
