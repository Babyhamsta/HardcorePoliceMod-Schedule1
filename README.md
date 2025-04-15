# HardcorePolice Mod - Schedule1


🚨 Features

Dynamic Flanking – Officers are assigned roles (Chaser or Interceptor). Interceptors attempt to predict your route and cut you off from the sides or ahead.

Path Unstucking – AI recalculates paths if stuck, helping them navigate tricky terrain or corners better.

Arrest Logic Tweaks – Fine-tuned radius and progress speed for arrests makes evading a close-range officer nearly impossible.


🧠 Enhanced Awareness

Vision Cone Upgrade – Widened field of view and range allows officers to detect you faster and from further distances.

Optional Faster Detection – Officers require less time to lock on if you’re in view.


🏃 Speed-Based Boosting

Initial Chase Boost – Upon detection, cops get a burst of speed to catch up quickly.

Distance-Based Scaling – Officers dynamically run faster the further away you get, with optional cap control to prevent imbalance.


📡 Radioing Nearby Units

Backup System – Officers who first spot you will radio others in the area to join the chase after a short delay.

Staggered Joining – Prevents lag spikes by queueing officers one at a time into the pursuit instead of all at once.


🛠️ Full Configuration Support

All features can be toggled or tuned via an auto-generated JSON config:

Enable/disable flanking, vision upgrades, speed boosts, and radio calls

Adjust multipliers, detection speeds, and chase logic

Safely reloads older configs and applies missing defaults


🎮 How This Improves Gameplay

✅ Forces smarter movement during escapes

✅ Makes police feel more coordinated and tactical

✅ Increases tension, especially with multiple officers

✅ Prevents cheesy exploits like looping buildings endlessly

✅ Fully configurable for realism, chaos, or balance


📦 Installation

Install MelonLoader.
Drop HardcorePolice.dll into your Mods folder.

Run the game once to generate a config in `UserData/HardcorePoliceConfig.json`

Edit the config as needed, or use the default hardcore settings.


⚙️ Configuration Guide

✅ General Toggles

ExtendedVision – Enables wider and longer vision cones for officers.

FasterDetection – Reduces notice time when an officer sees you.

SpeedBoost – Applies an immediate speed boost at the start of a pursuit.

DistanceSpeedBoost – Increases officer speed the farther they are from the player.

Flanking – Allows officers to split roles (Chasers and Interceptors) and attempt strategic cut-offs.

RadioNearby – Nearby officers will be called in for backup a few seconds after a pursuit begins.

ArrestAdjustments – Modifies arrest behavior (arrest cooldown radius and speed of progress bar).


👁️ Vision Settings (Game defaults shown for reference)

VisionRangeMultiplier – Multiplier for how far officers can see. (Default: 1.0 | Mod Default: 10)

VisionHorizontalFOV – Horizontal field of view in degrees. (Default: 135.0 | Mod Default: 165)

VisionVerticalFOV – Vertical field of view in degrees. (Default: 60.0 | Mod Default: 80)


🏃 Speed Boost Settings

SpeedBoostMultiplier – Flat speed multiplier applied when chase begins. (Default: 1.3)

MovementSpeedScale – Adjusts movement animation speed alongside the boost. (Default: 1.15)

MoveSpeedMultiplier – Multiplies actual speed value for consistent pursuit. (Default: 1.15)



📏 Distance-Based Speed Boost

DistanceBoostBase – Minimum speed boost when close to the player. (Default: 1.45)

DistanceBoostMax – Maximum speed boost when far away. (Default: 2.0)

DistanceMax – Max distance (in meters) used to calculate the boost curve. (Default: 100.0)


📡 Radio System

RadioDistance – Radius (in meters) for which nearby officers are called in for pursuit. (Default: 100.0)


🔀 Flanking & Positioning

FlankUpdateCooldown – Seconds between recalculating a new flank position for each officer. (Default: 3)


👮 Arrest Behavior (Game defaults not mod defaults)

ArrestCooldownCircleDistance – Outer radius where if the player stays within this area, the cuffing cooldown will not decrease. This prevents players from “kiting” officers by hovering just outside arrest range. (Default: 2.5 | Mod Default: 5)

ArrestProgressSpeed – Time in seconds required to fully fill the arrest meter. (Default: 1.7 | Mod Default: 1.5)


🧪 Notes & Known Issues
Lag with 10+ officers? The mod now staggers AI decisions and backup requests, but performance may still dip if you’re being chased by a full squad. Consider tuning flanking or radio range in the config.

Police vision may conflict with other mods that modify detection. Be mindful of compatibility.

Flanking option can cause some FPS loss depending on how many officers are involved in pursuit, this is being worked on.
