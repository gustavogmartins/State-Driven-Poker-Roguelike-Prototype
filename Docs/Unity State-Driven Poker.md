# Projeto 1 — Unity State-Driven Poker Roguelike Prototype

> **Objetivo:** construir um protótipo **inspirado no loop central de Balatro**, com foco em arquitetura state-driven, regras claras, testes e legibilidade de código.
> **Importante:** este projeto é para **estudo e portfólio**, sem usar artes, nomes comerciais, assets ou conteúdo copiado do jogo original.

---

## Visão geral

A ideia não é fazer um **clone completo** de Balatro.

A ideia é construir um **protótipo inspirado no core loop**:

* run dividida em **antes**
* cada ante com **3 blinds**
* deck padrão de **52 cartas**
* jogador forma **mãos de pôquer**
* score baseado em **Chips x Mult**
* progresso por rounds
* shop simples entre blinds
* alguns modificadores estilo **Joker**

Esse é o coração do tipo de experiência que vale estudar.

---

## O que copiar no começo

Para o MVP, vale seguir estas bases:

* run dividida em **antes**
* cada ante com:

  * **Small Blind**
  * **Big Blind**
  * **Boss Blind**
* deck inicial de **52 cartas**
* **hand size** padrão de 8
* jogador pode **selecionar e jogar até 5 cartas**
* sistema identifica a **melhor mão de pôquer válida**
* a jogada gera:

  * **Chips**
  * **Mult**
* score final da jogada = **Chips x Mult**
* existe uma **meta de score** para vencer o blind
* entre blinds pode haver uma **shop simples**
* a shop vende poucos **modificadores/Jokers** fáceis de testar

---

## O que não copiar no começo

Para não travar, deixe para depois:

* dezenas de Jokers
* Tarot / Planet / Spectral cards
* decks alternativos
* tags de skip
* boss blinds complexos
* mãos secretas
* balanceamento fiel
* efeitos visuais avançados
* animações sofisticadas
* sistema completo de progressão do jogo original

---

## Nome recomendado para o projeto

Evite chamar de “clone do Balatro”.

Use algo como:

* **Unity State-Driven Poker Roguelike Prototype**
* **Balatro-Inspired Card Architecture Sample**
* **State-Driven Poker Run Prototype**

Isso passa mais maturidade e fica melhor para portfólio.

---

## Objetivo do projeto

Construir um protótipo state-driven focado em:

* modelagem de estado de uma run
* avaliação de mãos de pôquer
* score baseado em **Chips x Mult**
* fluxo de round e ante
* progressão entre blinds
* integração de modificadores simples
* testes de regras

---

## O que esse projeto deve demonstrar

* organização de estado
* reducers
* presenters
* separação entre regra e visual
* fluxo de round previsível
* clareza arquitetural
* testes de comportamento
* capacidade de evoluir regras sem quebrar a base

---

## O raciocínio central do projeto

Você **não** vai começar fazendo “Balatro inteiro”.

Você vai começar fazendo a **menor fatia jogável** que já prova a arquitetura.

### Primeira fatia ideal

Um único blind, jogável, com:

* deck
* compra de cartas
* descarte
* seleção de cartas
* identificação de mão de pôquer
* cálculo de score
* meta de blind
* vitória ou derrota do round

Sem shop.
Sem Joker.
Sem boss especial.
Sem conteúdo extra.

---

# Arquitetura state-driven adaptada para esse projeto

## 1. `RunState`

Representa a **run inteira**.

Exemplos do que guardar:

* ante atual
* blind atual
* dinheiro atual
* jokers/modificadores ativos
* estado do deck da run
* fase atual do jogo
* se está no round, shop, vitória ou derrota

---

## 2. `RoundState`

Representa o **round atual**.

Exemplos do que guardar:

* deck atual do round
* mão atual do jogador
* descarte atual
* cartas selecionadas
* hands restantes
* discards restantes
* blind atual
* target score
* score acumulado do round
* status do round

---

## 3. `PlayAreaState`

Representa a **jogada atual**.

Exemplos:

* cartas selecionadas
* mão reconhecida
* score da jogada
* quais cartas pontuaram
* quais efeitos foram ativados

---

## 4. `ShopState`

Entra depois, quando o loop principal já estiver pronto.

Exemplos:

* ofertas atuais
* dinheiro
* jokers possuídos
* item selecionado

---

# Ações principais

## Fase 1 — Round puro

* `StartRunAction`
* `StartBlindAction`
* `ToggleCardSelectionAction`
* `PlaySelectedCardsAction`
* `DiscardSelectedCardsAction`
* `DrawToHandSizeAction`
* `ResolveBlindOutcomeAction`

