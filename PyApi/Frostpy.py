"""
This modules contains all of the methods variables and classes you will need in order to make a Bundle Operation
"""

# NOTICE:
# The scripts here are just dummy scripts, and will be replaced when parsed by the Bundle Operator.
# this is done for simplicity on your part
from typing import List


class BunpyApi:  # really annoying work around. We shove everything inside a class to stop people from just doing
    # from BundleOperator import Asset(too complicated to process that)
    class Asset:
        """
        An EBX asset to check with. This contains the type, the filename, and filepath.
        """

        def __init__(self, asset_path: str):
            self.FilePath = asset_path

        FilePath = ""
        """
        The path to the file, e.g Gameplay/Weapons/wep_gun
        """
        Filename = ""
        """
        The name of the file, e.g wep_gun
        """
        Type = "Invalid"
        """
        The type of asset this is, e.g SpatialPrefab
        """

        IsModified = False
        """
        Whether or not this asset has been modified
        """
        IsAdded = False
        """
        Whether or not this asset is added/duped
        """

        OriginalBundles = []
        """
        a list of Bundles this asset is its unmodified state
        """
        AddedBundles = []
        """
        a list of bundles this asset has been added to
        """
        AllBundles = []
        """
        all of the bundles this asset has, added or original
        """

        def GetReferences(self) -> list:
            """
            :return: A list of Assets which are referenced by this Asset
            """
            references = []
            return references

    class Bundle:
        """
        A Bundle which you can add/remove assets to and from. These control what gets loaded in vs what doesn't.
        To create this, all you need is to input the name of the bundle.
        """

        Name = ""
        """
        The name of this bundle
        """
        Type = ""
        """
        The type of this bundle, so Shared, Sublevel, Blueprint
        """
        SuperBundle = ""
        """
        The name of this bundles Super Bundle
        """

        def __init__(self, bundle_name: str):
            self.Name = bundle_name

        def get_blueprint(self):
            """
            :return: The asset which this bundle is associated with
            """
            pass

    def AddToBundle(asset: Asset, bundle: Bundle):
        """
        This adds an Asset to a Bundle
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def AddToReg(asset: Asset, bundle: Bundle):
        """
        This adds an Asset to the Bundles assigned network registry
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def AddToMvdb(asset: Asset, bundle: Bundle):
        """
        This adds an Asset to the Bundles assigned Mesh Variation Database
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RemFromBundle(asset: Asset, bundle: Bundle):
        """
        This removes an Asset from a Bundle
        :param asset: The asset you want to remove
        :param bundle: The bundle to remove it from
        """
        pass

    def RemFromReg(asset: Asset, bundle: Bundle):
        """
        This removes an Asset from a Bundle's Network Registry
        :param asset: The asset you want to remove
        :param bundle: The bundle to remove it from
        """
        pass

    def RemFromMvdb(asset: Asset, bundle: Bundle):
        """
        This removes an Asset from a Bundle's Mesh Variation Database
        :param asset: The asset you want to remove
        :param bundle: The bundle to remove it from
        """
        pass

    def RecAddToBundle(asset: Asset, bundle: Bundle):
        """
        This recursively adds an Asset to a Bundle
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RecAddToReg(asset: Asset, bundle: Bundle):
        """
        This recursively adds an Asset to a Bundles Network Registry
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RecAddToMvdb(asset: Asset, bundle: Bundle):
        """
        This recursively adds an Asset to a Bundles Mesh Variation Database
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RecRemFromBundle(asset: Asset, bundle: Bundle):
        """
        This recursively removes an Asset from a Bundle
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RecRemFromReg(asset: Asset, bundle: Bundle):
        """
        This recursively removes an Asset from a Bundles Network Registry
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def RecRemFromMvdb(asset: Asset, bundle: Bundle):
        """
        This recursively removes an Asset from a Bundles Mesh Variation Database
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        """
        pass

    def CompletelyAddAsset(asset: Asset, bundle: Bundle, recursive=True):
        """
        This will add an asset to everything it needs to a bundle alongside everything else it needs to be in to function correctly.
        :param asset: The asset you want to add
        :param bundle: The bundle to add it to
        :param recursive: Whether or not you want to do this operation recursively
        """
        pass

    def CompletelyRemoveAsset(asset: Asset, bundle: Bundle, recursive=True):
        """
        This will remove an asset to everything it needs to a bundle alongside everything else it needs to be in to function correctly.
        :param asset: The asset you want to remove
        :param bundle: The bundle to remove it from
        :param recursive: Whether or not you want to do this operation recursively
        """
        pass

    def AddBundle(BundlePath: str, SuperBundleName: str, BundleType: str = "Shared", GenerateBlueprints=True,
                  BlueprintType="BlueprintBundle"):
        """
        This will create a brand new bundle in the specified directory.
        :param BundlePath: The path the bundle should be in, all of its assets will be put here as well.
        :param SuperBundleName: The name of the Super Bundle this should be parented to. Some games do not have this, you can check by going into frosty, then in the "Super Bundles" tab of an asset, if it only displays "<none>" that means the game does not have SuperBundles.
        :param BundleType: The type of bundle to create. It can either be Shared, Sublevel, or Blueprint.
        :param GenerateBlueprints: Whether or not the Bundle should be populated with generic blueprints e.g Network Registries. If its a Blueprint Bundle it will also generate a Blueprint of the specified type.
        :param BlueprintType: The type of blueprint to create when creating a BlueprintBundle.
        """
        return BunpyApi.Bundle("win32/" + BundlePath.lower())

    def GetAllOfType(type: str, OnlyModified=False, OnlyAdded=False) -> List[Asset]:
        """
        Gets a list of all of the Assets matching specified parameters
        :param type: The type to search for. If left blank will enumerate over all files
        :param OnlyModified: Whether or not to only search for modified assets
        :param OnlyAdded: Whether or not to only search for added assets
        :return: A list of the assets found
        """
        return []

    def GetAllInBundle(Bundle: Bundle, OnlyModified=False, OnlyAdded=False) -> List[Asset]:
        """
        Gets a list of all of the Assets matching specified parameters
        :param Bundle: The bundle to search through
        :param OnlyModified: Whether or not to only search for modified assets
        :param OnlyAdded: Whether or not to only search for added assets
        :return: A list of the assets found
        """
        return []

    def Log(Message: str):
        pass

    SelectedAsset = Asset("")
    """
    The currently selected asset in the Data Explorer
    """
    SelectedBundle = Bundle("")
    """
    The currently selected Bundle in the Bundle Editor
    """
