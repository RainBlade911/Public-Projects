# Bad Omen — Turn-Based Roguelike

Bad Omen is a turn-based roguelike developed in Unity and C#. The game combines strategic turn-based combat with randomized encounters, elemental affinities, persistent player progression, and a reward system that allows the player's build to evolve throughout a run.

The project was developed as a team-based university game development project.

## Gameplay

Players progress through a series of encounters against randomized enemies and bosses. During combat, the player chooses between attacks, abilities, and items while managing health, mana, enemy weaknesses, and turn order.

After defeating enemies, the player receives randomized upgrades that can improve their stats, moveset, or inventory.

## Core Systems

### Turn-Based Combat

Combat is controlled through a turn-management system that determines player and enemy actions based on character speed.

Player actions include:

* Attacking enemies
* Selecting different moves
* Using consumable items
* Managing mana
* Targeting individual enemies

Enemy turns are automatically processed after the player's actions are complete.

### Elemental Affinities

Moves and enemies use an affinity system that modifies damage based on elemental effectiveness.

* Super-effective attacks deal increased damage
* Resistant enemies take reduced damage
* Combat UI communicates effectiveness to the player

### Player Progression

Player statistics persist between battles during a run, including:

* Current health
* Maximum health
* Current mana
* Maximum mana
* Learned moves
* Inventory items

## Reward System

After combat, players can receive several types of upgrades.

### Equipment Upgrades

Equipment rewards can increase attributes such as maximum health and maximum mana.

### Moveset Upgrades

Players can unlock additional attack abilities and expand their available moveset.

### Item Rewards

Consumable items can restore health or mana and can be carried into future encounters.

## Reward Wheel

The reward system includes animated reward wheels for:

* Equipment
* Moves
* Items

Each wheel spins through multiple possible rewards before selecting the final upgrade.

## Boss Rewards

Boss encounters provide a separate progression system using special upgrades.

### Essence of Strength

Increases the damage of the player's Strike ability.

### Essence of Knowledge

Increases the damage of all player abilities.

## Technical Features

The project includes:

* Turn-based combat management
* Speed-based action ordering
* ScriptableObject-based moves, enemies, and items
* Elemental affinity calculations
* Dynamic enemy targeting
* Health and mana UI systems
* Persistent player statistics
* Randomized enemy encounters
* Animated reward wheels
* Moveset progression
* Inventory management
* Boss encounters
* Boss-specific progression rewards
* UI transitions and fades

## Technologies

| Technology              | Purpose                                           |
| ----------------------- | ------------------------------------------------- |
| Unity                   | Game engine                                       |
| C#                      | Gameplay and systems programming                  |
| Unity ScriptableObjects | Moves, enemies, and item data                     |
| Unity UI                | Combat menus, health bars, rewards, and targeting |

## Development

The project was created as part of a university game development course and was developed collaboratively using Git for version control.

My contributions focused on gameplay programming and system integration, including combat mechanics, progression systems, UI behavior, rewards, and debugging interactions between these systems.