## Fase 2 — Progressão

* `AdvanceToNextBlindAction`
* `AdvanceToNextAnteAction`
* `LoseRunAction`
* `WinRunAction`

## Fase 3 — Shop

* `EnterShopAction`
* `BuyJokerAction`
* `SellJokerAction`
* `LeaveShopAction`

## Fase 4 — Modificadores

* `ApplyJokerEffectsAction`

---

# Reducers principais

## `RunReducer`

Responsável por:

* iniciar a run
* trocar de blind
* trocar de ante
* entrar e sair da shop
* encerrar a run

---

## `RoundReducer`

Responsável por:

* comprar cartas
* descartar
* jogar a mão
* consumir hands/discards
* acumular score
* verificar vitória ou derrota do blind

---

## `PokerHandEvaluator`

Responsável por reconhecer a mão jogada:

* High Card
* Pair
* Two Pair
* Three of a Kind
* Straight
* Flush
* Full House
* Four of a Kind
* Straight Flush

---

## `ScoringReducer` ou `ScoreCalculator`

Responsável por:

* calcular Chips base
* calcular Mult base
* aplicar bônus
* aplicar modificadores/Jokers
* retornar o score final da jogada

---

# Estrutura de domínio recomendada

Eu sugiro algo assim:

* `RunState`
* `RoundState`
* `BlindState`
* `DeckState`
* `HandState`
* `DiscardPileState`
* `SelectedCardsState`
* `PokerHandResult`
* `ScoreResult`
* `JokerState`
* `ShopState`

E para as entidades principais:

* `Card`
* `Suit`
* `Rank`
* `BlindType`
* `PokerHandType`
* `RoundPhase`
* `RunPhase`

---

# MVP ideal

## Milestone 1 — Blind único jogável

O jogo abre e permite:

* iniciar uma run
* enfrentar um blind com target score
* comprar cartas até hand size
* selecionar até 5 cartas
* jogar a mão
* detectar a mão formada
* calcular score
* acumular score no blind
* vencer ou perder o round

### Ainda não tem:

* shop
* jokers
* boss effects
* conteúdo especial

---

## Milestone 2 — Loop de ante

Status atual no código:

* Small Blind implementado
* Big Blind implementado
* Boss Blind implementado
* progressão de ante implementada
* recompensa em dinheiro ao vencer blind implementada
* overlay simples de transição em `Assets/Scenes/GameScene.unity` implementado

Ainda não entra aqui:

* regra especial de boss blind
* shop
* `RunState` separado para fluxo completo de run

---

## Milestone 3 — Shop simples

Adicionar:

* shop entre blinds
* dinheiro
* compra e venda simples
* 3 a 5 Jokers/modificadores criados por você

### Exemplos de modificadores simples

* `+4 Mult se a mão for Pair`
* `+30 Chips se a mão tiver um Ace`
* `X2 Mult se a mão for Flush`
* `+1 discard`
* `+1 hand`

---

## Milestone 4 — Boss blind simples

Adicionar efeitos especiais leves, por exemplo:

* “Pares valem menos”
* “Só pode jogar 4 cartas”
* “Flush não pontua”
* “-1 discard neste round”

A ideia aqui é mostrar que sua arquitetura aguenta regras especiais sem virar bagunça.

---

# Ordem prática de implementação

## Etapa 1 — Núcleo de cartas

Implementar:

* `Card`
* `Suit`
* `Rank`
* geração do deck de 52 cartas
* embaralhamento
* compra de cartas
* descarte
* seleção de até 5 cartas

---

## Etapa 2 — Estado do round

Implementar:

* `RoundState`
* `GameStateStore` / `RunStore`
* ações básicas do round
* reducers básicos
* presenters mínimos
* UI mínima para debug

---

## Etapa 3 — Avaliador de mãos de pôquer

Implementar:

* High Card
* Pair
* Two Pair
* Three of a Kind
* Straight
* Flush
* Full House
* Four of a Kind
* Straight Flush

Garantir precedência correta.

---

## Etapa 4 — Sistema de score

Implementar:

* Chips base por tipo de mão
* Mult base por tipo de mão
* score final = `Chips * Mult`
* score acumulado no blind
* target score do blind

---

## Etapa 5 — Fluxo de round

Implementar:

* hands restantes
* discards restantes
* jogar cartas
* descartar cartas
* comprar novamente até hand size
* verificar vitória
* verificar derrota

---

## Etapa 6 — Progressão da run

Implementar:

* `RunState`
* troca de blind
* troca de ante
* reward simples em dinheiro
* vitória/derrota da run

---

## Etapa 7 — Shop

Implementar:

