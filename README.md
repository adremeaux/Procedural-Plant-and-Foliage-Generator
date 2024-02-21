# Procedural Plant and Foliage Generator

The Procedural Plant and Foliage Generator is a tool written in C# and Unity URP to dynamically generate a stunning array of foliage and full plants from a set of over 100 parameters.

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/grid.png)

The tool also allows hybridization between plants at various levels.

![Hybrids](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/lines.png)

Plants are created sequentially with params assigned to major groups, starting from the leaf shape:

#### Leaf Shape

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/1.png)

#### Vein Layout

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/2.png)

#### Vein Width

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/3.png)

#### Albedo Texture Gen

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/4.png)

#### Normal Map Texture Gen

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/5.png)

#### Leaf Distortions

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/6.png)

#### Leaf Arrangement

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/7.png)

## Usage

Load up the Lab scene and begin! Select PlantPrefab, scroll down to the Preset menu, and try loading some different presets.

#### UI Overview

![Examples](https://raw.githubusercontent.com/adremeaux/Procedural-Plant-and-Foliage-Generator/main/Assets/Images/Publicity/UI.png)

- **Core**
  - **Clear Cache:** Used when code or parameter updates are breaking things
  - **Refresh:** Refresh the AssetDatabase
  - **Randomize Seed:** Randomize the seed used for randomness generation. This can also be set manually in Deps -> Base Params -> Random Seed
  - **Create Texture:** Run the enabled Texture Commands (see below) to generate a texture for your current preset
  - **Beziers/Veins/Prerender etc:** Various view modes that can be used while editing
  - **Min Poly/Low Poly etc:** Quality levels for mesh generation and subdivision
  - **Fix Mats:** Sometimes the materials come off, this should fix everything up
- **Texture Commands**
  - **Render Layers:** Which set of textures to create when pressing Create Texture
  - **Albedo/Normal/Height/VeinMask:** Enable or disable render passes for the various textures
- **Preset**
  - **Load Preset:** Select a preset from the dropdown and press Load Preset to load it
  - **Save Preset As:** Add a new preset with the given name (this creates a permanent new xml file)
  - **Next Preset:** Quickly cycle through the presets
  - **Change Pot:** Change the pot!
- **Generator**
  - **Render:** This shouldn't have to be used, but this will force a full render pass if a setting that has been changed isn't showing up properly (notable usecase: the "enable distortions" checkbox)
  - **Randomize:** This should randomize the parameters and give a random leaf, but don't expect good results, it needs work

## Contributing

Pull requests are welcome. If you would like to contribute new plant designs/presets, you can submit pull requests, but please keep them constrained to setups of _real world plants._ Although a couple novel plants are included in the presets ("Pumpkin Smash", "Blackstrap"), future requests should be for real plants only.

## Where to Start

Here are links to some of the critical files in the project:
* [Plant.cs](Assets/Scripts/Core/PlantEditor/Plant.cs): The root of the plant renderer. Start in RenderAll()
* [IMTextureFactory](Assets/Scripts/Core/PlantEditor/Texture/IMTextureFactory.cs): Factory for the classic (CPU) texture generation
* [PlantPhysicsSimulator](Assets/Scripts/Core/Physics/PlantPhysicsSimulator.cs): Leaf collision detection and layout
* [LeafParamDefaults](Assets/Scripts/Core/PlantEditor/Model/LeafParamDefaults.cs): Parameter definitions
* [LeafDistort.compute](Assets/Shaders/Plant/LeafDistort.compute): Distortion compute shader

## License

[GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html)

-----

## What's New

v2.0.0
* Major changes to the distortion flow, moving all distortion code into compute shaders and instancing meshes
* Introduction of a pure pixel shader materials rendering pipeline. Currently needs to be manually activated in the code. Covers the whole albedo target but is buggy.
* Physics computations to prevent leaf intersections. Dynamic stems to support sporadically placed leaves.
* Stem and leaf skinning and rigging. Full physics is implemented however relies on a proprietary library, so has been removed in this version.
* Rendered plants can be serialized to disc as meshes
* Major randomization overhaul
* Lots of new parameters
