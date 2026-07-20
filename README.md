<div align="center">

# 🌑 NIGHTFALL

**Gather by day. Fortify by dusk. Survive three nights.**

*The sun keeps them away. When it sets, everything that's out there comes for you — and you'd better be ready.*

[![Unity](https://img.shields.io/badge/Unity-6000.5-black?logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/render-URP-blue)]()
[![Platform](https://img.shields.io/badge/platform-Windows%20(PC)-blue)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](LICENSE)

<!-- Drop a looping gameplay GIF here — it sells the game better than any text below. -->
<img src="docs/media/hero.gif" alt="Nightfall gameplay" width="720"/>

</div>

---

## What it is

Nightfall is a **third-person, isometric survival game** built in Unity. It runs on a hard day/night clock: **days are for gathering and building, nights are for holding the line.** Make it through **three nights** and you win.

- **Day (06:00–20:00)** — the world is safe. Chop trees, mine stone and iron, dig for buried loot, cook food to stave off hunger, and craft the gear you'll need. Repair your turrets and spend at the shopkeeper.
- **Dusk** — the light shifts early, like real twilight. That's your warning: get back to base.
- **Night (20:00–06:00)** — a horde spawns around you and closes in. Fight with rifle and blade, let your turrets thin the crowd, and stay alive. Each night is faster, denser, and nastier than the last.
- **Dawn** — daybreak burns the monsters away. Rebuild and do it again… twice more.

---

## Features

- 🌗 **Self-driving day/night cycle** — a real clock (starts 06:00), a rising day counter, dynamic sun (rotation, colour temperature, intensity) that eases through dawn/dusk, and day/night music that fades in and out.
- 🪓 **Interaction & gathering** — chop trees for wood, mine rock/iron, dig for loot, repair structures, talk to the shopkeeper. The right tool (axe / pickaxe / shovel) appears in hand for each job; **click a target to auto-path to it**, or hit **E** up close.
- 🎒 **Inventory, crafting & cooking** — stackable resource inventory, craft tools/weapons from what you gather, and cook consumables that restore hunger.
- 🍖 **Survival needs** — health and hunger, with feedback SFX as you eat and when you're starving.
- 🔫 **Combat** — a bolt-action rifle (only usable once you've crafted one) that aims at the cursor with a magazine, reload, muzzle flash and recoil pause; plus an always-ready **melee dagger** (press **V**, or right-click an enemy to charge in and strike).
- 🛡️ **Repairable turrets** — auto-tracking Y-axis turrets that lock the closest target and fire until out of ammo, then power down until you repair the base to reload them.
- 🧟 **Five enemy types** — Zombie, Ghoul, Girl Scout, Mutant, and Parasite, each with a distinct move-speed / attack-speed / damage / health profile.
- 🌊 **Escalating horde director** — enemies spawn in a ring around you on the NavMesh, with **safe zones** (base, barn, hut) excluded and a grace delay each night. Every night the spawn rate ×1.5, the alive-cap grows, and the mix shifts from grunts toward elites.
- 🐄 **Wildlife** — chickens and cows that wander their home range, take hits (with a red flash), and drop resources.
- 🎥 **Isometric camera** — Cinemachine follow with mouse-wheel zoom that ignores scroll while you're over UI.

---

## Controls

| Input | Action |
|---|---|
| **WASD / Arrows** | Move |
| **Mouse wheel** | Zoom camera |
| **Left-click** on a highlighted object | Auto-walk to it and interact (chop / mine / dig / repair / shop) |
| **Left-click** elsewhere | Draw the rifle and fire toward the cursor *(if you own one)* |
| **E** | Interact with the nearest object in range |
| **R** | Reload |
| **V** | Melee the closest enemy in range |
| **Right-click** an enemy | Run up to it and melee |

Keyboard + mouse is the primary scheme; movement/interaction also work on a gamepad via the shared input actions.

---

## Built with

- **Engine:** Unity `6000.5` (Universal Render Pipeline)
- **Language:** C#
- **Input:** Unity Input System (keyboard + mouse, partial controller)
- **Navigation:** AI Navigation 2.0 (NavMesh) — enemy pathing, crowd avoidance, click-to-move
- **Camera:** Cinemachine 3.1 (3rd Person Follow)
- **Character base:** Unity Starter Assets (ThirdPersonController), extended with a movement-lock flag for interactions/combat
- **Architecture:** MonoBehaviour + ScriptableObject data (items, recipes); a shared `IDamageable` interface connects bullets, melee, turrets, enemies, animals, and the player

---

## Getting started

### Prerequisites
- Unity Hub + Unity **6000.5** (match `ProjectSettings/ProjectVersion.txt` to avoid reimport churn)
- Git (with Git LFS if you add large art/audio)

### Clone & open
```bash
git clone https://github.com/adruiid/IUT-Jam.git
cd IUT-Jam
```
Then in Unity Hub: **Add → select the cloned folder → open with the matching editor version.** First import builds the `Library/` (a few minutes) — that's expected and not committed.

### Play
Open the main gameplay scene under `Assets/Scenes/`, bake the NavMesh if prompted, and press ▶️.

---

## Project structure

```
Assets/
├── Scripts/
│   ├── Interaction/    # Interactable base, PlayerInteractor, auto-walk,
│   │                   #   ResourceNode (log/mine), DigSpot, RepairableStructure, Shopkeeper
│   ├── Combat/         # PlayerCombat (gun + dagger), Weapon, Bullet, Turret, IDamageable
│   ├── Enemies/        # EnemyAI, EnemyHealth, HitFlash, EnemySpawner (horde director)
│   ├── Animals/        # AnimalAI (chicken / cow)
│   ├── Inventory/      # Inventory, items, consumables, pickups
│   ├── Crafting/       # Crafting + cooking
│   ├── Managers/       # DayNightController (clock, sun, music, day counter, win event)
│   ├── Player/         # PlayerStatusBasic (health / hunger)
│   ├── Camera/         # CameraZoom
│   ├── UI/             # DayNightUI and other HUD binders
│   ├── Main Menu/  ·  Pause Menu/
│   └── Audio/          # VolumeSettings
├── Starter Assets/     # ThirdPersonController (Unity), extended in-repo
├── Prefabs/            # Enemies, weapons, turrets, resources, UI
└── Scenes/
```

**Convention:** standard C# style, one MonoBehaviour per file, `[SerializeField] private` over public fields, tunable numbers exposed to the Inspector. Enemies, drops, tools and reactions are data-driven so new content rarely needs code.

---

## Credits

- **Team:** Bad Wifi Interactive
- Built for the **IUT Jam**.
- Third-party: Unity Starter Assets, JMO Assets **WarFX** (muzzle/impact VFX), plus environment/audio packs — see individual asset folders for their licenses.

---

## License

Project code is released under the [MIT License](LICENSE). Bundled third-party art, audio, and VFX remain under their original licenses.

---

<div align="center">
<sub>Three nights. Hold out until dawn.</sub>
</div>
