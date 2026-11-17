# Changelog

## [0.10.0] - 2026-11-17

### New

- Carriable stones now snap to the center of floor switches upon contact and lock in place, preventing accidental re-grabbing with the tongue during puzzles.
- Knockback impact physics added when hitting Fox and Small Owl enemies with arrows.
- 360-degree radial cooldown overlay and countdown timer added to the Stun/Sneeze HUD icon.
- Animated mucus expansion intro sequence when opening the pause menu, with input locking during the transition to prevent button spam.
- Smooth horizontal carousel transitions and clickable page indicators for the Credits screen, along with refreshed team and partner logos.
- Layered parallax background, updated stone frog statue animation, and ambient rain overlay effect on the Main Menu.

### Changes

- Dropping carried stones is now blocked when facing walls or obstacles to prevent clipping out of bounds.
- Arrow counter and Stun/Sneeze ability icons remain hidden on the HUD until unlocked during gameplay progression, and the legacy world-space ready indicator was removed.
- Relocated the updated game logo to the Splash Screen and refreshed pressed/selected button visuals and menu typography.
- Standardized sprite rendering layers across all characters, props, and environments to prevent overlapping visual glitches.
- Updated shop doors to a 2x2 footprint with proper collision alignment and organized shop interior and exterior visuals.

### Bug Fixes

- FROG-12 Implemented stone locking when placed on top of a floor switch.
- FROG-17 Fixed wall clipping when dropping carried stones near obstacles.
- FROG-21 Fixed missing tiles in the Meseta level scene.
- FROG-25 Updated shop spritesheet and collision so the player can no longer walk over the counter.
- FROG-27 Fixed Fox and Shielded Fox chase AI so they detect water edges instead of walking across water.
- FROG-33 Added boundaries and collisions in the Mangrove level to prevent the player from walking off-camera.
- FROG-35 Implemented dynamic HUD visibility so Stun and Arrow icons appear only when unlocked.
- FROG-36 Fixed exit door collision in the shop where the player would get stuck when leaving.
- FROG-38 Fixed Mangrove zone transitions where the player could clip through obstacles while switching rooms.
- FROG-39 Fixed Mangrove room transition edge detection causing obstacle clipping.
- FROG-43 Adjusted platform alignment in the Meseta level so the player no longer falls off misaligned edges.
- FROG-46 Fixed Mangrove room camera boundaries so levels no longer appear cut in half at screen edges.
- FROG-47 Added starting boundary walls in the Mangrove level to prevent accidental falls into water upon spawning.
- FROG-48 Fixed stone rendering layer so carried and dropped stones always display above ground obstacles.

## [0.9.0] - 2026-11-10

### New

- Dynamic visual changes for player helmets (automatic breaking/equipping states upon taking damage or drowning).
- Passive Shopkeeper NPC inside the bow shop with independent body animations and particle effects.
- Weighted item drop system for destructible objects (bushes and grass now drop health pickups and arrows).
- Ambient and health fly entity animations.
- New isolated test scene (`InteractableObjects_Test`) for validating destructible props and loot drop rates.
- UI focus persistence: reopening panels now restores selection to the previously focused button instead of resetting.
- Global event system and focus recovery logic to prevent input loss when window focus changes.
- Visual button selection feedback (color tinting) during D-Pad, keyboard, and mouse navigation.
- Paginated credits screen with horizontal stick/D-Pad navigation and input throttling.
- Pixel-crisp typography engine applied across all fonts, UI buttons, and HUD labels.
- Animated background visuals for the main menu.
- Updated studio and partner branding logos.

### Changes

- Tongue grab mechanics updated: item pull and retraction now sync correctly, granting items/shields only when successfully returning to the player.
- Drowning mechanics fixed: standing on moving platforms correctly suspends water-drowning checks and safe-tile resets.
- Destructible objects now evaluate and spawn loot drops before clearing tilemap cells.
- Hardware mouse cursor is now strictly locked and hidden during splash screens and main menus.
- Refactored level selection UI, canvas elements, and layout scaling.
- Updated default studio company name in settings.
- Splash screen input optimized to prevent duplicate scene loads when pressing start/enter.
- Pause menu flow refactored to properly delegate pause states and force initial button focus.
- Legacy HUD elements replaced with dedicated health bar spritesheet assets.

### Bug Fixes

- FROG-23 Fixed keyboard and joystick navigation bounds to prevent selection from leaving selectable button ranges.
- FROG-24 Fixed splash screen input bug where pressing Enter/Start failed to trigger the main menu transition.
- FROG-29 Fixed button selection loss when minimizing and restoring application window focus.
- FROG-31 Fixed navigation wrapping issues allowing focus to escape valid menu button boundaries.
- FROG-34 Fixed pause menu toggle issue where subsequent pause inputs failed to function correctly.

