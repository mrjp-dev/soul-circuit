# Soul Circuit: domain language

Terms used in code. Design material lives in `Planning/` (gitignored); this file carries
only the vocabulary the codebase needs, so names in issues, tests, and refactors match.

## Non-playables

`NP` in code means **non-playable**. It is the parent of two kinds:

| Term | Code | Meaning |
|---|---|---|
| Non-playable | `NonPlayable` | Base behaviour for anything in the world the player does not control |
| Non-playable character | `NonPlayableCharacter` | An NP that talks or reacts. Kinds: Androids, Animals, Cyborgs, Enemies, Robots, Servants |
| Non-playable object | `NonPlayableObject` | An NP that is scenery or a fixture, not a character |
| Save point | `SavePointControl` | A non-playable object that writes a save when used |

Each has a paired `ScriptableObject` for data: `NPData`, `NPCData`, `NPOData`.

An **item** is a non-playable object the player can pick up. Pickup is not implemented
yet: `PlayerInteraction.Interact` logs a warning for items.

`NPCData` carries allegiance as three independent booleans (`isHostile`, `isFriendly`,
`isNeutral`), so a character can currently be more than one at once.

## How a non-playable is recognised

Three mechanisms answer three different questions. They are not competing classifications.

| Mechanism | Question | Where |
|---|---|---|
| Type hierarchy | What is it? | `NonPlayable` and subclasses, plus their `NPData` |
| Unity's built-in `GameObject.tag` | Can it be approached? | `ToggleInteractable` writes `"Interactable"`; the player's trigger reads it |
| `CustomTag` string list | What can it do? | `"NPC"` turns to face the player, `"Item"` is picked up |

Only `"NPC"` is applied in any scene today.

## Playable characters

More than one character is playable, each with a distinct combat style.

| Name | Role | Style |
|---|---|---|
| Heisse | Initial player character | Melee, close combat |
| Gauss | Secondary player character | Ranged "magic", also melee |

Note: an earlier design idea to swap focus between protagonist and antagonist is **not
current**. Do not treat it as live.

## Characters appearing in dialogue

Irene (also Irrlicht) is a deity. Gauss and Irene are the two names currently present in
`Assets/Dialogues/`; the wider cast in `Planning/Characters/` is not yet in code.

## Interaction

The player has an **interact collider**. An NP inside it becomes the **current
interactable**, tagged via `CustomTag`. Interacting opens dialogue or triggers the NP.

## Camera zoom

Zoom is stepped, not continuous: 1x, 2x, 4x, 5x, 8x, 10x, 20x, 40x. Usable zoom is capped
by resolution, since the UI distorts past a point (360p caps at 1x, 720p at 2x, 1080p at
3x, 1440p at 4x).

## Design concepts not yet in code

Present in `Planning/`, absent from the codebase. Do not invent implementations for these;
if work starts on one, add it above.

- **Crystals** . seven of them, collected to halt the world's decay. Drives the plot.
- **Roadtrip** . the travel structure connecting locations.
- **Weapon skill tree**.
- **Puzzles** and **minigames**.
