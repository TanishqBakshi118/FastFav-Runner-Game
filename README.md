# FastFav Runner

**FastFav Runner** is a high-speed, endless runner where players must instantly shape-shift between a Car, Boat, and Plane to survive an ever-accelerating, procedurally generated gauntlet. 

🎮 **[Play the WebGL Build Here in your Browser](https://play.unity.com/api/v1/games/game/243332ac-8d95-48b5-9fd5-2217de388c32/build/latest/frame)**

## ⚙️ Programming & Design Highlights
This project was developed in **Unity (C#)** using the **Universal Render Pipeline (URP)**. As a solo developer on this project, my focus was on gameplay programming, system architecture, and player-experience design. 

* **Dynamic Vehicle State Machine (`PlayerController.cs`):** 
  Engineered a frictionless "Fast Fav" swapping system allowing the player to instantly transition between three distinct physical states (Car, Boat, Plane). The system dynamically toggles rigidbodies, gravity scales, and meshes while seamlessly maintaining forward momentum.
* **Procedural Track Generation (`WorldManager.cs`):** 
  Built a treadmill-style chunk spawner that infinitely generates the environment. Programmed specific overriding logic for specialized terrain variants (e.g., Canyon chunks) to restrict ground obstacles and force aerial hazards.
* **Weighted Spawning & Game Balance:** 
  Designed a percentage-based drop system for hazards across multiple lanes (Water, Road, Air). Implemented a rare 10% chance to spawn an Invincibility Star, dynamically adopting the exact coordinate data of the replaced hazard.
* **Dynamic Difficulty Scaling:** 
  Designed a difficulty curve where the global environment scroll speed accelerates over time, increasing the mechanical demand on the player the longer they survive.
* **Session-Based JSON Persistence:** 
  Implemented a JSON data structure to serialize, handle, and display high-score tracking within the active game memory. 
* **Performant Out-of-Bounds Detection:** 
  Utilized raw Y-coordinate math checks for canyon fall deaths rather than relying on physics-heavy trigger planes, ensuring exact hit-detection even at maximum game speeds.


## 🛠️ Built With
* Unity 2022.x (Universal Render Pipeline)
* C# 
* WebGL 

## 🎨 Asset Credits
The 3D models, textures, and primary audio/visual assets used in this project are licensed from the Unity Asset Store.
The core objective of this portfolio piece was to demonstrate backend systems programming, game loop design, and procedural generation mechanics.
