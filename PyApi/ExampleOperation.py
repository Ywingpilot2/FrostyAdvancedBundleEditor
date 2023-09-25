# NOTICE: This is example code, and may not actually execute.
from Frostpy import BunpyApi  # Import the Bundle Operator's API
# You can also just do import Frostpy, and use Frostpy.BunpyApi

BunpyApi.Log("Hello world!")  # This will log a message in the logger stating "Hello World!"

testAsset = BunpyApi.Asset("Gameplay/Weapons/BFG/Wep_BFG")  # Declaring an asset
# An asset is anything that can be found in Frosty's base file explorer

BunpyApi.Log(testAsset.Filename)  # Currently, the Bundle Operator is not able to log anything other then raw strings
# If you wish to log a variable, please manually convert that variable to a string, and input that string like here.
BunpyApi.Log(testAsset.AllBundles[1].Name)  # This will log the name of the first bundle our asset has
# You can also get OriginalBundles and AddedBundles
# OriginalBundles are the bundles an asset in the vanilla game contains
# AddedBundles are bundles this asset has been modified to be in
if testAsset.IsModified:
    BunpyApi.Log("Asset has been modified!")  # This will log if the asset has been modified at all

elif testAsset.IsAdded:
    BunpyApi.Log("Asset has been added!")  # This will log if the asset has been added/duped

testBundle = BunpyApi.Bundle("win32/gameplay/sharedweapons")  # Declaring a bundle

BunpyApi.Log(testBundle.Name) # This will log the name of the bundle, so win32/gameplay/sharedweapons
BunpyApi.Log(testBundle.GetBundleType())  # This will log the type of the bundle, so in this case Shared
# There are 3 kinds of bundles: Shared, Sublevel, and Blueprint.
# Blueprint and Sublevel bundles have at the center of them have an Asset, such as Sublevel bundles having a LevelData or Subworld
BunpyApi.Log(testBundle.GetBlueprint().Filename)  # This will get that asset and log its Filename

BunpyApi.AddAsset(testAsset, testBundle)  #  This will add our test asset to the bundle

BunpyApi.RemoveAsset(testAsset, testBundle)  # This will then remove our test asset from the bundle

BunpyApi.ClearAsset(BunpyApi.Asset("Gameplay/Weapons/SFG/Wep_SFG"))  # This will remove all of the Added Bundles of another asset
