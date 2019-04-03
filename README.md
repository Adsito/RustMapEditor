# Rust Map Making Unity Editor

**Make sure you're using a Unity 2018.3.4f1 .**
Download Unity Hub: https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe
After installing Unity Hub, download the Unity 2018.3.4f1 from here, clicking the Unity Hub button: https://unity3d.com/get-unity/download/archive

### Rust Map Making
https://discord.gg/VQruSpk

### Contribute
Anyone experienced in C# or Unity is welcome to help contribute and update the editor, if you have any questions feel free to check out the discord. https://discord.gg/VQruSpk

### Credits
The foundations of this Editor is based on the hard work from Dezinated. He has put in a lot of work into this Editor, and he has a patreon if anyone would like to support his work.
https://github.com/JasonM97/Rust-Map-Editor
https://www.patreon.com/dezinated

Also using Assets from:
-https://assetstore.unity.com/packages/2d/textures-materials/floors/yughues-free-ground-materials-13001 for the Grass Texture.

-https://assetstore.unity.com/packages/tools/terrain/terrain-toolkit-2017-83490 for Terrain Toolkit 2017.

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
- Maps need to be hosted on websites in order for other players to download them (You can use a local path if just testing on your own network)
- If you get a FileMismatch error that means you already have the file but it doesn't match the one on the server. To delete the copy of the map on your PC just open console in game by hitting F1 and look for the map file name it was trying to download. Then find it in this directory "C:\Program Files (x86)\Steam\steamapps\common\Rust\maps" and delete it. If you're still getting this error that means the server already has the map but it doesn't match the one being downloaded from the website. 
- Moving terrains around won't do anything. The editor will only export the heightmap.
- !Important! Water needs to be at 500. It can go over but if it's under 500 it will still count as water but look like land. Use the flatten option from the terrain editor to set it to 500.
- If you get a `NullReferenceException: Object reference not set to an instance of an object` error message you probably have water disabled. Select the `Water` object from the Hierarchy and in the Inspector select the checkbox by its name to enable it.
- Rust's alphamaps resolution can only go upto 2048, so maps made over 4096 in size will have less detail per tile as the heightmap will be twice the resolution.
- If the scene view camera clips through the terrain when too close, click on the terrain with Middle Mouse Button to have Unity refocus on the terrain and turn off clipping of the terrain.

## How to use the Editor

### 1) Opening the Project
Make sure you are on Unity 2018.3.4 and then launch Unity. We recommend using Unity Hub to open the editor.
When you open Unity Hub a projects window will appear, click on Open. Do not create a new project. Select the folder that contains the files downloaded from GitHub. You will then see the Project in the window. Click on the three dots "..." on the right and make sure you are on Unity version 2018.3.4 and click "Open". The Unity project along with our editor scripts will now be loaded into Unity.

### 2) Loading a Map
On the left side of the editor screen, in the Hierarchy you will see all of the object the scene contains. Click on the `MapIO` object and you will notice map options on the right side of the editor in the Inspector. Import/Export buttons are to load and save .map files.

### 3) Editing Map Features
Again with `MapIO` selected you can switch between editing different map features. The dropdown list will contain Ground, Biome, Alpha, and Topology. Simply select a feature you would like to edit and then click on `Land` in the Hierarchy. From there you will see the Terrain component on the right side in the Inspector. Changing the terrain height will change the heightmap no matter which feature you are editing. To change features go to the texture painting tool on the terrain options. Depending on which feature you are editing there will be different textures available to use.

<b>What are the different features you can edit?</b>
- <b>Ground</b>: The ground textures you see when walking around in game. (Sand, dirt, rock, grass, etc.)
- <b>Biome</b>: Affects how the ground textures are coloured and what type of foilage spawns. (Arid, Arctic, Tundra, Temperate)
- <b>Alpha</b>: Makes parts of the map invisible. This is used for cave entrances and underground tunnels. Basically anything you need to punch a hole in the map for.
- <b>Topology</b>: One of the most fun features to mess with. This controls quite a few things and I haven't messed around with all of them yet. With this you're able to flag certain areas and control what happens there. For instance areas marked beach will spawn boats and count as spawn points for naked. Setting areas to `RoadSide` will make them spawn road loot.
	- <b>List of Topologies</b>: `Field`, `Cliff`, `Summit`, `Beachside`, `Beach`, `Forest`, `Forestside`, `Ocean`, `Oceanside`, `Decor`, `Monument`, `Road`, `Roadside`, `Swamp`, `River`, `Riverside`, `Lake`, `Lakeside`, `Offshore`, `Powerline`, `Runway`, `Building`, `Cliffside`, `Mountain`, `Clutter`, `Alt`, `Tier0`, `Tier1`, `Tier2`, `Mainland`, `Hilltop`


### 4) Using Prefabs
Prefabs are shown in the editor as white cubes. Prefabs also have an option to load them from the game files on play. 
If you press the play button the editor will load the actual Prefab from the game files. Make sure you remember to select your bundles file so the director field is filled when pressing play.

#Note: 
Prefabs from the Assets/prefabs path do not load currently.
- Warning: Unity will use alot RAM if you load prefabs from the game files. So don't press play if you have unsaved changes.
- The prefab loading is really basic, it works but its not the best
- Any changes to the map in play mode need to be saved by exporting the map before unloading the scene.
	
### 5) Rotating Maps
	- Rotate Clockwise or Counterclockwise.
	- Rotate All Splatmaps (Ground, Biome, Alpha, Topology), Prefabs, Paths and Heightmaps (Land and Water).
	- To rotate map, select MapIO and select either 'Rotate 90° or Rotate 270°'
	
### 6) Generating Layers
	- Generate certain Ground, Biome and Topology layers based on certain rules.
	
	Ground: 
	Sand = 0 - 502
	Rock = Slopes of 45° or higher
	
	Biome:	
	Arctic = 750 - 1000
	
	Topology: 
	Cliff = Slopes of 50° or higher
	Ocean = 0 - 498
	Offshore = 0 - 475
	Beach = 500 - 502
	Oceanside = 500 - 502
	Mainland = 500 - 1000
	Tier 0 = Bottom third of map
	Tier 1 = Middle third of map
	Tier 2 = Top third of map
	
### 7) Painting Layers
	- Paint layers based on height, slope, area and clear layers.
	- Paint any areas where the alpha is active with rock texture, to remove floating grass ingame.
	- Paint topologies, ground and biome layers if another layer has a selected texture.
		eg: Paint all snow textures with arctic or snow biomes.
	- Toggle blending within certain parameters in the Slope and Height tools.
	- Invert Topology and Alpha layers.

### 8) Copying Layers
	- Copy a texture from one layer, and paint it to another.
	  eg: For every Grass texture on the Ground map, paint the biome to be Temperate.
	- For topologies, copying and pasting will reference the active texture (The Green Texture)
	- You cannot currently copy the inactive topology layer, this will be fixed soon.
	
				
		
	
