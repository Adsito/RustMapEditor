# Rust Map Making Unity Editor

**Make sure you're using a Unity 2018 version. 2018.2.0b7 is confirmed working, not sure about 2018.1.**

DOWNLOAD LINK:
https://unity3d.com/unity/beta/unity2018.2.0b7

Set your Unity .NET version to 4.0.
Edit > Project Settings > Player > Other Settings > Configuration > Scripting Runtime Version

### Rust Map Making
https://discord.gg/VQruSpk

### Contribute
Anyone experienced in C# or Unity is welcome to help contribute and update the editor, if you have any questions feel free to check out the discord.

## Features
- Extended SDK that allows Prefab loading direct from game content files.
- Importing and exporting .map files.
- Ability to edit
  - Terrain Map
  - Ground Textures
  - Biomes
  - Topology
  - Alpha Map
- Simple painting tools to easily edit map features.
- Spawn in prefabs such as monuments and decor.
- Paint terrain based on height and slopes.
  
## Notes

- <b>Make sure you're on Unity version 2018.2.0b7. Unity 2018.2.0b11 DOES NOT WORK!</b>
- Use .NET 4.0
- Maps need to be hosted on websites in order for other players to download them (You can use a local path if just testing on your own network)
- If you get a FileMismatch error that means you already have the file but it doesn't match the one on the server. To delete the copy of the map on your PC just open console in game by hitting F1 and look for the map file name it was trying to download. Then find it in this directory "C:\Program Files (x86)\Steam\steamapps\common\Rust\maps" and delete it. If you're still getting this error that means the server already has the map but it doesn't match the one being downloaded from the website. 
- Moving terrains around won't do anything. The editor will only export the heightmap.
- !Important! Water needs to be at 500. It can go over but if it's under 500 it will still count as water but look like land. Use the flatten option from the terrain editor to set it to 500.
- If you get a `NullReferenceException: Object reference not set to an instance of an object` error message you probably have water disabled. Select the `Water` object from the Hierarchy and in the Inspector select the checkbox by its name to enable it.
- Rust's alphamaps resolution can only go upto 2048, so maps made over 4096 in size will have less detail per tile as the heightmap will be twice the resolution.


## How to use the Editor

### 1) Opening the Project
Make sure you're on Unity 2018.2 and then launch Unity. When the Unity projects window opens up click on Open. Do not create a new project. Select the folder that contains the files downloaded from GitHub. Load `Dezinated's Rust Map Editor` project and it should take you into the editor view. Then at the bottom of the editor view there should be a file explorer; navigate to the Scenes folder and open `SampleScene`.

### 2) Loading a Map
On the left side of the editor screen, in the Hierarchy you will see all of the object the scene contains. Click on the `MapIO` object and you will notice map option on the right side of the editor in the Inspector. Import/Export buttons are to load and save .map files.

### 3) Editing Map Features
Again with `MapIO` selected you can switch between editing different map features. The dropdown list will contain Ground, Biome, Alpha, and Topology. Simply select a feature you would like to edit and then click on `Land` in the Hierarchy. From there you will see the Terrain component on the right side in the Inspector. Changing the terrain height will change the heightmap no matter which feature you are editing. To change features go to the texture painting tool on the terrain options. Depending on which feature you are editing there will be different textures available to use.

<b>What are the different features you can edit?</b>
- <b>Ground</b>: The ground textures you see when walking around in game. (Sand, dirt, rock, grass, etc.)
- <b>Biome</b>: Affects how the ground textures are coloured and what type of foilage spawns. (Arid, Arctic, Tundra, Temperate)
- <b>Alpha</b>: Makes parts of the map invisible. This is used for cave entrances and underground tunnels. Basically anything you need to punch a hole in the map for.
- <b>Topology</b>: One of the most fun features to mess with. This controls quite a few things and I haven't messed around with all of them yet. With this you're able to flag certain areas and control what happens there. For instance areas marked beach will spawn boats and count as spawn points for naked. Setting areas to `RoadSide` will make them spawn road loot.
	- <b>List of Topologies</b>: `Field`, `Cliff`, `Summit`, `Beachside`, `Beach`, `Forest`, `Forestside`, `Ocean`, `Oceanside`, `Decor`, `Monument`, `Road`, `Roadside`, `Swamp`, `River`, `Riverside`, `Lake`, `Lakeside`, `Offshore`, `Powerline`, `Runway`, `Building`, `Cliffside`, `Mountain`, `Clutter`, `Alt`, `Tier0`, `Tier1`, `Tier2`, `Mainland`, `Hilltop`


### 4) Using Prefabs
Prefabs still need to be worked on a bit. How it's currently being done is every Prefab that spawns on the map is represented by a white cube in the editor. When the map is saved and loaded in to Rust, the client spawns the actual prefab there. In the editor if you load a map you will notice white cubes everywhere. Clicking on a white cube will show you information about that Prefab and moving it around will update it's position values. If you use your file explorer to naviate to Resources/Prefabs/Monuments you will notice I made a Prefab called `Dome`. To make Prefabs like this simply select a Prefab that you have in your scene and drag it in to the file explorer to save it. You can then rename it to anything you like and drag it back in to the Scene view as many times as you'd like.

Prefabs also have an option to load them from the game files on play. If you press the play button the editor will load the actual Prefab from the game files.

- Extended SDK to allow loading prefabs from bundle files
	- Warning: Unity will use alot RAM if you load prefabs from the game files. So don't press play if you have unsaved changes.
	- The prefab loading is really basic, it works but its not the best
	- Prefabs from core/building won't load.

	
### 5) Rotating Maps
	- Rotate Clockwise or Counterclockwise.
	- Rotate All Splatmaps (Ground, Biome, Alpha, Topology), Prefabs and Heightmaps (Land and Water).
	- To rotate map, select MapIO and select either 'Rotate 90° or Rotate 270°'
	- For Topologies each topology layer will need to be rotated seperately for now. Should be fixed shortly.
	
