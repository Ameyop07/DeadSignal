# Dead Signal upgrade guide

## What is connected

The existing Unity 6000.5.2f1 / URP 17.5 project retains its movement, damage, scope, enemies and three missions. Open `Assets/Scenes/Compound.unity` and press Play. No manual scene wiring is required.

The menu now opens Missions and Armory screens. The HUD shows the current mission, health bar, ammo, weapon and credit balance. Buttons have hover/press feedback and screens fade in using unscaled time. Canvas scaling fits the layout to wider or narrower displays.

Start with the AR-41 and P9. Complete Outpost 17 to unlock Freight Depot, and Freight Depot to unlock Relay Station. Mission defaults are rifle, pistol, rifle. Selecting an owned weapon in the Armory overrides those defaults for future missions; Use Level Default restores automatic selection. VX-9 Compact and M12 Marksman are purchasable for 350 and 700 credits. Purchases do not automatically equip a weapon: select Equip after purchasing.

Credits and progression save after rewards, purchases and loadout selection. Rewards are per kill and per successful mission run, so replaying missions can earn more credits. Completing a mission rewards once within that run. Progress is local to the Windows user, using the `DeadSignal.Progress.v1` PlayerPrefs key. Graphics and audio preferences are also saved locally.

## Inspector configuration

1. Select `Assets/Configuration/Campaign.asset`. Its Weapons and Levels arrays define the catalog. Graphics Presets contains the Low, Medium and High URP assets.
2. Select `Assets/Configuration/Level1.asset`, `Level2.asset` or `Level3.asset`. Edit Title, Briefing, Default Weapon, Kill Reward and Completion Reward. Build Index must match the scene's position in Build Settings.
3. Select a weapon definition in `Assets/Prefabs/Weapons`. Edit its unique ID, display name, price, Initially Owned flag, combat stats, preview and audio clips. Do not change an existing ID after distributing a save unless intentionally migrating ownership.
4. A weapon must also be present in each Player's WeaponManager.Weapons array to be usable. The four supplied weapons are already connected. Add a new weapon to the player prefab, apply it consistently to all three scenes, and add its definition to the campaign catalog.
5. Select `Assets/Configuration/AudioPalette.asset` to replace concrete, grass and metal footstep arrays, impact clips, landing, UI and ambience sounds. Select weapon definitions for each gun's fire/reload/empty/switch clips. Reload clips should match Reload Duration.
6. Add a SurfaceAudio component to a ground or cover collider and choose Concrete, Grass or Metal. Unmarked surfaces use concrete. The starting area includes grass on the left and steel on the right for easy testing; decks, ramps and containers use metal.
7. For new levels, add the scene to Build Settings, create a LevelConfiguration asset, add it to Campaign.Levels, assign the catalog and level to GameManager, and rebake its NavMeshSurface. Configure ObjectiveManager.requiredEnemies for the actual guard count. Supplied scenes are already configured.

## Audio and art limitations

The 40 WAV files under `Assets/Audio/Designed` are original synthesized placeholders, not recordings of real guns or footsteps. They provide distinct layered transients, surface differences, variation and synchronized timing. Replace their Inspector references with licensed recordings for natural realism; no code change is needed. The generator is `AgentScripts/generate_audio.py`.

Soldiers, guns and environments remain original stylized geometry. Lighting, atmospheric sky, terrain silhouettes, surface grain and UI have been improved; this is not a photorealistic asset pack. Store previews are rendered from the actual weapon models.

## Performance and validation

Measured results and remaining limitations are recorded in [Verification/UPGRADE-RESULTS.md](Verification/UPGRADE-RESULTS.md). Both final standalone runs passed 54 functional checks, with about 59.8 FPS average at 1080p on the tested T600 laptop GPU.

Soldier mesh pieces are combined by material within separately animated body parts. Bullet impacts and blood marks reuse pools, muzzle flashes are reused, and audio uses 24 reusable spatial voices. Shooting remains hitscan, so it does not create projectile objects. Audio is generated offline and imported as assets instead of being synthesized on every scene load. Levels use LoadSceneAsync with an input-blocking progress screen; Unity scene activation can still cause a brief frame spike.

The three presets adjust URP render scale, shadow range/resolution, anti-aliasing, HDR, texture mip limits and LOD bias. The target is 60 FPS, subject to measured hardware performance.

To repeat the standalone verification, launch the executable with `-fps-benchmark -fps-quality 1` (0 Low, 1 Medium, 2 High). It uses an isolated in-memory progression profile and an explicitly named temporary round-trip key, leaves your real progression untouched, tests gameplay and exits. Reports are written to `Builds/DeadSignal/Verification`. Normal launches never run this automation.

The pre-upgrade scripts, scenes, prefabs, materials and project settings are backed up in `Backups/BeforeUpgrade-20261018`. The scripts in `AgentScripts` are construction/verification utilities, not instructions to rerun on customized scenes.
