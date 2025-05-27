# Roguelike System Setup Instructions

This document explains how to set up the roguelike system with random level generation using Edgar.

## Overview

The roguelike system consists of:
1. A fixed hub world (Level0.0)
2. Randomly generated levels (Level1.1 - Level1.4)
3. When the player dies, they respawn in the hub world
4. Each time the player dies, the levels are regenerated with a new random seed

## Setup Instructions

### 1. Create Level Graphs

1. Right-click in the Project window and select **Create > Edgar > Level Graph**
2. Create at least one level graph for each level type you want to have
3. Configure the level graph with rooms and connections
4. Add room templates to the level graph

### 2. Create Level Exit Prefab

1. Create a new empty GameObject in a new scene
2. Add a sprite renderer and set an appropriate sprite for the level exit
3. Add the `LevelExit` script to the GameObject
4. Save the GameObject as a prefab in your project

### 3. Create Post-Processing

1. Right-click in the Project window and select **Create > Game > Roguelike Post Processing**
2. Configure the post-processing settings:
   - Set the enemy spawn chance
   - Leave the level exit prefab empty (it will be set at runtime)

### 4. Set Up Hub World (Level0.0)

1. Open or create the hub world scene (Level0.0)
2. Add your player character and other necessary objects
3. Add a level exit that leads to the first randomly generated level

### 5. Set Up Level Scenes (Level1.1 - Level1.4)

For each level scene:
1. Create a new scene
2. Add an empty GameObject and name it "LevelGenerator"
3. Add the `LevelGeneratorSetup` script to the GameObject
4. Assign the Roguelike Post Processing asset to the script
5. Assign the Level Exit prefab to the script
6. Add a `DungeonGeneratorGrid2D` component to the GameObject
7. Configure the `DungeonGeneratorGrid2D` component:
   - Add a Fixed Level Graph Config
   - Add a Generator Config
   - Add a Post Processing Config
   - Set "Generate On Start" to false

### 6. Set Up Game Manager

1. Create an empty GameObject in your hub world scene
2. Add the `LevelManager` script to the GameObject
3. Configure the LevelManager:
   - Set the hub world scene name to "Level0.0"
   - Assign your level graphs to the levelGraphs array
   - Set useRandomSeed to true

### 7. Add Scripts to Player

1. Add the `PlayerController` script to your player character
2. Add the `PlayerHealth` script to your player character

### 8. Update Build Settings

1. Open **File > Build Settings**
2. Add all your scenes to the build settings:
   - Level0.0 (hub world)
   - Level1.1
   - Level1.2
   - Level1.3
   - Level1.4
3. Make sure Level0.0 is the first scene in the list

## How It Works

1. The player starts in the hub world (Level0.0)
2. When they exit the hub world, they go to Level1.1
3. Level1.1 is randomly generated using Edgar
4. When they exit Level1.1, they go to Level1.2, and so on
5. If the player dies, they return to the hub world
6. The next time they exit the hub world, new random levels are generated

## Random Enemy Generation

The roguelike system now supports two methods of enemy spawning:

### Method 1: Predefined Enemy Positions

1. Create an "Enemies" GameObject as a child of your room template
2. Add enemy GameObjects as children of the "Enemies" GameObject
3. Position them where you want them to spawn
4. In the RoguelikePostProcessing asset, set `useRandomEnemyPositions` to false
5. Adjust the `enemySpawnChance` to control how likely each enemy is to appear

### Method 2: Random Enemy Generation (New)

1. Create enemy prefabs with the BasicEnemy script or your own enemy scripts
2. In the RoguelikePostProcessing asset, set `useRandomEnemyPositions` to true
3. Add your enemy prefabs to the `enemyPrefabs` list
4. For each enemy, set:
   - Enemy Prefab: The GameObject prefab for the enemy
   - Spawn Weight: Higher values make this enemy more likely to spawn
   - Enemy Name: Optional name for clarity in the inspector
5. Configure additional settings:
   - Enemies Per Room: Min/max number of enemies to spawn in each room
   - Enemy Spawn Margin: Distance from walls where enemies can spawn
   - Enemy Spawn Chance: Overall chance for each enemy position to have an enemy

This new system will:
1. Automatically determine room bounds based on floor tilemaps
2. Create random enemy positions within those bounds
3. Select enemies to spawn based on their spawn weights
4. Place them in the level with proper parent-child relationships

### Tips for Random Enemy Generation

- Create a variety of enemy types with different behaviors and difficulties
- Use spawn weights to control the rarity of different enemies
- Increase enemy difficulty in later levels by using different RoguelikePostProcessing assets
- Use the `enemySpawnMargin` to prevent enemies from spawning too close to walls

## Tips

- You can create different level graphs for different level types
- Customize the post-processing to add more features to your levels
- Add more enemy types and room templates for variety 