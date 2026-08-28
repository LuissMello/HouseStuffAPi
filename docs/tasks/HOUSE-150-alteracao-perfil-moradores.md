# HOUSE-150 — Alteração de perfil dos moradores

Status: concluída.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Na tela de pessoas da casa, um administrador promove outro morador para administrador ou rebaixa outro administrador para morador, com confirmação e retorno visual da API.

## Subtarefas

- [x] `HOUSE-150-01` — regras, contrato e critérios de aceite;
- [x] `HOUSE-150-02` — API segura de alteração de perfil;
- [x] `HOUSE-150-03` — ação responsiva na tela de pessoas;
- [x] `HOUSE-150-04` — integração, testes, publicação e fechamento.

## Regras

- somente administrador autenticado executa a operação;
- o usuário-alvo precisa pertencer à mesma residência do administrador;
- acessos ainda sem casa e usuários de outra casa são recusados;
- o administrador não altera o próprio perfil por essa operação;
- promover remove o papel `Member` e adiciona `Administrator`;
- rebaixar remove `Administrator` e adiciona `Member`;
- trocar perfil invalida sessões já abertas do usuário-alvo;
- falha intermediária restaura o papel anterior antes de responder com erro.

## Critérios de aceite

- [x] API promove morador da própria casa;
- [x] API rebaixa outro administrador da própria casa;
- [x] API recusa alteração do próprio usuário, acesso pendente e outra residência;
- [x] interface pede confirmação e atualiza o selo sem recarregar a página;
- [x] ação possui estado de processamento, sucesso e erro;
- [x] controles funcionam por toque e teclado no celular;
- [x] testes e builds dos dois repositórios passam;
- [x] fluxo publicado pode ser executado pelo usuário.
