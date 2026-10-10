# Mascotchi

A Tamagotchi-style creature-raising game for fans of VTuber content creators.

Raise a mysterious blob through two growth stages until it evolves into one of your favourite VTuber mascots. Which mascot emerges depends entirely on how you play -- what activities you do, how well you care for your creature, and what habits you build over time. No thresholds. No evolution hints. Just your creature and the choices you make.

Built in Unity 2D for Windows, macOS, Linux, and Android. Designed to expand to new VTuber communities over time without touching the core code.

---

## Features

- Tamagotchi-style creature raising with real-time stat decay (Hunger, Stress/Sleep, Hygiene)
- Five hidden attributes (Music, Gaming, Yapping, Mischief, Knowledge) that shape evolution
- Five activities: Karaoke, Gaming, Streaming, Marbles, Coding
- Profile-based evolution -- the mascot whose personality best fits your play style wins
- Evolution reveal with an optional full-screen stinger image per mascot, also shown on launch and when swapping mascots
- Evolved mascots get a branded egg and a reduced Stress cost for their signature activity
- Your creature's nickname floats above it as it bounces around its screen
- Roster gallery -- every mascot you evolve is unlocked, and you can swap between them at any time
- Egg-only mode on PC -- shrink the window down to just the egg and keep your mascot on screen while you work or stream
- Music, sound effects, and separate volume controls
- Artist credits shown in settings for the active mascot
- Drop-in mascot packages -- add new mascots to an installed copy of the game, no Unity required
- Windows, macOS, Linux, and Android support

### Planned

- Multiplayer collab activities (v2.0)
- Mascot visual variants (v3.0)
- Android pop-out (picture-in-picture) player

---

## Download and Install

| Platform | Download | How to run |
| --- | --- | --- |
| Windows | `.zip` | Extract and run `Mascotchi.exe` |
| macOS | `-mac.zip` | Extract, move `Mascotchi.app` out of Downloads (into Applications or any other folder), then open it. The first time, macOS will block it: go to **System Settings > Privacy & Security**, click **Open Anyway**, and open it again. |
| Linux | `-linux.tar.gz` | Extract, then double-click `Mascotchi.x86_64` or run `./Mascotchi.x86_64` from a terminal in that folder. Also works on Steam Deck in desktop mode. |
| Android | `.apk` | Copy to your phone, tap it, and allow **Install unknown apps** when asked. Requires Android 8.0 or newer. |

### Save files

Your creature is saved automatically. Updating to a newer version keeps your save.

