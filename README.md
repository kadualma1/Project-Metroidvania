# Guia de Trabalho em Equipe no Unity com GitHub

Este documento define o fluxo oficial de trabalho do projeto em **Unity + GitHub**, com o objetivo de evitar conflitos, perda de referências, cenas quebradas, arquivos corrompidos e retrabalho desnecessário.

A ideia é que todo membro do time siga este guia como padrão.

---

# Objetivo

Garantir que o desenvolvimento em equipe seja:

- organizado
- previsível
- seguro
- fácil de manter
- com menos conflitos entre cenas, prefabs e assets

---

# Stack de versionamento adotada

Este projeto utiliza:

- **Unity**
- **Git**
- **GitHub**
- **Git LFS** para arquivos pesados, quando necessário

Não utilizaremos Unity Version Control / Plastic SCM neste projeto, salvo mudança futura de decisão da equipe.

---

# Regras fundamentais

## 1. Nunca apagar, mover ou renomear assets manualmente pelo Explorer/Finder

Assets da Unity devem ser movidos, duplicados, renomeados e apagados **dentro do Unity Editor**, e não diretamente pelas pastas do sistema operacional.

Motivo:

A Unity usa arquivos `.meta` para manter referências internas.  
Se um asset for movido ou renomeado por fora de forma incorreta, referências podem quebrar.

### Certo

- mover asset pelo Project Window da Unity
- renomear asset pela Unity
- duplicar asset pela Unity

### Errado

- arrastar arquivos de uma pasta para outra pelo Explorer
- renomear prefab, scene, material, sprite ou script por fora da Unity
- apagar `.meta`

---

## 2. Atualizar o projeto antes de abrir a Unity

Antes de abrir o projeto, cada pessoa deve primeiro atualizar o repositório local.

### Fluxo padrão

1. Abrir o GitHub Desktop
2. Acessar a branch `main` antes de tudo
3. Fazer `git pull`
4. Verificar se houve conflito
5. Só então abrir a Unity

Isso reduz muitos problemas de reimport, arquivos desatualizados e conflito em scene/prefab.

---

## 3. Não trabalhar direto na branch principal

Ninguém deve desenvolver diretamente na `main`.

Toda tarefa deve ser feita em uma branch própria.

A branch deve ter o id da tarefa, contido na planilha de Sprints.

Pode ser formatado como id + "descrição curta da tarefa", mas o id sempre será obrigatório para evitar duplicatas.

---

## 4. Commits pequenos e claros

Evite commits gigantes misturando tudo.

Um commit bom é fácil de entender e fácil de reverter.

### Bom exemplo

- Corrige colisão do player no tilemap
- Adiciona menu de pausa
- Ajusta animação de ataque do inimigo
- Cria prefab base dos itens coletáveis

#### Prefixos de commits

Commits podem ter até 3 tipos.

- Feature/ - Utilizado para quando uma modificação que apenas adiciona conteúdo é feita.
- Fix/ - Utilizado para correções de bugs e comportamentos que não deveriam acontecer.
- Refactor/ - Para alterações conceituais de funções ou comportamentos já existentes.

#### Descrições de commits

Se atenha a poucas palavras. Usando os exemplos de Bom exemplo:

- Fix/tilemap-player-collision
- Feature/pause-menu
- Fix/enemy-attack-animation
- Feature/colectable-items-prefabs

---

## 5. Duas pessoas não devem mexer na mesma cena

Mesmo com merge em texto, cenas e prefabs ainda são pontos sensíveis no Unity.

Se duas pessoas precisarem alterar a mesma cena, isso deve ser combinado antes.

---
