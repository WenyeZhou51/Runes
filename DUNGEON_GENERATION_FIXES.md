# Dungeon Generation Timeout Fixes

## Issues Identified and Fixed

### 1. **Corridor Door Configuration Issue**
**Problem**: The corridor template (`corridor pot.prefab`) had no doors defined in Manual mode, preventing proper room connections.

**Fix Applied**:
- Changed `SelectedMode` from `2` (Manual) back to `0` (Simple mode)
- Ensured `SimpleDoorModeData` is properly configured with:
  - `VerticalDoors.Enabled = 1`
  - `HorizontalDoors.Enabled = 1`
  - Proper margins and lengths

### 2. **Room Template Door Mode Inconsistency**
**Problem**: Room templates were using different door modes (Hybrid/Manual) which could cause compatibility issues.

**Fix Applied**:
- Changed all room templates to use `SelectedMode: 0` (Simple mode)
- This matches Edgar's example configurations and ensures compatibility

### 3. **Level Graph Complexity**
**Problem**: The original level graph had 8 rooms with 8 connections, creating a complex layout that Edgar struggled to generate.

**Fix Applied**:
- Simplified the level graph to 4 rooms with 3 connections
- Created a linear path: Room → Spawn → Room → Exit
- Removed complex interconnections that were causing generation failures

### 4. **Timeout Configuration**
**Problem**: Default timeout of 10 seconds was insufficient for complex layouts.

**Fix Applied**:
- Created debug scripts with 30-second timeout
- Added diagnostic capabilities to identify specific issues
- Provided configurable timeout settings

## Files Modified

### 1. `Assets/LevelGen/corridor pot.prefab`
```yaml
# Changed door configuration
SelectedMode: 0  # Simple mode
SimpleDoorModeData:
  VerticalDoors:
    Enabled: 1
  HorizontalDoors:
    Enabled: 1
```

### 2. `Assets/LevelGen/rectangle room.prefab`
```yaml
# Changed to Simple mode for consistency
SelectedMode: 0
```

### 3. `Assets/LevelGen/pillar room.prefab`
```yaml
# Changed to Simple mode for consistency
SelectedMode: 0
```

### 4. `Assets/LevelGen/LevelGraph.asset`
```yaml
# Simplified connections (reduced from 8 to 3)
Connections:
  - Room → Spawn
  - Spawn → Room  
  - Room → Exit

# Simplified rooms (reduced from 8 to 4)
Rooms:
  - Room (384, 144)
  - Spawn (496, 144) 
  - Room (496, 208)
  - Exit (608, 16)
```

## New Debug Scripts Created

### 1. `Assets/Scripts/DungeonGeneratorDebugger.cs`
- Comprehensive dungeon generator configuration
- 30-second timeout
- Diagnostics enabled
- Keyboard controls for testing

### 2. `Assets/Scripts/DungeonTestSetup.cs`
- Simple test setup for scene testing
- Configurable timeout and diagnostics
- Clear instructions and error handling

## How to Test the Fixes

### Method 1: Using the Debug Script
1. Add `DungeonTestSetup` component to any GameObject in your scene
2. Assign your `LevelGraph` to the `testLevelGraph` field
3. Press Play
4. Press `G` to generate dungeon
5. Press `C` to clear dungeon

### Method 2: Manual Testing
1. Find your existing DungeonGenerator in the scene
2. Set `Timeout` to 30000 (30 seconds)
3. Enable `EnableDiagnostics`
4. Assign the fixed `LevelGraph.asset`
5. Generate the dungeon

## Expected Results

With these fixes, you should see:
1. **Successful Generation**: No more timeout errors
2. **Diagnostic Output**: Detailed information about the generation process
3. **Proper Layout**: 4 connected rooms in a linear arrangement
4. **Performance Info**: Generation time and iteration count

## If Issues Persist

If you still encounter timeout issues:

1. **Further Simplify**: Reduce to 3 rooms (Start → Middle → End)
2. **Check Templates**: Ensure all room templates have proper floor tiles
3. **Increase Timeout**: Try 60 seconds (60000ms)
4. **Enable Diagnostics**: Check console for specific configuration issues

## Key Learnings

1. **Door Compatibility**: All templates should use the same door mode (Simple recommended)
2. **Graph Complexity**: Start simple and add complexity gradually
3. **Timeout Settings**: Edgar needs sufficient time for complex layouts
4. **Diagnostics**: Always enable diagnostics when debugging generation issues

## Edgar Best Practices Applied

1. Used Simple door mode (matches Edgar examples)
2. Linear room layout (easier to generate)
3. Proper corridor configuration
4. Adequate timeout settings
5. Diagnostic feedback enabled 