# HardcorePolice Mod

A tactical AI enhancement mod that makes police chases more challenging and dynamic.

## 🚨 Features

- **Dynamic Flanking** – Officers are assigned roles (Chaser or Interceptor). Interceptors attempt to predict your route and cut you off from the sides or ahead.
- **Path Unstucking** – AI recalculates paths if stuck, helping them navigate tricky terrain or corners better.
- **Arrest Logic Tweaks** – Fine-tuned radius and progress speed for arrests makes evading a close-range officer nearly impossible.

## 🧠 Enhanced Awareness

- **Vision Cone Upgrade** – Widened field of view and range allows officers to detect you faster and from further distances.
- **Optional Faster Detection** – Officers require less time to lock on if you're in view.

## 🏃 Speed-Based Boosting

- **Initial Chase Boost** – Upon detection, cops get a burst of speed to catch up quickly.
- **Distance-Based Scaling** – Officers dynamically run faster the further away you get, with optional cap control to prevent imbalance.

## 📡 Radioing Nearby Units

- **Backup System** – Officers who first spot you will radio others in the area to join the chase after a short delay.
- **Staggered Joining** – Prevents lag spikes by queueing officers one at a time into the pursuit instead of all at once.

## 🛠️ Full Configuration Support

All features can be toggled or tuned via an auto-generated JSON config:

- Enable/disable flanking, vision upgrades, speed boosts, and radio calls
- Adjust multipliers, detection speeds, and chase logic
- Safely reloads older configs and applies missing defaults

## 🎮 How This Improves Gameplay

- Forces smarter movement during escapes
- Makes police feel more coordinated and tactical
- Increases tension, especially with multiple officers
- Prevents cheesy exploits like looping buildings endlessly
- Fully configurable for realism, chaos, or balance

## 📦 Installation

1. Install MelonLoader.
2. Drop `HardcorePolice.dll` into your Mods folder.
3. Run the game once to generate a config in `UserData/HardcorePoliceConfig.json`
4. Edit the config as needed, or use the default hardcore settings.

## ⚙️ Configuration Guide

### General Toggles

| Setting | Description |
|---------|-------------|
| `ExtendedVision` | Enables wider and longer vision cones for officers. |
| `FasterDetection` | Reduces notice time when an officer sees you. |
| `SpeedBoost` | Applies an immediate speed boost at the start of a pursuit. |
| `DistanceSpeedBoost` | Increases officer speed the farther they are from the player. |
| `Flanking` | Allows officers to split roles (Chasers and Interceptors) and attempt strategic cut-offs. |
| `RadioNearby` | Nearby officers will be called in for backup a few seconds after a pursuit begins. |
| `ArrestAdjustments` | Modifies arrest behavior (arrest cooldown radius and speed of progress bar). |

### Vision Settings

| Setting | Description | Game Default | Mod Default |
|---------|-------------|--------------|------------|
| `VisionRangeMultiplier` | Multiplier for how far officers can see. | 1.0 | 10.0 |
| `VisionHorizontalFOV` | Horizontal field of view in degrees. | 135.0 | 165.0 |
| `VisionVerticalFOV` | Vertical field of view in degrees. | 60.0 | 80.0 |

### Speed Boost Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `SpeedBoostMultiplier` | Flat speed multiplier applied when chase begins. | 1.3 |
| `MovementSpeedScale` | Adjusts movement animation speed alongside the boost. | 1.15 |
| `MoveSpeedMultiplier` | Multiplies actual speed value for consistent pursuit. | 1.15 |

### Distance-Based Speed Boost

| Setting | Description | Default |
|---------|-------------|---------|
| `DistanceBoostBase` | Minimum speed boost when close to the player. | 1.45 |
| `DistanceBoostMax` | Maximum speed boost when far away. | 2.0 |
| `DistanceMax` | Max distance (in meters) used to calculate the boost curve. | 100.0 |

### Radio System

| Setting | Description | Default |
|---------|-------------|---------|
| `RadioDistance` | Radius (in meters) for which nearby officers are called in for pursuit. | 100.0 |

### Flanking & Positioning

| Setting | Description | Default |
|---------|-------------|---------|
| `FlankUpdateCooldown` | Seconds between recalculating a new flank position for each officer. | 3.0 |

### Arrest Behavior

| Setting | Description | Game Default | Mod Default |
|---------|-------------|--------------|------------|
| `ArrestCooldownCircleDistance` | Outer radius where if the player stays within this area, the cuffing cooldown will not decrease. This prevents players from "kiting" officers by hovering just outside arrest range. | 2.5 | 5.0 |
| `ArrestProgressSpeed` | Time in seconds required to fully fill the arrest meter. | 1.7 | 1.5 |

## 🧪 Notes & Known Issues

- **Performance:** Lag with 10+ officers? The mod now staggers AI decisions and backup requests, but performance may still dip if you're being chased by a full squad. Consider tuning flanking or radio range in the config.
- **Compatibility:** Police vision may conflict with other mods that modify detection. Be mindful of compatibility.
- **Optimization:** Flanking option can cause some FPS loss depending on how many officers are involved in pursuit, this is being worked on.
