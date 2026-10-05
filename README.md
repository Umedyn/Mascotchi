# Mascotchi

A Tamagotchi-style creature-raising game for fans of VTuber content creators.

Raise a mysterious blob through two growth stages until it evolves into one of your favourite VTuber mascots. Which mascot emerges depends entirely on how you play -- what activities you do, how well you care for your creature, and what habits you build over time. No thresholds. No evolution hints. Just your creature and the choices you make.

Built in Unity 2D for PC and Android. Designed to expand to new VTuber communities over time without touching the core code.

---

## Features

- Tamagotchi-style creature raising with real-time stat decay (Hunger, Stress/Sleep, Hygiene)
- Five hidden attributes (Music, Gaming, Yapping, Mischief, Knowledge) that shape evolution
- Five activities: Karaoke, Gaming, Streaming, Marbles, Coding
- Profile-based evolution -- the closest personality match wins, not the highest counter
- Evolution reveal with an optional full-screen stinger image per mascot
- Evolved mascots get a branded egg and a reduced Stress cost for their signature activity
- Roster gallery -- every mascot you evolve is unlocked, and you can swap between them at any time
- Artist credits shown in settings for the active mascot
- Music and SFX volume controls
- Drag-and-drop mascot package system -- anyone can add a new mascot
- PC and Android support

### Planned

- Multiplayer collab activities (v2.0)
- Mascot visual variants (v3.0)

---

## Getting Started

### Requirements

- Unity 6 (developed on 6000.4)
- Android Build Support module (for Android builds)

### Setup

1. Clone the repository
2. Open the project in Unity
3. Open `MainScene` in `Assets/Scenes/`
4. Press Play to run in the editor

### Building

- **PC:** File > Build Profiles > Windows or macOS > Build
- **Android:** File > Build Profiles > Android > Switch Platform > Build

---

## Adding a Mascot

Mascotchi is designed so that anyone can add a new mascot without touching the game's code. See [CONTRIBUTING.md](CONTRIBUTING.md) for the full mascot package standard and creator guidelines.

The short version:

1. Create a folder inside `Resources/Mascots/` named after your mascot
2. Fill in the `MascotName_Definition.json` template, including the `artistCredits` block
3. Add your sprite artwork to the `Sprites/` subfolder following the naming convention
4. Optionally add a `MascotName_Stinger.png` (480x854) to play on evolution, on startup, and when swapping to your mascot
5. Drop the folder in -- the game detects it automatically on next launch

### Animation frames

Each animation should have **5 to 12 frames**. Fewer than 5 tends to look choppy; more than 12 adds art time without much visible benefit at the game's default playback speed of 5 frames per second. Frames are 64x64 px PNGs with transparency, numbered from `01`.

In Unity, import all sprites with **Sprite Mode: Single** and **Filter Mode: Point (no filter)** to keep pixel art crisp.

---

## Project Structure

```
Assets/
  Resources/
    Creature/
      Blob/           -- The Blob, loaded separately from the mascot roster
    Mascots/          -- One subfolder per mascot
  Scripts/
    Core/             -- GameManager, TimeManager, SaveSystem, MascotLoader
    Creature/         -- CreatureBase, EvolutionTracker, CreatureAnimator
    UI/               -- Panels, settings, roster gallery, evolution reveal
  Scenes/
    MainScene
```

---

## License

Mascotchi is open source under a custom non-commercial license. You are free to use, modify, fork, and distribute this project as long as:

- You do not charge money for access or put it behind a paywall
- You credit the original project
- Your fork carries the same license terms
- Any mascot content based on a real creator's likeness has their express consent

See [LICENSE](LICENSE) for the full terms.

---

## Contributing

Pull requests are welcome. For significant changes please open an issue first to discuss what you would like to change.

All contributors must read and agree to the terms in [CONTRIBUTING.md](CONTRIBUTING.md) before submitting mascot packages or assets.

---

## Acknowledgements

Built for and with the Lab Brats VTuber community -- Shoomimi, Ellie_Minibot, MinikoMew, and Chrchie.
