# NOTICE: This is example code, and may not actually execute.
from Frostpy import BunpyApi  # Import the Bundle Operator's API
# You can also just do import Frostpy, and use Frostpy.BunpyApi

BunpyApi.Log("Hello world!")  # This will log a message in the logger stating "Hello World!"
asset = BunpyApi.Asset("Gameplay/HeroAvailableLogic")
BunpyApi.Log(asset.Filename)
bundle = BunpyApi.SelectedBundle
BunpyApi.Log(bundle.Name)
BunpyApi.AddToBundle(asset, bundle)
