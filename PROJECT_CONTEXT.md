# Floodify Project Context

Game type:
Mobile puzzle game based on flood fill mechanics, featuring both Campaign (Level-based) and Endless modes.

Core gameplay loop:
1. Player selects a color.
2. Player clicks a tile.
3. Flood fill spreads the color across connected tiles.
4. Each flood consumes one move.
5. Player can utilize Boosters/Items (Hint, Hammer, Color Bomb) to aid progression.
6. Player must convert all tiles to target color within move limit to win.

Main systems:
- GameManager: controls game state (Playing, Won, Lost, Endless Mode).
- BoardManager: generates the tile grid and handles grid logic.
- Tile: represents each cell.
- FloodFillAnimator: handles BFS flood animation and visual VFX (Particle System).
- LevelData: ScriptableObject storing level layout and target goals.
- SoundManager: manages BGM, UI SFX, and dedicated Loud SFX channels for Item impact.
- AdsManager: handles Google AdMob integration (Banner, Interstitial, Rewarded Ads).
- LeaderboardManager: connects to UGS Cloud to fetch Top 50 and current player rank.
- ItemManager & CurrencyManager: handles in-game economy, purchases, and booster inventory.
- SceneTransitionManager: manages smooth DOTween screen fades and loading spinners.
- AI Solver: utilizes Monte Carlo Tree Search (MCTS) with Time Budget optimization for the Hint system.

Scenes:
- MainMenu
- LevelSelect
- GameScene

Tech stack:
- Unity
- C#
- DOTween
- Unity Gaming Services (UGS) - Leaderboards
- Google Mobile Ads SDK