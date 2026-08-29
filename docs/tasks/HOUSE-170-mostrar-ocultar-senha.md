# HOUSE-170 — Mostrar e ocultar senha

Status: concluída.

Tipo: funcional de frontend.

Repositório: front.

## Resultado observável

Nos campos de senha do login e da criação de usuário, a pessoa toca em “Mostrar” para conferir o valor digitado e em “Ocultar” para mascará-lo novamente.

## Subtarefas

- [x] `HOUSE-170-01` — componente acessível de senha;
- [x] `HOUSE-170-02` — integração no login e administração;
- [x] `HOUSE-170-03` — testes, publicação e fechamento.

## Critérios de aceite

- [x] senha começa mascarada;
- [x] ação alterna entre “Mostrar” e “Ocultar” sem apagar o valor;
- [x] botão informa seu estado para leitor de tela;
- [x] login mantém autocomplete de senha atual;
- [x] criação mantém as regras e o nome do campo enviado;
- [x] controle possui área de toque adequada no celular;
- [x] lint, testes e build do Pages passam;
- [x] versão pública pode ser testada pelo usuário.
