using BundleEditorPlugin;
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace BundleEditPlugin
{
    public class BundleEditorMenuExtension : MenuExtension
    {
        public static ImageSource iconImageSource = new ImageSourceConverter().ConvertFromString("pack://application:,,,/BundleEditorPlugin;component/Images/BundleEdit.png") as ImageSource;

        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "Bundle Editor";

        public override string MenuItemName => "Bundle Editor";
        public override ImageSource Icon => iconImageSource;

        public override RelayCommand MenuItemClicked => new RelayCommand((o) =>
        {
            App.EditorWindow.OpenEditor("Bundle Editor", new BundleEditor());
        });
    }

    public class GenerateNetworkCache : MenuExtension
    {

        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "Bundle Editor";

        public override string MenuItemName => "Generate networking cache";

        public override RelayCommand MenuItemClicked => new RelayCommand((o) =>
        {
            FrostyTaskWindow.Show("Generating cache...", "Initiating...", (Task) =>
            {
                List<string> netTypes = new List<string>();
                List<string> netBundles = new List<string>();

                foreach (EbxAssetEntry networkRegistryEntry in App.AssetManager.EnumerateEbx("NetworkRegistryAsset"))
                {
                    BundleEntry bundleEntry = App.AssetManager.GetBundleEntry(networkRegistryEntry.Bundles[0]);
                    if (bundleEntry.Type == BundleType.SubLevel && Config.Get("CacheSubworldBundles", true))
                    {
                        Task.Update($"Caching {networkRegistryEntry.Filename}");
                        if (Config.Get("CacheNetworkedTypes", true))
                        {
                            foreach (PointerRef reference in ((dynamic)App.AssetManager.GetEbx(networkRegistryEntry).RootObject).Objects)
                            {
                                //Now we need to get the original object, so we can fetch its type
                                object ebxObject = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(reference.External.FileGuid)).GetObject(reference.External.ClassGuid);

                                if (!netTypes.Contains(ebxObject.GetType().Name))
                                {
                                    netTypes.Add(ebxObject.GetType().Name);
                                }
                            }
                        }
                        netBundles.Add($"{bundleEntry.Name},{networkRegistryEntry.Name}");
                    }
                    else if (bundleEntry.Type == BundleType.SharedBundle && Config.Get("CacheSharedBundles", true))
                    {
                        Task.Update($"Caching {networkRegistryEntry.Filename}");
                        if (Config.Get("CacheNetworkedTypes", true))
                        {
                            foreach (PointerRef reference in ((dynamic)App.AssetManager.GetEbx(networkRegistryEntry).RootObject).Objects)
                            {
                                //Now we need to get the original object, so we can fetch its type
                                object ebxObject = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(reference.External.FileGuid)).GetObject(reference.External.ClassGuid);

                                if (!netTypes.Contains(ebxObject.GetType().Name))
                                {
                                    netTypes.Add(ebxObject.GetType().Name);
                                }
                            }
                        }
                        netBundles.Add($"{bundleEntry.Name},{networkRegistryEntry.Name}");
                    }
                    else if (bundleEntry.Type == BundleType.BlueprintBundle && Config.Get("CacheBlueprintBundles", true))
                    {
                        Task.Update($"Caching {networkRegistryEntry.Filename}");
                        if (Config.Get("CacheNetworkedTypes", true))
                        {
                            foreach (PointerRef reference in ((dynamic)App.AssetManager.GetEbx(networkRegistryEntry).RootObject).Objects)
                            {
                                //Now we need to get the original object, so we can fetch its type
                                object ebxObject = App.AssetManager.GetEbx(App.AssetManager.GetEbxEntry(reference.External.FileGuid)).GetObject(reference.External.ClassGuid);

                                if (!netTypes.Contains(ebxObject.GetType().Name))
                                {
                                    netTypes.Add(ebxObject.GetType().Name);
                                }
                            }
                        }
                        netBundles.Add($"{bundleEntry.Name},{networkRegistryEntry.Name}");
                    }

                    //Now we store the cache in a text file
                    Task.Update("Writing to cache...");
                    StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedTypes.txt");
                    foreach (string assetType in netTypes)
                    {
                        sw.WriteLine(assetType);
                    }
                    sw.Close();

                    sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedBundles.txt");
                    foreach (string bundle in netBundles)
                    {
                        sw.WriteLine(bundle);
                    }
                    sw.Close();
                }
            });
        });
    }

    public class GenerateMvdbBundleCache : MenuExtension
    {

        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "Bundle Editor";

        public override string MenuItemName => "Generate MVDB cache";

        public override RelayCommand MenuItemClicked => new RelayCommand((o) =>
        {
            FrostyTaskWindow.Show("Generating cache...", "Initiating...", (Task) =>
            {
                List<string> mshBundles = new List<string>();

                foreach (EbxAssetEntry MeshVariationEntry in App.AssetManager.EnumerateEbx("MeshVariationDatabase"))
                {
                    BundleEntry bundleEntry = App.AssetManager.GetBundleEntry(MeshVariationEntry.Bundles[0]);
                    if (bundleEntry.Type == BundleType.SubLevel && Config.Get("CacheSubworldBundles", true))
                    {
                        Task.Update($"Caching {MeshVariationEntry.Filename}");

                        mshBundles.Add($"{bundleEntry.Name},{MeshVariationEntry.Name}");
                    }
                    else if (bundleEntry.Type == BundleType.SharedBundle && Config.Get("CacheSharedBundles", true))
                    {
                        Task.Update($"Caching {MeshVariationEntry.Filename}");

                        mshBundles.Add($"{bundleEntry.Name},{MeshVariationEntry.Name}");
                    }
                    else if (bundleEntry.Type == BundleType.BlueprintBundle && Config.Get("CacheBlueprintBundles", true))
                    {
                        Task.Update($"Caching {MeshVariationEntry.Filename}");

                        mshBundles.Add($"{bundleEntry.Name},{MeshVariationEntry.Name}");
                    }
                }

                //Now we store the cache in a text file
                Task.Update("Writing to cache...");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_MvdbBundles.txt");
                foreach (string assetType in mshBundles)
                {
                    sw.WriteLine(assetType);
                }
                sw.Close();
            });
        });
    }

    public class OpenBundleOperation : MenuExtension
    {
        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "Bundle Editor";

        public override string MenuItemName => "Open Bundle Operation";

        public override RelayCommand MenuItemClicked => new RelayCommand((o) =>
        {
            BundleOperator bundleOperator = new BundleOperator();
            bundleOperator.InitiateBundleOperation();
        });
    }

    public class IgnoreAssetContextMenuItem : DataExplorerContextMenuExtension
    {
        public override string ContextItemName => "Ignore Asset";

        public override RelayCommand ContextItemClicked => new RelayCommand((o) =>
        {
            EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
            Guid guid = entry.Guid;
            List<string> list = Config.Get("ExcludedAssets", "").Split(',').ToList();
            if (list.Contains(entry.Name)) //Check if Entry is already in list
            {
                list.Remove(entry.Name);
                string SaveList = string.Join(",", list);

                if (SaveList.EndsWith(","))
                {
                    int LastIndex = SaveList.LastIndexOf(",");
                    SaveList.Remove(LastIndex, 1);
                }
                App.Logger.Log("Removed asset {0} from ignored list", entry.DisplayName);
                Config.Add("ExcludedAssets", SaveList);
                Config.Save();
            }

            else //If not in list just add it to the list
            {
                list.Add(entry.Name);
                string SaveList = string.Join(",", list);
                if (SaveList.EndsWith(","))
                {
                    int LastIndex = SaveList.LastIndexOf(",");
                    SaveList.Remove(LastIndex, 1);
                }
                App.Logger.Log("Added asset {0} to ignored list", entry.DisplayName);
                Config.Add("ExcludedAssets", SaveList);
                Config.Save();
            }
            
        });
    }
}
