"""
This modules contains all of the methods variables and classes you will need in order to make a Bundle Operation
"""
#NOTICE:
#The scripts here are just dummy scripts, and will be replaced when parsed by the Bundle Operator.
#this is done for simplicity on your part
class BunpyApi: #really annoying work around. We shove everything inside a class to stop people from just doing from BundleOperator import Asset(too complicated to process that)
    class Asset:
        """
        An EBX asset to check with. This contains the type, the filename, and filepath.
        To create this, all you need to input is the FilePath.
        """

        FilePath = ""
        Type = "Invalid"
        IsModified = False
        IsAdded = False
    
        def GetFilename(self) -> str:
            """
            {ReadOnly}
            The name of the asset
            """
            return self.FilePath.Split("/")[-1]
        
        def GetReferences() -> list:
            """
            Returns a list of Assets which are referenced by this Asset
            """
            references = []
            return references
        
    class Bundle:
        """
        A Bundle which you can add/remove assets to and from. These control what gets loaded in vs what doesn't.
        To create this, all you need is to input the name of the bundle.
        """
        def __init__(self, BundleName: str):
            self.Name = BundleName
    
        def GetBundleType() -> str:
            """
            {ReadOnly}
            The type of bundle this is. This can be Shared, Sublevel, or Blueprint.
            """
            return "self.type"
    
        def GetSuperBundle() -> str:
            """
            {ReadOnly}
            This returns the Bundles parent SuperBundle. Some games(e.g swbf2) do not have this.
            You can check by going into frosty, then in the "Super Bundles" tab of an asset, if it only displays "<none>" that means the game does not have SuperBundles.
            """
            return "self.superbundle"
        
    def AddAsset(AssetsToAdd: Asset, SelectedBundles: Bundle, ForceAdd = False, Recursive = False, AddToNetregs = False, AddToMeshVariations = False):
        """
        Add assets to bundles, if they are valid for adding.
        AssetsToAdd: The assets that need have Bundles added.
        SelectedBundles: The bundles that will be added
        ForceAdd: This will disregard whether or not the assets(or referenced ones) are valid, and forcefully modify it.
        Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        AddToNetregs: Whether or not to add the assets to Network Registries
        AddToMeshVariations: Whether or not to add the assets to MeshVariationDatabases
        """
        pass

    def RemoveAsset(AssetsToRemove: Asset, SelectedBundles: Bundle, ForceAdd = False, Recursive = False, RemoveFromNetregs = False, RemoveFromMeshVariations = False):
        """
        Remove assets from bundles, if they are valid for removal.
        AssetsToRemove: The assets that need have Bundles removed.
        SelectedBundles: The bundles that will be affected
        ForceAdd: This will disregard whether or not the assets(or referenced ones) are valid, and forcefully modify it.
        Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        RemoveFromNetregs: Whether or not to remove the assets from Network Registries
        RemoveFromMeshVariations: Whether or not to remove assets from MeshVariationDatabases
        """
        pass

    def ClearAsset(AssetsToRemove: Asset, Recursive = False, RemoveFromNetregs = False, RemoveFromMeshVariations = False):
        """
        Clear all of the AddedBundles of all of the selected assets.
        AssetsToRemove: The assets that need have their bundles cleared.
        Recursive: Whether or not all referenced assets should have this done too. This includes references of references, this will recursively search down the reference chain until it cannot anymore.
        RemoveFromNetregs: Whether or not to remove the assets from Network Registries
        RemoveFromMeshVariations: Whether or not to remove assets from MeshVariationDatabases
        """
        pass

    def AddBundle(BundlePath: str, SuperBundleName: str, BundleType: str = "Shared", GenerateBlueprints = True, BlueprintType = "BlueprintBundle"):
        """
        This will create a brand new bundle in the specified directory.
        BundlePath: The path the bundle should be in, all of its assets will be put here as well.
        SuperBundleName: The name of the Super Bundle this should be parented to. Some games do not have this, you can check by going into frosty, then in the "Super Bundles" tab of an asset, if it only displays "<none>" that means the game does not have SuperBundles.
        BundleType: The type of bundle to create. It can either be Shared, Sublevel, or Blueprint.
        GenerateBlueprints: Whether or not the Bundle should be populated with generic blueprints e.g Network Registries. If its a Blueprint Bundle it will also generate a Blueprint of the specified type.
        BlueprintType: The type of blueprint to create when creating a BlueprintBundle.
        """
        return BunpyApi.Bundle("win32/" + BundlePath.lower)
    
    def GetAllOfType(type: str, OnlyModified: bool, OnlyAdded: bool) -> list:
        return []
    
    def Log(Message: str):
        pass