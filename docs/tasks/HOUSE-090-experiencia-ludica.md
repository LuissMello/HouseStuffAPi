# HOUSE-090 — Experiência lúdica e sorteio animado

Status: concluída.

Tipo: funcional.

Repositório: front.

## Resultado observável

O usuário anota tarefas em uma interface de post-it e realiza o sorteio por uma sequência animada: inicia a brincadeira, escolhe um pote, vê o pote avançar e chacoalhar e recebe a tarefa em um post-it no centro da tela.

## Subtarefas

- [x] `HOUSE-090-01` — direção visual e contrato da interação;
- [x] `HOUSE-090-02` — cadastro e catálogo em estilo post-it;
- [x] `HOUSE-090-03` — potes e sorteio animados;
- [x] `HOUSE-090-04` — acessibilidade, responsividade, testes e fechamento.

## Escopo

- tornar o cadastro administrativo de tarefas semelhante a uma anotação em post-it;
- apresentar tarefas cadastradas como post-its variados;
- iniciar o sorteio antes de pedir a escolha do pote;
- representar potes com formas, rótulos e papéis usando HTML e CSS;
- trazer o pote escolhido para frente e animá-lo durante o sorteio;
- animar o post-it sorteado da posição do pote até o centro da tela;
- preservar aceite, troca, erros e regras reais da API;
- respeitar `prefers-reduced-motion` e uso por teclado e toque.

## Fora do escopo

- imagens externas ou mudança de identidade da marca;
- sons, vibração ou efeitos que dependam de permissão do dispositivo;
- mudanças em regras, banco ou endpoints.

## Regras

- `BR-002` e `BR-003` permanecem inalteradas;
- animação nunca substitui texto, foco ou resposta da API;
- o resultado continua sendo proposta até o aceite.

## Critérios de aceite

- [x] nova tarefa é preenchida e salva em uma superfície de post-it;
- [x] catálogo apresenta tarefas como post-its legíveis;
- [x] fluxo começa em “Sortear uma tarefa” e depois solicita o pote;
- [x] pote selecionado avança visualmente e informa seleção acessível;
- [x] pote chacoalha enquanto a API sorteia;
- [x] tarefa sorteada entra no centro como post-it;
- [x] aceite, troca e erros continuam integrados à API;
- [x] experiência funciona em celular e desktop;
- [x] movimento reduzido é respeitado;
- [x] lint, build e testes passam;
- [x] documentação e tracking atualizados.

## Fechamento

- Resultado: cadastro em post-it, catálogo colorido e sorteio em duas etapas com pote selecionado, chacoalho e post-it central;
- Integração: endpoints e regras de proposta, aceite e troca foram preservados;
- Acessibilidade: seleção usa `aria-pressed`, resultado é um diálogo focável, Escape fecha e movimento reduzido desativa animações;
- Responsividade: potes usam duas colunas e post-it central adaptado nas telas pequenas;
- Testes: lint, build e nove testes do frontend aprovados, além do roteiro local completo.