* `ShopState`
* ofertas simples
* comprar modificador
* vender modificador
* sair da shop e voltar para a run

---

## Etapa 8 — Jokers/modificadores simples

Implementar poucos efeitos, mas com boa arquitetura.

O objetivo aqui é mostrar que você consegue:

* adicionar efeitos sem acoplar tudo
* manter regras testáveis
* manter UI separada da lógica

---

## Etapa 9 — Polish para portfólio

Adicionar:

* logs claros
* overlay de debug
* README profissional
* GIF ou vídeo curto
* screenshots
* explicação da arquitetura
* testes rodando

---

# O que testar primeiro

## 1. Avaliação de mão

Testes como:

* detectar Pair corretamente
* detectar Two Pair corretamente
* detectar Straight corretamente
* detectar Flush corretamente
* garantir precedência correta entre mãos

---

## 2. Score

Testes como:

* Pair gera o score esperado
* Straight gera o score esperado
* `Chips * Mult` está correto
* efeitos adicionais alteram score corretamente

---

## 3. Fluxo do round

Testes como:

* jogar uma mão reduz hands restantes
* descartar reduz discards restantes
* ao bater target score, blind é vencido
* ao acabar as hands sem atingir meta, blind é perdido

---

## 4. Jokers/modificadores

Testes como:

* modificador de Pair só ativa em Pair
* modificador de Ace só ativa quando houver Ace
* modificador de `+1 discard` altera o state corretamente

---

# Critério de sucesso adaptado

Esse projeto só está pronto quando eu conseguir:

* explicar a arquitetura em 3 minutos
* explicar o fluxo de uma jogada do início ao fim
* adicionar uma nova ação sem IA
* adicionar um novo modificador sem quebrar o resto
* corrigir um bug sozinho
* mostrar testes funcionando
* demonstrar uma run curta jogável
* ter um README claro e profissional
* deixar explícito que o projeto é **inspirado** no loop de um poker roguelike, sem copiar conteúdo visual/original

---

# Estrutura de pastas sugerida

```text
Assets/
  Scripts/
    Core/
      State/
        RunState.cs
        RoundState.cs
        BlindState.cs
        ShopState.cs
      Actions/
        StartRunAction.cs
        StartBlindAction.cs
        ToggleCardSelectionAction.cs
        PlaySelectedCardsAction.cs
        DiscardSelectedCardsAction.cs
        AdvanceToNextBlindAction.cs
        EnterShopAction.cs
        BuyJokerAction.cs
      Reducers/
        RunReducer.cs
        RoundReducer.cs
        ScoringReducer.cs
      Store/
        GameStateStore.cs

    Domain/
      Cards/
        Card.cs
        Rank.cs
        Suit.cs
        DeckBuilder.cs
      Poker/
        PokerHandType.cs
        PokerHandEvaluator.cs
        PokerHandResult.cs
      Scoring/
        ScoreResult.cs
        ScoreCalculator.cs
      Modifiers/
        JokerState.cs
        JokerEffectType.cs

    Presentation/
      Presenters/
        RunPresenter.cs
        RoundPresenter.cs
        ShopPresenter.cs
      ViewModels/
        RoundViewModel.cs
        ShopViewModel.cs
      UI/
        RunScreen.cs
        RoundScreen.cs
        ShopScreen.cs

    Tests/
      EditMode/
        PokerHandEvaluatorTests.cs
        ScoreCalculatorTests.cs
        RoundReducerTests.cs
        JokerTests.cs
```

---

# Escopo final recomendado

## Projeto 1 — Unity State-Driven Poker Roguelike Prototype

### Escopo sugerido

* run curta de 2 antes
* 3 blinds por ante
* hand size 8
* hands/discards limitados
* score por mão de pôquer
* 3 a 5 modificadores simples
* 1 boss blind especial
* testes de regras
* README profissional

Esse escopo já é forte o bastante para:

* estudo real de arquitetura
* portfólio
* entrevistas
* prática de gameplay systems
* prática de modelagem de estado
* prática de testes

---

# Resumo final

A melhor forma de adaptar seu projeto para um estudo inspirado em Balatro é:

1. **não tentar copiar tudo**
2. focar no **core loop**
3. usar **state-driven architecture** como base
4. começar por um **blind único jogável**
5. crescer em camadas:

   * round
   * score
   * progressão
   * shop
   * modificadores

---

# Regra de ouro deste projeto

> **Primeiro faça o round funcionar.**
>
> Depois faça o score funcionar.
> Depois faça a progressão funcionar.
> Só então adicione conteúdo extra.

---

Se você quiser, no próximo passo eu posso transformar isso em um `README.md` inicial de projeto, já pronto para colar no repositório.
