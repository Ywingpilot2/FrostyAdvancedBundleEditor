# Advanced Bundle Editor
This is a fork of the Bundle Editor Plugin from [Frosty Editor](https://github.com/CadeEvs/FrostyToolsuite/tree/1.0.6.3) which adds many new advanced features such as Recursive editing, automatic network registering and adding to Mesh Variation Databases

## Bundle Editor Features
This version of the Bundle Editor is designed to work for all games, and includes many major additions such as the ability to include all assets in a reference chain(or "Recursive editing") or automatically detecting when an asset needs to be added to Network Registries.

### Recursive Editing
With the Advanced Bundle Editor, you can recursively add and remove assets to and from a bundle. To Recursively edit an asset is to add its references, and the references of references, to a bundle. Meaning all requirements for an asset will be included. This workflow can allow for a user to add many many assets to a bundle at once, for example, adding the entire contents of one subworld to another.

![RecEditShowcase](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/127a74a2-72b9-46d4-b170-ce1ae00ea21b)

Don't let the amount of assets that can be modified dissuade you though, as the Advanced Bundle Editor can modify hundreds of assets in mere seconds. The Advanced Bundle Editor also does so intelligently, never adding an asset when it doesn't need to(for example, an asset in a shared bundle will not be added to a sublevel one, or an asset in a leveldata bundle will not be added to a child subworld bundle)

The way the Bundle Editor decides to modify assets recursively can also be customized in the options menu.

![RecEditOptions](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/d5d7baf5-8c60-4ac4-a231-f5f725f28112)

### Extra Modifications
Sometimes when adding certain asset types to bundles extra files such as Network Registries(so the game knows that an asset has networking) or Mesh Variation Databases(a database of all of the meshes and variations they have), with the Advanced Bundle Editor this is handled automatically. 

For Network Registries, a cache is generated which contains a list of all Networked asset types, as well as all of the bundles which contain Network Registries(for new bundles, this is done dynamically)

The same is true for Mesh Variation Databases, only a cache is not required for the types of assets that get added. that is determined automatically.

Extra modifications can be enabled and disabled in the options menu as well.

![ExtraEditOptions](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/b3de2209-b2d6-49d1-91c3-884992451b88)

## Bundle Operations
Bundle Operations allow you to automate repetitive Bundle Editing tasks, such as adding all modified assets to a list of bundles, with either Pseudo-Xml style config files or an advanced Python API.

### Using Bundle Operations
To use a Bundle Operation, all you need to do is open the "Tools" menu in frosty, then in the Bundle Editor section click Open Bundle Operation

![OpenBundleOperationShowcase](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/2a5ea839-4b76-4c27-9a88-755d33c54e8e)

Then, select the .bunop or .py operation of your choice.
If this is a Python script, you maybe prompted to install Python 3.8 or Pythonnet 2.5.0. If you are, please follow the instructions it gives you.

![SelectBunOpShowcase](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/858218c7-5c7e-42b2-9073-14cbc493f831)

The Bundle Operator should now be executing the tasks specified in the file, and a progress window should open. 

![BunOpShowcase](https://github.com/Ywingpilot2/FrostyAdvancedBundleEditor/assets/136618828/80d67b6f-a998-46ac-aabc-39ca9563ccf4)