## [0.8.0] - 2026-11-03

### New

- Player ability progression tracking (tongue, bow, and sneeze abilities).
- Stealable bow mechanic: players can use their tongue to grab the bow from the sleeping NPC.
- Kinematic moving platforms supporting continuous looping or player-triggered activation.
- Mangrove biome visual assets, tilesets, and animated water tiles.
- Clean UI panel transition framework managing display states without losing layout hierarchy.
- Automatic gamepad/keyboard focus assignment when opening UI menus.
- Main menu and splash screen flow architecture (press-any-key transition to main menu).
- In-game pause menu handling with automatic input mapping swaps (Gameplay vs. UI).
- Player damage reaction states with 4-way directional hit animations.

### Changes

- Movement and idle inputs are now properly restricted when required abilities (bow/sneeze) are locked.
- Players are now smoothly transported on moving platforms without physics jitter.
- Water drowning checks are suspended while standing on platforms over hazardous water.
- Optimized tilemap collision detection from Sprite to Grid across Mangrove and Shop levels for better performance.
- Reorganized shop assets and atlases to optimize rendering.
- Room transitions updated with edge-detection boundaries and automated obstacle clearance to prevent wall-clipping.
- Teleport doors and player facing directions adjusted for accurate spawning orientation.
- Player input and controls are locked during room transitions, applying seamless invincibility feedback.
- Tongue hitboxes re-calibrated to match the peak frame of the attack animation.
- Refreshed 4-directional idle sprites and corrected backward-facing stone pickup animations.

### Bug Fixes

- FROG-10 Fixed wall-clipping and shaking during vertical room transitions using deterministic edge positioning.
- FROG-11 Resolved player getting stuck inside door wall tiles upon transition completion.
- FROG-12 Blocked tongue execution during active camera transitions to prevent transition softlocks.
- FROG-13 Fixed camera failing to follow the player when immediately backtracking into a previous room.
- FROG-14 Blocked movement input during transition frames to prevent walking outside playable boundaries.
- FROG-15 Fixed collision snagging on stump environmental props when backtracking leftward.
- FROG-16 Replaced position snapping during chunk transitions with smooth interpolation.

## [0.7.1] - 2026-10-30

### New

- Mangrove biome integration including terrain sprites, tile palettes, animated water, and test scene (`TilemapTest_Mangrove`).
- Forced player sprite orientation support on spawns and teleports.
- Dedicated camera calibration test scene for viewport tracking and boundary limits.

### Changes

- Optimized collision detection mode from Sprite to Grid on Mangrove and Shop levels.
- Reorganized shop assets into structured directories and optimized texture atlases.
- Refactored teleport door mechanics and room transition coordinate handoffs.
- Synchronized tongue hitboxes precisely with attack animation extension frames.
- Updated player idle spritesheets and animations across all directions.
- Corrected pixel color inconsistencies on backward-facing stone pickup frames.
- Cleaned up audio integration binaries and repository ignore rules.
- Removed obsolete and legacy testing scenes.

## [0.7.0] - 2026-10-26

### New

- SmallOwl enemy animations, animator controller, and flying/attack clips.
- Fox and FoxShielded enemy animations, spritesheets, and animator controllers.
- EnemyAnimations_Test scene for isolated animation and behavior validation.
- Player stone-grabbing idle and movement animation clips.
- Player grapple movement toward target animations.
- Player sneeze attack animation and spritesheet asset.
- Interactive Wwise audio integration managed by AudioManager and MusicManager.
- Interactive music system supporting Overworld, Shop, Menu, Dungeon, and BossFight states.
- Wwise 3D spatialized sound effect event mappings for player, enemies, environment, and UI.
- Manual Main SoundBank preloading and unloading memory lifecycle handling.
- Drop shadow system for Player, Fox, FoxShielded, and SmallOwl prefabs.
- Exclamation mark alert indicator visual system for enemy detection states.
- Scratch attack visual FX spritesheets and prefabs for Fox and SmallOwl enemies.
- Sneeze stun animations for Fox and SmallOwl entities.

### Changes

- Dynamic horizontal sprite flipping for Fox and SmallOwl based on movement direction.
- Refactored prefab structures and organized scripts into dedicated folders for Fox, FoxShielded, and SmallOwl.
- Renamed primary player movement state from Locomotion to Move in FSM logic.
- Reorganized player animation clips and controllers into dedicated directory structure.
- Refactored MusicManager initialization and added playing ID guards to eliminate duplicated audio playback.
- Updated MusicManager codebase for compatibility with Wwise 2025 SDK.
- Synchronized player sneeze particle emission with animation frame timing.