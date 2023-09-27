from typing import List
# Designed for swbf1, this will go through and automatically add all assets to bundles.
from Frostpy import BunpyApi

BunpyApi.Log("Completing bundles...")
assetsAdded = 0


# get a list of assets to add to Sublevel Bundles
def recursive_sublevel_find(asset: BunpyApi.Asset) -> List[BunpyApi.Asset]:
    assets = [asset, asset.GetReferences()]
    checked_assets = []
    valid_assets = []
    while len(assets) != 0:
        if not checked_assets.__contains__(assets[0]):
            current_asset = assets[0]
            if not current_asset.Type == "SoldierWeaponUnlockAsset" or current_asset.Type == "ForceCardAsset":
                valid_assets.append(current_asset)
                checked_assets.append(current_asset)
                assets.remove(current_asset)
        else:
            checked_assets.append(assets[0])
            assets.remove(assets[0])

    return valid_assets


def asset_weaponbundle_valid(asset: BunpyApi.Asset) -> bool:
    valid = True
    for bundle in asset.AllBundles:
        if not bundle.GetBundleType() == "Blueprint" or bundle.GetBundleType() == "Sublevel":
            valid = False
            return valid
    return valid


# for weapons, we want to add to a shared bundle instead of adding to subworlds
weaponsBundle = BunpyApi.Bundle("win32/gameplay/bundles/weaponsbundlecommon")

# add assets to sublevels
for sublevel in BunpyApi.GetAllOfType("SubworldData"):
    for validAsset in recursive_sublevel_find(sublevel):
        BunpyApi.AddAsset(validAsset, sublevel.AllBundles[0], False, False, True, True)
        assetsAdded += 1

# add assets to shared bundle
for forcecard in BunpyApi.GetAllOfType("ForceCardAsset"):
    if asset_weaponbundle_valid(forcecard):  # if this asset is valid to be in the shared bundle
        BunpyApi.AddAsset(forcecard, weaponsBundle, False, True, True, True)
        assetsAdded += 1

    else:  # if not then it must be something loaded into level's and blueprints
        # first load it into level bundles, then later we will load it into blueprint bundles
        for leveldata in BunpyApi.GetAllOfType("LevelData"):
            BunpyApi.AddAsset(forcecard, leveldata.AllBundles[0], False, True, True, True)
            assetsAdded += 1

for blueprint in BunpyApi.GetAllOfType("SoldierBlueprint"):
    BunpyApi.AddAsset(blueprint, blueprint.AllBundles[0], False, True, True, True)
    assetsAdded += 1

for unlockassets in BunpyApi.GetAllOfType("SoldierWeaponUnlockAsset"):
    BunpyApi.AddAsset(unlockassets, weaponsBundle, False, True, True, True)
    assetsAdded += 1

BunpyApi.Log("Added a total of " + assetsAdded.__str__() + " assets to bundles")
