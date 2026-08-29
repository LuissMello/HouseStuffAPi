# HOUSE-170 — Mostrar e ocultar senha

Status: em andamento.

Tipo: funcional de frontend.

Repositório: front.

## Resultado observável

Nos campos de senha do login e da criação de usuário, a pessoa toca em “Mostrar” para conferir o valor digitado e em “Ocultar” para mascará-lo novamente.

## Subtarefas

- [>] `HOUSE-170-01` — componente acessível de senha;
- [ ] `HOUSE-170-02` — integração no login e administração;
- [ ] `HOUSE-170-03` — testes, publicação e fechamento.

## Critérios de aceite

- [ ] senha começa mascarada;
- [ ] ação alterna entre “Mostrar” e “Ocultar” sem apagar o valor;
- [ ] botão informa seu estado para leitor de tela;
- [ ] login mantém autocomplete de senha atual;
- [ ] criação mantém as regras e o nome do campo enviado;
- [ ] controle possui área de toque adequada no celular;
- [ ] lint, testes e build do Pages passam;
- [ ] versão pública pode ser testada pelo usuário.
