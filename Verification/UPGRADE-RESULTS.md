# Upgrade verification — 18 October 2026

The Windows build succeeded with **zero errors**. Both final standalone runs passed **54 functional assertions**. No runtime exception, missing-reference error or NavMesh creation failure was logged in those final runs.

## Measured standalone performance

Hardware: NVIDIA T600 Laptop GPU (4 GB), Intel Core i7-12800H. Windows player, Direct3D 12, 1920×1080 output, 60 FPS cap. Unity Editor remained open but was not running the game. Tests used scripted combat against live AI across all three missions; screenshots, menu tests and loading were excluded from the combat frame sample.

| Measurement | Medium | High |
|---|---:|---:|
| URP internal render scale | 85% | 100% |
| Sampled combat frames | 2,484 | 2,726 |
| Average FPS | 59.84 | 59.85 |
| Median frame time | 16.67 ms | 16.67 ms |
| 95th percentile frame time | 16.86 ms | 16.83 ms |
| 99th percentile frame time | 17.00 ms | 16.99 ms |
| GPU frame time, 95th percentile | 6.79 ms | 5.94 ms |
| Peak Unity used-memory counter | 162.5 MB | 164.3 MB |
| Level 1 to 2 loading | 0.167 s | 0.134 s |
| Level 2 to 3 loading | 0.141 s | 0.146 s |

Loads display a progress screen and use LoadSceneAsync, but scene activation is not guaranteed to be hitch-free on other hardware or cold storage.

These are short, capped tests, not a guarantee of universal 60 FPS. AI paths, camera visibility, cache warmth and OS scheduling vary between runs, so the small Medium/High differences do not imply High is faster. Main-thread timing includes frame-cap waiting and should not be interpreted as active CPU work. GPU values come from FrameTimingManager; main-thread and memory counters come from Unity's ProfilerRecorder. Draw-call and GC-allocation counters were unavailable in this release player; the zero values in the raw files are **not** claims of zero draw calls or zero allocation. Low is configured but was not separately performance-benchmarked.

## Functional coverage

- Insufficient funds, exact deductions, duplicate purchases, ownership and unauthorized equip rejection.
- Money, ownership and selected weapon serialization through a separate PlayerPrefs round-trip key; real player progression remains untouched by the test.
- Actual store Buy and Equip button callbacks.
- Level locks, completion rewards/unlocks, correct default gun in each level, and owned loadout override after a scene reload.
- All guards on NavMesh, combat and extraction in all three levels.
- Concrete, grass and metal footsteps during actual CharacterController movement; faster running cadence; silence while idle and airborne; landing audio.
- Each gun's fire, reload and empty-magazine behavior, ammunition counts and assigned distinct audio clips.
- Armory controls fit 1280×720, 1280×960 and 1920×1080 in the High run. Windows adjusted the first requested window in the Medium run to 1888×982, which also passed; the raw reports record actual sizes.
- Armory, mission selection and settings screenshots visually inspected.

## Optimizations and remaining limitations

The supplied soldier prefab had 42 original mesh renderers; combining by material within animated parts leaves 15 active renderers. Hitboxes and separate leg/chest animation remain intact. Impacts, blood marks and muzzle flashes are reused; 24 pooled AudioSources handle sound. Imported WAV assets replace per-scene audio synthesis. Graphics presets tune rendering scale, shadows, MSAA, HDR, texture mip limits and LOD bias.

Visual models and synthesized sounds remain identified placeholders. They are more detailed and differentiated, but not photorealistic models or authentic recordings. Inspector replacement steps are in UPGRADE-GUIDE.md. Some existing text updates and per-hit physics queries still allocate; no zero-GC guarantee is made. The build includes 486 warnings, largely existing Unity/Sentis shader optimization warnings and the disabled runtime Pipeline notice, rather than game compilation errors.

## Evidence

- `upgrade-benchmark-MEDIUM.txt` and `upgrade-benchmark-HIGH.txt`: raw measurements and assertions.
- `upgrade-benchmark-final.log` and `upgrade-benchmark-high.log`: final player logs.
- `upgrade-build-result.txt`: build summary.
- `../Builds/DeadSignal/Verification/`: standalone screenshots and latest raw reports.

The earlier `upgrade-benchmark-player.log` records issues found and fixed during development; use the final logs above for delivery status.
