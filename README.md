# More Pots

![Game](https://img.shields.io/badge/game-Schedule%20I-2f5d3a)
![Loader](https://img.shields.io/badge/MelonLoader-0.7.x-blue)
![Branches](https://img.shields.io/badge/branch-Mono%20%7C%20IL2CPP-lightgrey)

A [MelonLoader](https://github.com/LavaGang/MelonLoader) mod for **Schedule I** that adds two new grow pots, each with its own 3D model. They're a small step up from the vanilla pots and are sold only by **Oscar** at the warehouse.

## The pots

| Pot | Price | Growth speed | Moisture loss | Yield | Upgrade over |
|---|---|---|---|---|---|
| **Terracotta Pot** | $65 | +5% | 45% slower | same | Moisture-Preserving Pot ($50, 40% slower) |
| **Modern Pot** | $150 | +20% | 15% faster | same | Air Pot ($120, +15% growth, 30% faster) |

- **Terracotta Pot:** a thick fired-clay pot on a saucer that keeps the soil cool and damp.
- **Modern Pot:** a sage-glazed ceramic planter on an oak stand, with a built-in drainage reservoir.

Stats are relative to the Plastic Pot. Both pots stack 10 to a slot and use the usual 2x2 footprint.

## Features
- **Custom models** built into the DLL. There's nothing extra to download.
- **Oscar exclusive.** You buy them in person at the warehouse. They aren't in the phone delivery app.
- **Works like any other pot.** Soil, watering, additives, botanists, co-op and saving all work as normal.
- **Configurable** prices and stats.

## Installation
1. Install [MelonLoader 0.7.x](https://github.com/LavaGang/MelonLoader/releases) for Schedule I.
2. Download the zip for your game branch from [Releases](../../releases):
   - **IL2CPP** (the default Steam branch): `MorePots_IL2CPP.dll`
   - **Mono** (the "alternate" beta branch): `MorePots.dll`
3. Copy the DLL into `...\steamapps\common\Schedule I\Mods\`.
4. Launch the game and visit Oscar at the warehouse.

Install only one of the two DLLs. A Mono mod won't load on IL2CPP, and an IL2CPP mod won't load on Mono.

## Configuration
After the first launch, edit `UserData/MelonPreferences.cfg`:

```ini
[MorePots]
TerracottaPrice = 65.0
TerracottaGrowSpeed = 1.05       # 1.0 = Plastic Pot
TerracottaMoistureDrain = 0.55   # 0.6 = Moisture-Preserving Pot
TerracottaYield = 1.0
ModernPrice = 150.0
ModernGrowSpeed = 1.2            # 1.15 = Air Pot
ModernMoistureDrain = 1.15       # 1.3 = Air Pot
ModernYield = 1.0
```

Restart the game after changing these. The item descriptions update to match.

## Building from source
Requirements:
- The [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Python 3. This is only needed if you change a model.
- Your own copy of Schedule I with MelonLoader

The build references the DLLs in your own game folder. Nothing from the game is committed to this repo.

```powershell
./build.ps1 -Install           # Mono build    -> MorePots.dll
./build.ps1 -Il2Cpp -Install   # IL2CPP build  -> MorePots_IL2CPP.dll
./build.ps1 -GameDir "D:\Games\Schedule I"   # if the game is somewhere else
```

The script finds the right `Schedule I*` install for each branch under your Steam `common` folder. The IL2CPP build needs `MelonLoader/Il2CppAssemblies`, which MelonLoader generates the first time the game is launched.

### Models
```
Models/
  Terracotta Pot/terracotta_pot.glb   source model
  Terracotta Pot/terracotta_pot.bin   baked, embedded in the DLL
  Modern Pot/modern_pot.glb
  Modern Pot/modern_pot.bin
```
- The game has no glTF loader, so `tools/glb_to_pot.py` bakes each `.glb` into a small `.bin`. `build.ps1` re-bakes automatically whenever a `.glb` is newer than its `.bin`.
- Each model needs a node named `cavity` (the inside of the pot). Its base should sit at y = 0.
- In game, each pot is scaled so a full pot of soil sits at the same height as in the Plastic Pot, and the soil is resized to fit the cavity.

## How it works
| File | What it does |
|---|---|
| `MorePots.cs` | Entry point, the pot list and settings. Registers the pots each time the Main scene loads. |
| `PotSpec.cs` | Per-pot settings and the generated item description. |
| `PotFactory.cs` | Clones the Plastic Pot's prefab and item definition for each new pot, applies the stats, and registers it with the item registry and FishNet. |
| `PotModel.cs` | Builds the custom model, hides the plastic mesh, and fits the soil to the new cavity. The game builds its own Air and Moisture-Preserving pots the same way. |
| `PotIcon.cs` | Renders the item icon with the game's own preview renderer. |
| `OscarShop.cs` | Adds the in-person listings to Oscar's shop. |
| `Compat.cs`, `GlobalUsings.cs` | The differences between the Mono and IL2CPP builds. |
| `tools/glb_to_pot.py` | Model baker. |

## Notes
- **Uninstalling:** any placed modded pots, and the plants in them, are lost the next time the save loads. Harvest and pick them up first.
- **Co-op:** every player needs the mod installed.
- **Back up your saves** before trying any mod.

## Credits
- **Mod and models:** Ashton.
- No game files or decompiled code are included in this repo or its releases.
- Schedule I is developed by TVGS. This is an unofficial fan mod and isn't affiliated with or endorsed by TVGS.
