using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Controls.Editors;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;

namespace AdvancedBundleEditorPlugin.Options
{
    [EbxClassMeta(EbxFieldType.Struct)]
    public class BunStringOpt
    {
        [DisplayName("Value")]
        public string Value { get; set; } = "";
    }

    [EbxClassMeta(EbxFieldType.Struct)]
    public class BunPtrOpt
    {
        [DisplayName("Value")]
        [EbxFieldMeta(EbxFieldType.Pointer)]
        public PointerRef Value { get; set; }
    }

    [DisplayName("Bundle Editor Options")]
    public class BundleEditorOptions : OptionsExtension
    {
        [Category("Recursive Bundling Options")]
        [DisplayName("Excluded Bundles")]
        [Description("If an asset is in any of these bundles, it will not be edited.\nTo add multiple entries, use a comma with no spaces between each entry, like this:\nwin32/gameplay/wsgameconfiguration,win32/default_settings_win32,win32/ui/loadingscreens")]
        [EbxFieldMeta(EbxFieldType.Struct)]
        public List<BunStringOpt> ExcludedBundles { get; set; } = new List<BunStringOpt>();

        [Category("Recursive Bundling Options")]
        [DisplayName("Excluded Assets")]
        [Description("Ignore assets in this list. To add multiple entries, use a comma with no spaces between each entry, like this:\nAI/BattleAI/System/AISystem,AI/BattleAI/Tactics/AIRebelSoldierTactics")]
        [EbxFieldMeta(EbxFieldType.Struct)]
        public List<BunStringOpt> ExcludedAssets { get; set; } = new List<BunStringOpt>();

        [Category("Extra modifications")]
        [DisplayName("Add networked assets to Registries")]
        [Description("If true, adding an asset to a bundle will also add its root object to the network registry assigned to the bundle.\nNOTICE: This ONLY adds the Asset itself to the Registry, any other objects inside the asset(E.G Synced Bools) that need to be added will not be added.")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool AllowRootNetreg { get; set; }

        [Category("Extra modifications")]
        [DisplayName("Add mesh assets to MVDBs")]
        [Description("If true, mesh assets will be added to Mesh Variation Databases with their textures.\nTo get an asset to have its textures added to a MVDBs, Add the assets TextureParameters under the Shaders tab. I would suggest using existing MVDB entry texture parameters as a reference.")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool AllowMvdb { get; set; }

        [Category("Bundle Handling")]
        [DisplayName("Add global assets to local bundles")]
        [Description("Enabling this option will mean that assets in shared bundles will be recursively added to sublevel and blueprint bundles.")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool EditSharedBundled { get; set; }

        [Category("Bundle Handling")]
        [DisplayName("Add level assets to sublevel bundles")]
        [Description("Enabling this will mean assets loaded into the level data of a level will also be loaded into the subworld")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool EditSharedLevelBundled { get; set; }

        [Category("Caching")]
        [DisplayName("Check Blueprint bundles for cache")]
        [Description("Whether or not to check blueprint bundles for caching")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool CacheBlueprintBundles { get; set; }

        [Category("Caching")]
        [DisplayName("Check Subworld bundles for cache")]
        [Description("Whether or not to check Subworld/Level bundles for caching")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool CacheSubworldBundles { get; set; }

        [Category("Caching")]
        [DisplayName("Check Shared bundles for cache")]
        [Description("Whether or not to check Shared bundles for caching")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool CacheSharedBundles { get; set; }

        [Category("Caching")]
        [DisplayName("Cache Networked types")]
        [Description("Whether or not to cache networked types")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool CacheNetworkedTypes { get; set; }

        [Category("Bunpy")]
        [DisplayName("Python 3.8 location")]
        [Description("Where Python 3.8 is located. The Bundle Operator should be able to automatically find this, so unless asked to there should be no need to set this.\nThis should be set as the folder with the python.exe with it, as well as Python38.dll")]
        [EbxFieldMeta(EbxFieldType.String)]
        public string BunpyLocation { get; set; }

        [Category("Bunpy")]
        [DisplayName("PythonNet 2.5.0 location")]
        [Description("Where PythonNet 2.5.0 is located. If this is not already installed, and the Bundle Operator has the correct Python 3.8 path, then this should be automatically installed.")]
        [EbxFieldMeta(EbxFieldType.String)]
        public string PynetLocation { get; set; }

        public override void Load()
        {
            List<string> ExcludedBundlesNames = Config.Get("ExcludedBundlesv2", new List<string>(), ConfigScope.Game);
            foreach (string str in ExcludedBundlesNames)
            {
                ExcludedBundles.Add(new BunStringOpt() { Value = str });
            }

            ExcludedAssets = Config.Get("ExcludedAssetsv2", new List<BunStringOpt>(), ConfigScope.Game);
            AllowRootNetreg = Config.Get("AllowRootNetreg", false);
            EditSharedBundled = Config.Get("ModifySharedBundled", false);
            EditSharedLevelBundled = Config.Get("ModifyLevelBundled", false);
            AllowMvdb = Config.Get("AllowMVDB", false);
            CacheBlueprintBundles = Config.Get("CacheBlueprintBundles", false);
            CacheSubworldBundles = Config.Get("CacheSubworldBundles", true);
            CacheSharedBundles = Config.Get("CacheSharedBundles", true);
            CacheNetworkedTypes = Config.Get("CacheNetworkedTypes", true);
            BunpyLocation = Config.Get("BunpyLocation", "");
            PynetLocation = Config.Get("PynetLocation", "");
        }
        public override void Save()
        {
            List<string> list = new List<string>();
            foreach (BunStringOpt str in ExcludedBundles)
            {
                list.Add(str.Value);
            }
            Config.Add("ExcludedBundlesv2", list, ConfigScope.Game);
            Config.Add("ExcludedAssetsv2", ExcludedAssets, ConfigScope.Game);
            Config.Add("AllowRootNetreg", AllowRootNetreg);
            Config.Add("ModifySharedBundled", EditSharedBundled);
            Config.Add("ModifyLevelBundled", EditSharedLevelBundled);
            Config.Add("AllowMVDB", AllowMvdb);
            Config.Add("CacheBlueprintBundles", CacheBlueprintBundles);
            Config.Add("CacheSubworldBundles", CacheSubworldBundles);
            Config.Add("CacheSharedBundles", CacheSharedBundles);
            Config.Add("CacheNetworkedTypes", CacheNetworkedTypes);
            Config.Add("BunpyLocation", BunpyLocation);
            Config.Add("PynetLocation", PynetLocation);
        }


    }
}