| Platform | Save location |
| --- | --- |
| Windows | `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\` |
| macOS | `~/Library/Application Support/<CompanyName>/<ProductName>/` |
| Linux | `~/.config/unity3d/<CompanyName>/<ProductName>/` |
| Android | App data -- removed if you uninstall the app |

On Android, install updates over the top of the existing app rather than uninstalling first, or your save will be lost.

---

## Adding a Mascot

Mascotchi is designed so that anyone can add a new mascot without touching the game's code. See the **Mascot Creator Guide** and [CONTRIBUTING.md](CONTRIBUTING.md) for the full mascot package standard.

The short version:

1. Create a folder named after your mascot
2. Fill in the `MascotName_Definition.json` template, including the `artistCredits` block
3. Add your sprite artwork to a `Sprites/` subfolder following the naming convention
4. Optionally add a `MascotName_Stinger.png` (480x854) to play on evolution, on launch, and when swapping to your mascot
5. Drop the folder into the game's `Mascots` folder and relaunch

### Where the Mascots folder is

The game creates the `Mascots` folder the first time it runs.

| Platform | Location |
| --- | --- |
| Windows | Next to `Mascotchi.exe` |
| macOS | Next to `Mascotchi.app` (don't run the app from Downloads, or the folder can't be found) |
| Linux | Next to `Mascotchi.x86_64` |
| Android | `Android/data/com.umedyn.mascotchi/files/Mascots/` -- reach it over USB; some phones hide this folder from file manager apps |
| Unity Editor | `Mascots/` at the project root, next to `Assets/` |

If a dropped-in mascot has the same name as a built-in one, the built-in mascot is kept and the dropped-in copy is skipped.

Mascots that ship with the game live in `Assets/Resources/Mascots/` inside the Unity project instead.

### Animation frames

Each animation should have **5 to 12 frames**. Fewer than 5 tends to look choppy; more than 12 adds art time without much visible benefit at the game's default playback speed of 5 frames per second. The Evolution animation is always exactly 5 frames. Frames are 64x64 px PNGs with transparency, numbered from `01`.

For mascots added to the Unity project, import all sprites with **Sprite Mode: Single** and **Filter Mode: Point (no filter)** to keep pixel art crisp. Dropped-in mascots get Point filtering automatically.

---

## Getting Started (Development)

### Requirements

- Unity 6 (developed on 6000.4)
- Unity Hub build modules for the platforms you want to build:
  - **Android Build Support** (with OpenJDK and Android SDK & NDK Tools)
  - **Mac Build Support (Mono)**
  - **Linux Build Support (Mono)**
- Python 3, to package Mac and Linux builds made on Windows

### Setup

1. Clone the repository
2. Open the project in Unity
3. Open `MainScene` in `Assets/Scenes/`
4. Press Play to run in the editor

### Building

All platforms build from **File > Build Profiles**. Select the platform, click **Switch Platform**, then **Build**. When saving, go *into* the target folder and enter `Mascotchi` as the file name -- otherwise Unity names the build after the folder.

| Platform | Settings | Output |
| --- | --- | --- |
| Windows | Default | `Builds/Windows/` -- zip the folder |
| macOS | Architecture: Intel 64-bit + Apple silicon | `Builds/Mac/Mascotchi.app` |
| Linux | Default | `Builds/Linux/` |
| Android | IL2CPP, ARM64, minimum API 26, custom keystore, **Build App Bundle** unticked | `Builds/Android/Mascotchi.apk` |

Delete any `*_BurstDebugInformation_DoNotShip` folder before packaging -- it is debug data and should not be distributed.

#### Packaging Mac and Linux builds on Windows

Windows can't mark files as executable, so Mac and Linux builds made on Windows won't launch if zipped normally. Use the packaging script instead, run from the `Builds/` folder:

```
python ../Tools/pack_build.py mac   Mac/Mascotchi.app  Mascotchi-<version>-mac.zip
python ../Tools/pack_build.py linux Linux              Mascotchi-<version>-linux.tar.gz
```

Upload the resulting files as-is. Unzipping and re-zipping them on Windows removes the permissions again.

#### Android signing

Android only installs an update if it is signed with the same key as the installed version. Keep the release keystore and its passwords backed up **outside** this repository, and increase **Bundle Version Code** for every release.

#### Special editions

Setting **Starter Mascot Id** on the GameManager makes every new game start already evolved as that mascot. Leave it empty for normal play.

---

## Project Structure

```
Assets/
  Art/
    Splash/           -- Logo shown after the Unity splash screen
  Audio/
    Music/            -- Main theme
    SFX/              -- Action, activity, movement, and UI sounds
  Resources/
    Creature/
      Blob/           -- The Blob, loaded separately from the mascot roster
    Mascots/          -- Built-in mascots, one subfolder each
  Scripts/
    Core/             -- GameManager, TimeManager, SaveSystem, MascotLoader, AudioManager
    Creature/         -- CreatureBase, EvolutionTracker, CreatureAnimator
    UI/               -- Panels, settings, roster gallery, evolution reveal, egg-only mode
  Scenes/
    MainScene
Mascots/              -- Drop-in mascots for testing in the editor (git-ignored)
Tools/
  pack_build.py       -- Packages Mac and Linux builds made on Windows
Builds/               -- Build output (git-ignored)
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
