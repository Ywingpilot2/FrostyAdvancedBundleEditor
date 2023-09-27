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

        def __init__(self, assetPath: str):
            self.FilePath = assetPath

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

        def __init__(self, bundle_name: str):
            self.Name = bundle_name

        def GetBlueprint(self):
            """
            :return: The asset which this bundle is associated with
            """
            pass

        def GetBundleType(self) -> str:
            """
            The type of bundle this is. This can be Shared, Sublevel, or Blueprint.
            :return: The name of this super bundles type(Shared, Sublevel, Blueprint)
            """
            return "self.type"

        def GetSuperBundle(self) -> str:
            """
            This returns the Bundles parent SuperBundle. Some games(e.g swbf2) do not have this.
            You can check by going into frosty, then in the "Super Bundles" tab of an asset, if it only displays "<none>" that means the game does not have SuperBundles.
            :return: The name of the bundles superbundle
            """
            return "self.superbundle"

    def AddAsset(AssetsToAdd: Asset, SelectedBundles: Bundle, ForceAdd=False, Recursive=False, AddToNetregs=False,
                 AddToMeshVariations=False):
        """
        Add assets to bundles, if they are valid for adding.
        :param AssetsToAdd: The assets that need have Bundles added.
        :param SelectedBundles: The bundles that will be added
        :param ForceAdd: This will disregard whether or not the assets(or referenced ones) are valid, and forcefully modify it.
        :param Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        :param AddToNetregs: Whether or not to add the assets to Network Registries
        :param AddToMeshVariations: Whether or not to add the assets to MeshVariationDatabases
        """
        pass

    def RemoveAsset(AssetsToRemove: Asset, SelectedBundles: Bundle, ForceAdd=False, Recursive=False,
                    RemoveFromNetregs=False, RemoveFromMeshVariations=False):
        """
        Remove assets from bundles, if they are valid for removal.
        :param AssetsToRemove: The assets that need have Bundles removed.
        :param SelectedBundles: The bundles that will be affected
        :param ForceAdd: This will disregard whether or not the assets(or referenced ones) are valid, and forcefully modify it.
        :param Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        :param RemoveFromNetregs: Whether or not to remove the assets from Network Registries
        :param RemoveFromMeshVariations: Whether or not to remove assets from MeshVariationDatabases
        """
        pass

    def ClearAsset(AssetsToRemove: Asset, Recursive=False, RemoveFromNetregs=False, RemoveFromMeshVariations=False):
        """
        Clear all of the AddedBundles of all of the selected assets.
        :param AssetsToRemove: The assets that need have their bundles cleared.
        :param Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        :param RemoveFromNetregs: Whether or not to remove the assets from Network Registries
        :param RemoveFromMeshVariations: Whether or not to remove assets from MeshVariationDatabases
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

    def Log(Message: str):
        pass

    SelectedAsset = Asset("")
    """
    The currently selected asset in the Data Explorer
    """
