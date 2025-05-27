# Enemy Generation System

This system provides a flexible way to generate enemies in procedurally generated levels using the post-processing capabilities of the dungeon generator.

## Prerequisites

This system is designed to work with the Edgar Procedural Level Generator framework. Make sure you have Edgar installed in your project:

1. Install the Edgar package from the Unity Asset Store or GitHub
2. Import the package into your project
3. Set up your dungeon generator using the Edgar framework

## Setup Instructions

### 1. Using the ScriptableObject Approach

1. Create an instance of the `EnemyGenerationSystem` ScriptableObject:
   - Right-click in the Project window
   - Select `Create > Rune > Enemy Generation > Enemy Generation System`

2. Configure the ScriptableObject:
   - Assign enemy prefabs to the Basic, Elite, and Boss categories
   - Adjust spawn settings like min/max enemies per room
   - Configure room type tags if you use custom tags

3. Add the ScriptableObject to your dungeon generator:
   - Select your dungeon generator GameObject
   - Find the `Custom Post Process Tasks` section
   - Add the `EnemyGenerationSystem` ScriptableObject to the array

### 2. Using the MonoBehaviour Component Approach

1. Add the `EnemyGenerationComponent` to your dungeon generator GameObject:
   - Select your dungeon generator GameObject (with the `DungeonGeneratorGrid2D` component)
   - Click `Add Component`
   - Search for `Enemy Generation Component`

2. Configure the component:
   - Assign enemy prefabs to the Basic, Elite, and Boss categories
   - Adjust spawn settings like min/max enemies per room
   - Configure room type tags if you use custom tags

## Room Setup

1. Add the `RoomInfo` component to your room template prefabs:
   - Select your room template prefab
   - Click `Add Component`
   - Search for `Room Info`

2. Configure the room type:
   - Set the `Room Type` to match the room's purpose (Normal, Start, Boss, etc.)
   - For special rooms, you can override the global enemy spawn settings
   - For rooms that shouldn't have enemies, uncheck `Allow Enemy Spawning`

## Special Features

### Wave-Based Enemy Spawning

For boss fights or special rooms, you can use the `WaveBasedEnemySpawner`:

1. Add the `WaveBasedEnemySpawner` component to a GameObject in your scene
2. Configure waves with different enemy types, counts, and spawn patterns
3. Use events to trigger game logic when waves start/end

### Utility Functions

The `EnemySpawnUtils` class provides helper methods for spawning enemies in specific patterns:

- Circle patterns
- Grid patterns
- Line patterns
- Random patterns within an area

## Tips

1. For better performance, use object pooling for frequently spawned enemies
2. Use the debug options to visualize spawn areas during development
3. For boss rooms, consider using the `WaveBasedEnemySpawner` for more control
4. Make sure your enemy prefabs have appropriate AI and navigation components

## Troubleshooting

- If enemies spawn inside walls, increase the `Min Distance From Walls` setting
- If enemies overlap, increase the `Min Distance Between Enemies` setting
- If no enemies spawn, check the console for error messages
- Make sure your room templates have proper colliders for boundary detection
- If you get compiler errors about missing types, ensure you have the Edgar framework imported and the `using Edgar.Unity;` namespace is included in your scripts 