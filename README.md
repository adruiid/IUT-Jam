<div align="center">

# 🕯️ ASHFALL

**An isometric survival game about the hours between dusk and dawn.**

*Scavenge by day. Barricade by night. Decide who's worth the last can of food.*

[![Unity](https://img.shields.io/badge/Unity-6000.0%20LTS-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20(PC)-blue)]()
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](LICENSE)
[![Wishlist on Steam](https://img.shields.io/badge/Steam-Wishlist-1b2838?logo=steam)]()

<!-- Drop a looping gameplay GIF here — it does more than any paragraph below. -->
<img src="docs/media/hero.gif" alt="Ashfall gameplay" width="720"/>

</div>

---

## What it is

Ashfall is an **isometric, real-time survival game** built in Unity. The design DNA is *Darkwood*'s dread-soaked day/night split crossed with *60 Seconds*' brutal resource triage: daylight is for risk, darkness is for consequences.

- **Day** — leave the shelter to scavenge. The map is dangerous but you need fuel, food, meds, and materials. Every trip is a bet on how far you push before the light fails.
- **Night** — you cannot fight what's out there head-on. Board the windows, ration the lamp oil, and survive until dawn. What you *didn't* prepare for is what kills you.
- **Between** — decide who eats, who's healthy enough to scavenge tomorrow, and what you're willing to trade to see another sunrise.

> Not a horde shooter. A game about scarcity, bad options, and the noise outside the door.

---

## Features

- 🌗 **Day/night survival loop** — distinct scavenge and defense phases with a hard, escalating clock.
- 🎒 **Grid inventory & crafting** — weight and space matter; you can't take everything.
- 🔨 **Barricade system** — reinforce entry points; structures degrade under pressure.
- 🧑‍🤝‍🧑 **Survivor management** — hunger, health, sanity, and morale per character; hard choices with no clean answer.
- 🗺️ **Hand-authored + seeded maps** — a stable home layout with procedurally varied scavenge zones for replayability.
- 🕯️ **Light as a resource** — visibility is a currency you spend, not a given.
- 📻 **Event system** — narrative and random events (data-driven ScriptableObjects) that reshape a run.

---

## Screenshots

| Shelter | Scavenge run | Nightfall |
|---|---|---|
| ![](docs/media/shot_shelter.png) | ![](docs/media/shot_scavenge.png) | ![](docs/media/shot_night.png) |

---

## Built with

- **Engine:** Unity `6000.0 LTS` (Universal Render Pipeline)
- **Language:** C#
- **Input:** Unity Input System (keyboard + mouse, controller supported)
- **Architecture:** MonoBehaviour + ScriptableObject-driven data (items, events, enemies)
- **Persistence:** JSON save system (`Application.persistentDataPath`)

> **Assumption:** Unity 6 LTS + URP, PC-first. If you're on a different LTS, update `ProjectSettings/ProjectVersion.txt` expectations and the badge above.

---

## Getting started

### Prerequisites
- Unity Hub + Unity **6000.0 LTS** (install the exact version in `ProjectSettings/ProjectVersion.txt` to avoid reimport churn)
- Git with [Git LFS](https://git-lfs.com/) (art/audio are tracked via LFS)

### Clone & open
```bash
git lfs install
git clone https://github.com/<you>/ashfall.git
cd ashfall
```
Then in Unity Hub: **Add project from disk → select the cloned folder → open with the matching editor version.**

First open will take a few minutes while Unity imports and builds the Library. That's expected — don't commit `Library/`.

### Play
Open `Assets/_Project/Scenes/Boot.unity` and press ▶️. `Boot` loads into the main menu; **New Run** starts the loop.

---

## Building

**From the editor:** `File → Build Settings → Windows → Build`.

**Headless / CI:**
```bash
Unity -quit -batchmode -projectPath . \
  -executeMethod BuildScripts.BuildWindows \
  -logFile build.log
```
See `Assets/Editor/BuildScripts.cs` for build targets and output paths.

---

## Project structure

```
Assets/
├── _Project/              # Everything first-party lives under one root
│   ├── Scenes/            # Boot, MainMenu, Shelter, Scavenge_*
│   ├── Scripts/
│   │   ├── Core/          # Game loop, day/night clock, save system
│   │   ├── Survival/      # Needs (hunger/health/sanity), survivors
│   │   ├── Inventory/     # Grid inventory, items, crafting
│   │   ├── Defense/       # Barricades, night threats, spawn logic
│   │   ├── World/         # Map gen, scavenge zones, interactables
│   │   └── UI/            # HUD, menus, inventory screens
│   ├── Data/              # ScriptableObjects: items, recipes, events, enemies
│   ├── Art/               # Sprites, models, materials (LFS)
│   ├── Audio/             # SFX, music (LFS)
│   └── Prefabs/
├── Editor/                # Build scripts, custom inspectors, tooling
├── Plugins/               # Third-party
└── Settings/              # URP assets, input actions
```

**Convention:** all first-party content lives under `Assets/_Project/` so imported packages and store assets never mix with your own. Data over code — new items/events/enemies should be authorable as ScriptableObjects without touching C#.

---

## Roadmap

- [x] Core day/night loop
- [x] Grid inventory + crafting
- [ ] Barricade degradation & repair pass
- [ ] Survivor morale / sanity systems
- [ ] Procedural scavenge-zone generator
- [ ] Event/card system content
- [ ] Audio pass (adaptive night tension mix)
- [ ] Steam wishlist page + demo build

See [Issues](../../issues) for the granular backlog and [Projects](../../projects) for the current milestone.

---

## Contributing

This is a small-team project. If you're collaborating:

1. Branch from `develop`: `git checkout -b feature/your-thing`
2. Keep scenes out of merge hell — **one person edits a given scene at a time**, or use prefab-based composition.
3. Never commit `Library/`, `Temp/`, `Logs/`, `Build/` (see `.gitignore`).
4. Track binary art/audio with **Git LFS** (`.gitattributes` is configured).
5. Open a PR into `develop` with a short "what/why" and a GIF if it's visible.

Code style: standard C# conventions, `PascalCase` for public members, one MonoBehaviour per file, `[SerializeField] private` over public fields.

---

## Credits

- **Design / Programming:** you
- **Art:** TBD
- **Audio:** TBD
- Built during / for [jam or context].

---

## License

Released under the [MIT License](LICENSE). Art and audio assets may be under separate terms — see `Assets/_Project/Art/CREDITS.md`.

---

<div align="center">
<sub>Survive the night. Then do it again.</sub>
</div>
