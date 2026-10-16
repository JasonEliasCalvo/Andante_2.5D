# Andante 2.5D

A 2.5D narrative adventure game developed in Unity and C#, featuring exploration, interaction systems, dialogue, gameplay mechanics, and interactive minigames.

The project originally started as a graduation project and is currently being independently completed and refactored, with a focus on improving gameplay systems, maintainability, and separation of responsibilities.

## Current Portfolio Build

The current playable portfolio build focuses on two representative minigames:

### Memory Game

A card-matching minigame where the player reveals and matches pairs of cards.

Key gameplay systems include:

- Procedural card generation from configurable `CardData`
- Card shuffling
- Delayed card generation
- Automatic card reveal/hide behavior
- Match detection
- Match feedback
- Win-state detection
- Timer integration
- Camera transitions
- UnityEvent-based completion callbacks

### Happy & Angry

A character classification minigame where the player identifies a character's gender and emotional state using draggable UI elements.

Key gameplay systems include:

- Character data-driven gameplay
- Drag-and-drop interaction
- Gender and emotion validation
- Correct/incorrect feedback
- Audio feedback
- Animated character transitions
- Multiple sequential challenges
- Victory state and completion events

Additional minigame prototypes exist in the project, but the current portfolio path focuses on these two systems.

---

## Gameplay Systems

The project contains several reusable gameplay systems developed in Unity, including:

- Character movement and locomotion
- Character controllers
- Interaction systems
- Dialogue and dialogue choices
- Camera management
- Action and reaction systems
- Interactive objects
- UI interaction
- Inventory-related systems
- Moving platforms and environmental interactions
- Gameplay state management
- Minigame systems

The goal is to keep gameplay features modular enough to be iterated on independently.

---

## Technical Stack

**Engine**
- Unity 6
- Unity 6000.3.0f1

**Programming**
- C#
- Object-oriented gameplay architecture
- Unity MonoBehaviours
- ScriptableObjects
- UnityEvents
- Coroutines

**Unity Technologies**
- Unity Input System
- Universal Render Pipeline (URP)
- Cinemachine
- TextMeshPro
- Unity Test Framework

**Third-party**
- DOTween

---

## Architecture & Refactoring

One of the current development goals is reducing the amount of gameplay-specific logic inside global managers.

Originally, minigames were started and coordinated through large centralized managers such as `GameManager` and `UIManager`.

As the project grew, this approach became difficult to maintain because adding new minigames required adding more:

- Events
- Start methods
- Reset methods
- End methods
- UI references
- Gameplay-specific dependencies

The current refactor moves minigame activation closer to the minigame itself.

For example, minigames can now expose public entry points such as:

```csharp
public void StartMemoryGame()
{
    // Initialize memory game
}
```

or:

```csharp
public void StartHappyGame()
{
    // Initialize happy and angry game
}
```

These methods can be connected directly from the Unity Inspector using `UnityEvent`.

This reduces the responsibility of global managers and makes individual gameplay systems easier to test, replace, and extend.

---

## Code Organization

Some of the main gameplay areas include:

```text
Assets/
└── Scripts/
    ├── Core/
    │   ├── Action/
    │   └── Locomotion/
    │
    ├── Entity/
    │
    ├── Escenary/
    │
    ├── Inventario/
    │
    ├── UI/
    │
    └── Utils/
```

The project uses reusable components and data-driven structures where appropriate, allowing gameplay behavior to be configured from the Unity Inspector.

---

## Current Development Focus

The project is actively being improved with emphasis on:

- Gameplay reliability
- Debugging and playtesting
- Modular minigame architecture
- Separation of responsibilities
- Inspector-driven configuration
- Reusable gameplay components
- Cleaner global manager responsibilities

The current portfolio build intentionally highlights gameplay implementation and systems programming rather than presenting every prototype feature in the project.

---

## Running the Project

Open the project with:

**Unity 6000.3.0f1**

Then open the main gameplay scene from:

```text
Assets/Base/Scenes/
```

The project contains both gameplay and test scenes for development and iteration.

---

## Portfolio

GitHub:  
https://github.com/JasonEliasCalvo

Project repository:  
https://github.com/JasonEliasCalvo/Andante_2.5D

---

## About the Project

This project demonstrates hands-on experience with Unity gameplay programming, gameplay systems, debugging, iteration, and architectural refactoring.

The current development direction is focused on turning the original graduation project into a cleaner and more maintainable gameplay project while preserving its existing systems and expanding its playable content.