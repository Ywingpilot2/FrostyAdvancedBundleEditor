from Frostpy import BunpyApi
# This will clear out all of the added bundles of assets

BunpyApi.Log("Clearing Bundles...")
ClearedCount = 0

for assetToClear in BunpyApi.GetAllOfType("", True):
    BunpyApi.ClearAsset(assetToClear, False, True, True)
    ClearedCount += 1

BunpyApi.Log("Cleared " + ClearedCount.__str__() + " assets")
