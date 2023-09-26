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

### Making Bundle Operations(bunop config)
This section will dive into the basics of creating a Bundle Operation Config, or .bunop file.

### Properties & Instructions
Properties and Instructions are types of inputs you can give the Bundle Operator. The Bundle Operator reads and executes these line by line, as opposed to reading the whole file at once and deciding off of that.

Instructions tell the Bundle Operator to do something, such as add an asset to a bundle, they *instruct* the Bundle Operator.
Instructions are defined by Curly Brackets( "{ }" ), so for example "{AddAssets}" tells the Bundle Operator to add assets to bundles.

Properties set options, they tell the Bundle Operator HOW to do an Instruction. Such as telling the Bundle Operator what asset to add to what Bundle.
Properties are defined by Square Brackets( "[ ]" ), a property name then what value to set the property to, so for example "[AddToNetregs = true]" would tell the Bundle Operator to add to network registries.

Comments in a Bunop can also be created by using "//" then putting whatever text you'd like to comment. Comments are ignored by the Bundle Operator and as a result can be set to anything.
```
:{ The Bundle Operator will look at me and give an error
// :> But it won't look at me and give an error since I am a comment
```

### Order Of Operations
As said previously, Properties and Instructions are read line by line, meaning you can set a property, trigger an instruction, then set that property again(therefore changing it) as something else, the Instruction will use the first property since it was before it, then the next Instruction will use the second property.
```
[Assets = Gameplay/Weapons/BFG/wep_BFG] //The instruction {AddAssets} will use me, since I am set before it
{AddAssets} // ^ Uses

[Assets = Gameplay/Weapons/SFG/wep_SFG] //The instruction {RemoveAssets} will use me, since I am set after the first Assets, but before it
{RemoveAssets}
```

This same idea applies to properties
```
//This won't work, since ExclusiveTypes relies on the Types property, but Types are set after ExclusiveTypes
[ExclusiveTypes = true]
[Types = Weapon,Mesh,Unlock]

//This will work though, since Types are set before
[Types = BiggerWeapon,ProfileOption]
[IgnoreTypes = true]
```

### Definitions
With these basic concepts in mind, you should now have the capability to create Bundle Operations. This section will define each property and what it does, as well as instructions.

Properties:
```
[AddToBundles = true/false] //By default set to true.
If this is set to false, the assets will not be added to the bundles.

[RemoveFromBundles = true/false] //By default set to true.
If this is set to false, the assets will not be removed from the bundles.

[AddToNetregs = true/false] //By default set to false.
If this is set to true, assets will be added to Network Registries.

[RemoveFromNetregs = true/false] //By default set to false.
If this is set to true, assets will be removed from Network Registries.

[AddToMeshVariations = true/false] //By default set to false.
If this is set to true, assets will be added to MVDBs.

[RemoveFromMeshVariations = true/false] //By default set to false.
If this is set to true, assets will be removed from MVDBs.

[AddToUnlockIdTables = true/false] //By default set to false.
Only applies to games with UnlockIdTables in their LevelData's, if this is set to true assets will be added to tables.

[RemoveFromUnlockIdTables = true/false] //By default set to false.
Only applies to games with UnlockIdTables in their LevelData's, if this is set to true assets will be removed from tables.

[IsRecursive = true/false] //By default set to false, if set to true assets will be recursively edited.
[ForceAdd = true/false] //By default set to false, if set to true an asset will be edited regardless of if its valid or not.

[Types = Type1,Type2,Type3] //A list of asset types
[Assets = Levels/Asset1,Characters/Asset2,Gameplay/Asset3] //A list of assets to edit
//If "(Selected)" is inputted it will add the selected asset
//If "(AllOfTypes)" is selected then all assets that are one of the types in the [Types] Property will be selected

[Bundles = win32/bundle1, win32/bundle2] // A list of bundles to edit

[IgnoreTypes = true/false] //By default set to false,
Whether or not to ignore the types found in the [Types] Property

[ExclusiveTypes = true/false] //By default set to false,
If set to true only types in the [Types] Property will be edited or searched for

[OnlyModified = true/false] //By default set to false,
if set to true only modified assets will be edited or searched for

[OnlyAdded = true/false] //By default set to false,
if set to true only added assets will be edited or searched for

[AffectsRecursive = true/false] //By default set to false,
whether or not these conditions affect recursively found assets

//{CreateBundle} settings
[BundlePath = Gameplay/Weapons/WeaponsShared] //The path of the bundle we are adding
[SuperBundleName = win32/weapons/shared] //The name of the SuperBundle our bundle will be a part of. Not used for all games
[BundleType = Shared] //The type of bundle this should be. Shared, Sublevel, or Blueprint.

[GenerateBlueprints = true/false] //By default set to true.
If set to false assets such as Network Registries or Subworlds won't be created.

[BlueprintType = BlueprintBundle] //By default set to BlueprintBundle, The type we should create for our new BlueprintBundle
```

Instructions:
```
{AddAssets} //Adds [Assets] to [Bundles]

{RemoveAssets} //Removes [Assets] from [Bundles]

{ClearAssets} //Removes all of the Added Bundles from [Assets]

{CreateBundle} //Creates a bundle in [BundlePath]

{ResetPropertiesToDefault} //Resets all Properties to their default values
```
