# HOUSE-150 — Alteração de perfil dos moradores

Status: em andamento.

Tipo: funcional.

Repositório: ambos.

## Resultado observável

Na tela de pessoas da casa, um administrador promove outro morador para administrador ou rebaixa outro administrador para morador, com confirmação e retorno visual da API.

## Subtarefas

- [x] `HOUSE-150-01` — regras, contrato e critérios de aceite;
- [ ] `HOUSE-150-02` — API segura de alteração de perfil;
- [ ] `HOUSE-150-03` — ação responsiva na tela de pessoas;
- [ ] `HOUSE-150-04` — integração, testes, publicação e fechamento.

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

- [ ] API promove morador da própria casa;
- [ ] API rebaixa outro administrador da própria casa;
- [ ] API recusa alteração do próprio usuário, acesso pendente e outra residência;
- [ ] interface pede confirmação e atualiza o selo sem recarregar a página;
- [ ] ação possui estado de processamento, sucesso e erro;
- [ ] controles funcionam por toque e teclado no celular;
- [ ] testes e builds dos dois repositórios passam;
- [ ] fluxo publicado pode ser executado pelo usuário.
