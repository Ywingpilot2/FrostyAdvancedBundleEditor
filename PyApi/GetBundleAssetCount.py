from Frostpy import BunpyApi
# Gets the number of assets in the first bundle of an asset

selectedAsset = BunpyApi.SelectedAsset
selectedBundle = selectedAsset.AllBundles[0]
assetCount = 0
for asset in BunpyApi.GetAllInBundle(selectedBundle):
    assetCount += 1


BunpyApi.Log(assetCount.__str__())
