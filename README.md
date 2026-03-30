# Mini Survival FPS - Unity Technical Test

## Unity Version
This project was developed using **Unity 2022.3.62f2**.

---

## How to Run
1. Open the project using Unity 2022.3.62f2  
2. Load the main scene  
3. Press Play in the editor  

---

## Controls
- WASD → Movement  
- Mouse → Look  
- Left Click → Shoot  
- Q → Switch weapon  
- Shift → Sprint  
- Space → Jump  
- Escape → Pause  

---

## Architecture Decisions

The project was built with a focus on scalability, modularity, and clear separation of responsibilities.

### Game Flow Management
A centralized GameManager handles the main game states:
- Menu
- Playing
- Pause
- GameOver

This allows UI and gameplay systems to react to state changes without tight coupling.

---

### Event-Driven Communication
An event system is used to decouple systems such as:
- UI updates
- player health
- wave progression
- game state transitions

---

### Input Abstraction
Input handling is separated into an InputController, keeping gameplay logic independent from the input system.

---

### Modular Player Design
The player is split into multiple components:
- Movement
- Camera
- Weapons
- Health

This avoids monolithic scripts and improves maintainability.

---

### Weapon System
Weapons are implemented using a base class (`WeaponBase`) with derived classes for specific behaviors (pistol, rifle, shotgun).  
This allows easy extension for future weapons.

---

### ScriptableObjects Usage
ScriptableObjects are used for configurable data:
- weapon stats
- enemy stats
- wave progression

This enables tuning without modifying code.

---

### Object Pooling
Pooling is implemented for:
- projectiles
- enemies

Enemies are reused between waves instead of being destroyed.

---

### Enemy Architecture
Enemies share a base class and are extended into specific behaviors:
- Aggressive enemies
- Flanker enemies

This structure avoids duplication while allowing variation.

---

### State Machine
Enemies use a simple state machine with states such as:
- Chase
- Attack
- Flee
- Dead

---

### Infinite Wave System
Waves are generated dynamically using progression rules:
- increasing enemy count
- scaling stats (health, speed, damage)
- unlocking enemy types over time

This removes the need for manual wave configuration.

---

### Progression System
The game tracks:
- current wave
- highest wave reached

This replaces the traditional score system and better fits the gameplay loop.

---

## What I Would Improve With More Time

### Gameplay
- Additional enemy types with more distinct behaviors  
- More weapons and gameplay variety  
- Progression systems (upgrades, perks)  
- Better balancing  

---

### Visual and Feedback
- Improved UI and UX  
- VFX and particle systems  
- Stronger visual feedback for hits, damage, and kills  
- More polished transitions  

---

### Audio
- Audio Mixer implementation  
- Volume controls (music / SFX)  
- Improved sound feedback and variation  

---

### Architecture
- Scene separation (menu, gameplay, persistent systems)  
- Improved system decoupling  
- Better debugging and tuning tools  

---

### General Polish
- Additional testing and edge case handling  
- Refinement of gameplay loop  
- Improved presentation  

---

## Notes
The focus of this project was to build a solid and scalable foundation, prioritizing code structure and maintainability over visual polish.