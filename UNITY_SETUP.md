# LifeGame - Unity Setup Guide

## Quick Start (3 steps)

### 1. Open in Unity
- Install Unity Hub + Unity **2022.3 LTS** (or newer)
- Open Unity Hub -> "Open" -> select the `lifegame` folder
- Wait for import to complete

### 2. One-Click Build
- In Unity menu bar, click: **LifeGame -> Build All**
- This auto-generates everything: camera, lighting, board, UI, player, managers

### 3. Play
- Press the **Play** button
- Game starts at Main Menu -> click "Start New Life"

## Controls
| Key | Action |
|-----|--------|
| Space | Roll dice |
| WASD | Move character (open world) |
| Shift | Sprint |
| E | Interact |
| ESC | Exit grid world |

## Project Structure
```
Assets/Scripts/
  Core/           GameManager, GameFlowController, SaveSystem, Enums
  BoardGame/      BoardManager, DiceSystem, FamilyGenerator, KarmaJudge
  OpenWorld/      PlayerController, EventTrigger, IInteractable
  UI/             UIManager
  Audio/          AudioManager
  Editor/         LifeGameSceneBuilder (one-click scene generator)
Resources/
  GridData/       grids_sample.json (grid event data)
```

## What the Builder Creates
- **Board**: 30 grid cells in a circle, color-coded by age phase
- **PlayerToken**: Red sphere that moves along the board
- **DiceVisual**: White cube in center that spins when rolling
- **UI Canvas**: Main menu, dice panel, speed choice, event dialog, death screen, player info HUD
- **GameManager**: Singleton with state machine
- **GameFlowController**: Wires all UI buttons to game logic
- **Camera**: Top-down view of the board
- **Lighting**: Directional light with soft shadows

## Customization
- Edit `Resources/GridData/grids_sample.json` to add/modify grid events
- Grid cell colors are based on age phase (see `GetAgeColor` in builder)
- UI colors and layout can be tweaked in the builder or directly in Unity Inspector