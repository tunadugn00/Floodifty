# Floodify Project Context

Game type:
Mobile puzzle game based on flood fill mechanics.

Core gameplay loop:
1. Player selects a color.
2. Player clicks a tile.
3. Flood fill spreads the color across connected tiles.
4. Each flood consumes one move.
5. Player must convert all tiles to target color within move limit.

Main systems:
- GameManager: controls game state (Playing, Won, Lost).
- BoardManager: generates the tile grid.
- Tile: represents each cell.
- FloodFillAnimator: handles BFS flood animation.
- LevelData: ScriptableObject storing level layout.

Scenes:
- MainMenu
- LevelSelect
- GameScene

Tech stack:
- Unity
- C#
- DOTween