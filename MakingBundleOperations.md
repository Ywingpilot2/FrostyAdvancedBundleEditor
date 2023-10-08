# Making Bundle Operations(bunop config)
This page will dive into the basics of creating a Bundle Operation Config, or .bunop file.

## Properties & Instructions
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

## Order Of Operations
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

## Definitions
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
