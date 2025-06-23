# Door Sprite Setup Guide

## Overview
The DoorController script now supports custom door sprites that can be assigned in the Unity Editor. If no custom sprite is assigned, doors will automatically use a white square as the default.

## How to Assign Door Sprites

### Method 1: Global Configuration (Recommended)
1. Navigate to any existing door GameObject in your scene
2. Look for the **DoorController** component
3. In the **Door Sprite** section, find the **Custom Door Sprite** field
4. Drag any sprite from your Project window into this field

### Method 2: Using Context Menu
1. Right-click on any DoorController component in the Inspector
2. Select "Find Available Door Sprites" to see suggested sprites
3. Select "Setup Door with Default Settings" to automatically configure the door

## Available Door Sprites in Project
The project includes several door sprites you can use:

- **Assets/Brackeys/2D Mega Pack/Environment/Gothic/Door.png** - Standard door
- **Assets/Brackeys/2D Mega Pack/Environment/Gothic/DoorUp.png** - Upward-facing door
- **Assets/Brackeys/2D Mega Pack/Items & Icons/Arcade/BlueDoor.png** - Blue door
- **Assets/Brackeys/2D Mega Pack/Items & Icons/Arcade/GreenDoor.png** - Green door

## Features

### Automatic White Square Default
- If no custom sprite is assigned, doors automatically display as white squares
- Color changes based on locked/unlocked state (red = locked, green = unlocked)

### Inspector Fields
- **Custom Door Sprite**: Assign your preferred door sprite here
- **Start Locked**: Whether the door starts in a locked state
- **Locked Color**: Color tint when door is locked (default: red)
- **Unlocked Color**: Color tint when door is unlocked (default: green)

### Runtime Methods
- `RefreshSprite()`: Call this to update the door sprite if you change it at runtime
- `SetLocked(bool)`: Lock or unlock the door
- `IsLocked()`: Check if the door is currently locked

## Technical Notes
- Doors are created dynamically during level generation
- The DoorController automatically handles sprite setup in the Awake() method
- SpriteRenderer and Collider2D components are automatically added if missing
- The sprite system is backwards compatible with existing door setups

## Troubleshooting
- **No sprite showing**: Make sure the GameObject has a SpriteRenderer component
- **Wrong sprite**: Check the Custom Door Sprite field in the DoorController
- **Sprite not updating**: Call `RefreshSprite()` method or restart the scene 