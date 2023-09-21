using BundleEditPlugin;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundleEditorPlugin
{
    public class BundleOperator
    {
        public string BunOpName { get; private set; }
        private string BunOpPath { get; set; }

        /// <summary>
        /// Initiate a new Bundle Operation
        /// </summary>
        public BundleOperator InitiateBundleOperation()
        {
            //create a open file dialogue so the user can select a file
            FrostyOpenFileDialog ofd = new FrostyOpenFileDialog("Open Bundle Operation", "*.bunop|*.bunop", "BundleOperation");
            if (ofd.ShowDialog())
            {
                BunOpName = ofd.FileName.Replace("\\", "/").Split('/').Last();
                BunOpPath = ofd.FileName;
                FrostyTaskWindow.Show(BunOpName, "", (task) =>
                {
                    ReadBundleOperation(task);
                });
            }
            return this;
        }

        /// <summary>
        /// The brains behind the operation
        /// </summary>
        private void ReadBundleOperation(FrostyTaskWindow task)
        {
            BunOpProperties opProperties = new BunOpProperties();

            StreamReader sr = new StreamReader(BunOpPath);
            string line = sr.ReadLine();
            while (line != null)
            {
                //First we need to setup the line for reading
                int commentposition = line.IndexOf("//");
                if (commentposition != -1 && commentposition != 0)
                {
                    line = line.Remove(commentposition).TrimEnd(' ');
                }
                if (commentposition == 0 || string.IsNullOrEmpty(line))
                {
                    line = sr.ReadLine();
                    continue;
                }

                if (line.StartsWith("["))
                {
                    switch (line.Replace("[", "").Replace("]", "").Split('=').First())
                    {
                        #region --Parse Asset Bools--
                        case "AddToNetregs ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.AddToNetregs = value;
                                break;
                            }
                        case "RemoveFromNetregs ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.RemoveFromNetregs = value;
                                break;
                            }
                        case "AddToMeshVariations ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.AddToMeshVariations = value;
                                break;
                            }
                        case "RemoveFromMeshVariations ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.RemoveFromMeshVariations = value;
                                break;
                            }
                        case "IsRecursive ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.IsRecursive = value;
                                break;
                            }
                        case "IgnoreTypes ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.IgnoreTypes = value;
                                if (opProperties.ExclusiveTypes)
                                {
                                    App.Logger.LogWarning("Exclusive types and IgnoreTypes are active at the same time, issues may occur.");
                                }
                                break;
                            }
                        case "ExclusiveTypes ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.ExclusiveTypes = value;
                                if (opProperties.IgnoreTypes)
                                {
                                    App.Logger.LogWarning("Exclusive types and IgnoreTypes are active at the same time, issues may occur.");
                                }
                                break;
                            }
                        case "OnlyModified":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.OnlyModified = value;
                                break;
                            }
                        #endregion

                        #region --Parse Lists--
                        case "Bundles ":
                            {
                                //Boolean.TryParse(line.Split('=').Last().TrimStart(' '), out bool value);
                                List<int> values = new List<int>();
                                foreach (string bundle in line.Split('=').Last().TrimStart(' ').TrimEnd(']').Split(',').ToList())
                                {
                                    if (App.AssetManager.GetBundleId(bundle) != -1)
                                    {
                                        values.Add(App.AssetManager.GetBundleId(bundle));
                                    }
                                }
                                opProperties.Bundles = values;
                                break;
                            }
                        case "Assets ":
                            {
                                //Boolean.TryParse(line.Split('=').Last().TrimStart(' '), out bool value);
                                List<Guid> values = new List<Guid>();
                                foreach (string asset in line.Split('=').Last().TrimStart(' ').TrimEnd(']').Split(',').ToList())
                                {
                                    switch (asset)
                                    {
                                        case "(Selected)":
                                            {
                                                if (App.EditorWindow.DataExplorer.SelectedAssets == null)
                                                {
                                                    values.Add((App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry).Guid);
                                                }
                                                else
                                                {
                                                    foreach (EbxAssetEntry assetEntry in App.EditorWindow.DataExplorer.SelectedAssets)
                                                    {
                                                        values.Add(assetEntry.Guid);
                                                    }
                                                }
                                                break;
                                            }
                                        case "(AllOfTypes)":
                                            {
                                                foreach (string type in opProperties.Types)
                                                {
                                                    foreach (EbxAssetEntry assetEntry in App.AssetManager.EnumerateEbx(type, opProperties.OnlyModified || opProperties.OnlyAdded))
                                                    {
                                                        values.Add(assetEntry.Guid);
                                                    }
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                if (App.AssetManager.GetEbxEntry(asset) != null)
                                                {
                                                    values.Add(App.AssetManager.GetEbxEntry(asset).Guid);
                                                }
                                                else
                                                {
                                                    App.Logger.LogError("{0} is not a valid", line);
                                                }
                                                break;
                                            }
                                    }
                                }
                                opProperties.Assets = values;
                                break;
                            }
                        case "Types ":
                            {
                                opProperties.Types = line.Split('=').Last().TrimStart(' ').TrimEnd(']').Split(',').ToList();
                                break;
                            }
                        #endregion

                        #region --Parse Bundle Properties--
                        case "BundlePath ":
                            {
                                opProperties.BundlePath = line.Split('=').Last().TrimStart(' ').TrimEnd(']');
                                break;
                            }
                        case "SuperBundleName ":
                            {
                                if (!ProfilesLibrary.IsLoaded(ProfileVersion.StarWarsBattlefrontII))
                                {
                                    if (App.AssetManager.GetSuperBundleId(line.Split('=').Last().TrimStart(' ').TrimEnd(']')) != -1)
                                    {
                                        opProperties.SuperBundleName = line.Split('=').Last().TrimStart(' ').TrimEnd(']');
                                    }
                                    else
                                    {
                                        App.Logger.LogError("{0} is not a valid super bundle.", line.Split('=').Last().TrimStart(' ').TrimEnd(']'));
                                    }
                                }
                                else
                                {
                                    App.Logger.LogError("{0} does not have superbundles, as a result name is not needed.", ProfilesLibrary.DisplayName);
                                }
                                break;
                            }
                        case "BundleType ":
                            {
                                switch (line.Split('=').Last().TrimStart(' ').TrimEnd(']'))
                                {
                                    case "Shared":
                                        {
                                            opProperties.BundleType = BundleType.SharedBundle; 
                                            break;
                                        }
                                    case "Sublevel":
                                        {
                                            opProperties.BundleType = BundleType.SubLevel;
                                            break;
                                        }
                                    case "Blueprint":
                                        {
                                            opProperties.BundleType = BundleType.BlueprintBundle;
                                            break;
                                        }
                                    default:
                                        {
                                            App.Logger.LogError("{0} is not a valid bundle type.", line.Split('=').Last().TrimStart(' ').TrimEnd(']'));
                                            break;
                                        }
                                }
                                break;
                            }
                        case "GenerateBlueprints ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.GenerateBlueprints = value;
                                break;
                            }
                            #endregion
                    }
                }
                else
                {
                    opProperties.TriggerInstruction(line.Replace("{", "").Replace("}", ""), task);
                }

                line = sr.ReadLine(); //This will set the current line to the next line
            }
            App.EditorWindow.DataExplorer.RefreshItems();
            sr.Close(); //Close the file once we are done
        }
    }

    /// <summary>
    /// A list of properties that a BundleOperation has
    /// </summary>
    public class BunOpProperties
    {
        #region --Asset Bools--
        public bool AddToBundles = true;
        public bool RemoveFromBundles = true;
        public bool AddToNetregs = false;
        public bool RemoveFromNetregs = false;
        public bool AddToMeshVariations = false;
        public bool RemoveFromMeshVariations = false;
        public bool AddToUnlockIdTables = false; //Only applies to games with UnlockIdTables
        public bool RemoveFromUnlockIdTables = false; //Only applies to games with UnlockIdTables
        public bool IsRecursive = false;
        public bool IgnoreTypes = false;
        public bool ExclusiveTypes = false;
        public bool AffectsRecursive = false;
        public bool ForceAdd = false;
        public bool OnlyModified = false;
        public bool OnlyAdded = false;
        #endregion

        #region --Asset Lists--
        public List<int> Bundles = new List<int>();
        public List<Guid> Assets = new List<Guid>();
        public List<string> Types = new List<string>();
        #endregion

        #region --Bundle Properties--
        public string BundlePath = ""; //The filepath of the bundle, not the name of the bundle itself
        public string SuperBundleName = ""; //Not used for swbf2
        public BundleType BundleType = BundleType.None; //Shared, Sublevel, Blueprint
        public bool GenerateBlueprints = true;
        public string BlueprintType = "BlueprintBundle"; //The type we should create for our new BlueprintBundle
        #endregion

        /// <summary>
        /// Trigger an instruction of a name. The name must be without Curly brackets("{}")
        /// </summary>
        /// <param name="InstructionName">AddAssets, RemoveAssets, CreateBundle, RemoveCreatedBundle</param>
        /// <param name="task"></param>
        public void TriggerInstruction(string InstructionName, FrostyTaskWindow task)
        {
            switch (InstructionName)
            {
                #region --Asset Instructions--
                case "AddAssets":
                    {
                        List<Guid> assetsToBundle = new List<Guid>(Assets);
                        List<Guid> bundledAssets = new List<Guid>();
                        EbxAssetEntry currentAsset;

                        while (assetsToBundle.Count != 0)
                        {
                            currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            foreach (int bunId in Bundles)
                            {
                                BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                //Adding
                                if ((BundleEditors.AssetRecAddValid(currentAsset, bundle) || ForceAdd) && ((!OnlyModified || currentAsset.IsModified) || (!OnlyAdded || currentAsset.IsAdded)) && !bundledAssets.Contains(currentAsset.Guid))
                                {
                                    if ((IgnoreTypes && !Types.Contains(currentAsset.Type)) || (ExclusiveTypes && Types.Contains(currentAsset.Type)) || (!IgnoreTypes && !ExclusiveTypes))
                                    {
                                        if (AddToBundles)
                                        {
                                            BundleEditors.AddAssetToBundle(currentAsset, bundle);
                                        }
                                        if (AddToNetregs && BundleEditors.AssetAddNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToNetRegs(currentAsset, bundle);
                                        }
                                        else if (AddToMeshVariations && BundleEditors.AssetAddMeshVariationValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToMVDBs(currentAsset, bundle, task);
                                        }
                                        if (AddToUnlockIdTables && BundleEditors.AssetAddNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToTables(currentAsset, bundle);
                                        }
                                    }
                                }
                                if (IsRecursive)
                                {
                                    if (!(OnlyAdded || OnlyModified || ExclusiveTypes || IgnoreTypes) || !AffectsRecursive)
                                    {
                                        assetsToBundle.AddRange(currentAsset.EnumerateDependencies());
                                    }
                                    else
                                    {
                                        foreach (Guid reference in currentAsset.EnumerateDependencies())
                                        {
                                            EbxAssetEntry referencedAsset = App.AssetManager.GetEbxEntry(reference);
                                            if ((referencedAsset.IsAdded && OnlyAdded) || (referencedAsset.IsModified && OnlyAdded) || (Types.Contains(referencedAsset.Type) && ExclusiveTypes) || (!Types.Contains(referencedAsset.Type) && IgnoreTypes))
                                            {

                                            }
                                        }
                                    }
                                }
                            }

                            //Recursive stuff
                            assetsToBundle.RemoveAt(0); //Remove the asset we are currently on since we have already bundled it
                            bundledAssets.Add(currentAsset.Guid); //We have bundled it
                        }

                        break;
                    }
                case "RemoveAssets":
                    {
                        List<Guid> assetsToBundle = new List<Guid>(Assets);
                        List<Guid> bundledAssets = new List<Guid>();
                        EbxAssetEntry currentAsset;

                        while (assetsToBundle.Count != 0)
                        {
                            currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            foreach (int bunId in Bundles)
                            {
                                BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                //Removing
                                if ((BundleEditors.AssetRecRemValid(currentAsset, bundle) || ForceAdd) && (OnlyModified || currentAsset.IsModified) && !bundledAssets.Contains(currentAsset.Guid))
                                {
                                    if ((IgnoreTypes && !Types.Contains(currentAsset.Type)) || (ExclusiveTypes && Types.Contains(currentAsset.Type)) || (!IgnoreTypes && !ExclusiveTypes))
                                    {
                                        if (RemoveFromBundles)
                                        {
                                            BundleEditors.RemoveAssetFromBundle(currentAsset, bundle);
                                        }
                                        if (AddToNetregs && BundleEditors.AssetRemNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromNetRegs(currentAsset, bundle);
                                        }
                                        else if (AddToMeshVariations && BundleEditors.AssetRemMeshVariationValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromMeshVariations(currentAsset, bundle);
                                        }
                                        if (RemoveFromUnlockIdTables && BundleEditors.AssetRemNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromTables(currentAsset, bundle);
                                        }
                                    }
                                }
                                if (IsRecursive)
                                {
                                    assetsToBundle.AddRange(currentAsset.EnumerateDependencies());
                                }
                            }

                            //Recursive stuff
                            assetsToBundle.RemoveAt(0); //Remove the asset we are currently on since we have already bundled it
                            bundledAssets.Add(currentAsset.Guid); //We have bundled it
                        }

                        break;
                    }
                case "ClearAssets":
                    {
                        List<Guid> assetsToBundle = new List<Guid>(Assets);
                        List<Guid> bundledAssets = new List<Guid>();
                        EbxAssetEntry currentAsset;

                        while (assetsToBundle.Count != 0)
                        {
                            currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            foreach (int bunId in currentAsset.AddedBundles)
                            {
                                BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                //Removing
                                if ((BundleEditors.AssetRecRemValid(currentAsset, bundle) || ForceAdd) && (OnlyModified || currentAsset.IsModified) && !bundledAssets.Contains(currentAsset.Guid))
                                {
                                    if ((IgnoreTypes && !Types.Contains(currentAsset.Type)) || (ExclusiveTypes && Types.Contains(currentAsset.Type)) || (!IgnoreTypes && !ExclusiveTypes))
                                    {
                                        if (RemoveFromBundles)
                                        {
                                            BundleEditors.RemoveAssetFromBundle(currentAsset, bundle);
                                        }
                                        if (AddToNetregs && BundleEditors.AssetRemNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromNetRegs(currentAsset, bundle);
                                        }
                                        else if (AddToMeshVariations && BundleEditors.AssetRemMeshVariationValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromMeshVariations(currentAsset, bundle);
                                        }
                                        if (RemoveFromUnlockIdTables && BundleEditors.AssetRemNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.RemoveAssetFromTables(currentAsset, bundle);
                                        }
                                    }
                                }
                                if (IsRecursive)
                                {
                                    assetsToBundle.AddRange(currentAsset.EnumerateDependencies());
                                }
                            }

                            //Recursive stuff
                            assetsToBundle.RemoveAt(0); //Remove the asset we are currently on since we have already bundled it
                            bundledAssets.Add(currentAsset.Guid); //We have bundled it
                        }

                        break;
                    }
                #endregion

                #region --Bundle Instructions--
                case "CreateBundle":
                    {
                        if (!string.IsNullOrEmpty(BundlePath) && BundleType != BundleType.None)
                        {
                            BundleEntry newBundle = App.AssetManager.AddBundle("win32/" + BundlePath.ToLower(), BundleType, App.AssetManager.GetSuperBundleId(SuperBundleName));
                            if (GenerateBlueprints)
                            {
                                if (BundleType == BundleType.BlueprintBundle)
                                {
                                    EbxAsset blueprint = new EbxAsset(TypeLibrary.CreateObject("BlueprintBundle"));
                                    blueprint.SetFileGuid(Guid.NewGuid());

                                    dynamic obj = blueprint.RootObject;
                                    obj.Name = BundlePath;

                                    AssetClassGuid guid = new AssetClassGuid(Utils.GenerateDeterministicGuid(blueprint.Objects, (Type)obj.GetType(), blueprint.FileGuid), -1);
                                    obj.SetInstanceGuid(guid);

                                    EbxAssetEntry newEntry = App.AssetManager.AddEbx(BundlePath, blueprint);

                                    newEntry.AddedBundles.Add(App.AssetManager.GetBundleId(newBundle));
                                    newEntry.ModifiedEntry.DependentAssets.AddRange(blueprint.Dependencies);
                                }
                                else if (BundleType == BundleType.SubLevel)
                                {
                                    EbxAsset blueprint = new EbxAsset(TypeLibrary.CreateObject("SubWorldData"));
                                    blueprint.SetFileGuid(Guid.NewGuid());

                                    dynamic obj = blueprint.RootObject;
                                    obj.Name = BundlePath;

                                    AssetClassGuid guid = new AssetClassGuid(Utils.GenerateDeterministicGuid(blueprint.Objects, (Type)obj.GetType(), blueprint.FileGuid), -1);
                                    obj.SetInstanceGuid(guid);

                                    EbxAssetEntry newEntry = App.AssetManager.AddEbx(BundlePath, blueprint);

                                    newEntry.AddedBundles.Add(App.AssetManager.GetBundleId(newBundle));
                                    newEntry.ModifiedEntry.DependentAssets.AddRange(blueprint.Dependencies);
                                }

                                //Create 2 additional blueprints: mvdb and netreg
                                for (int i = 1; i != 2; i++)
                                {
                                    EbxAsset blueprint;
                                    string name;
                                    if (i == 1)
                                    {
                                        blueprint = new EbxAsset(TypeLibrary.CreateObject("MeshVariationDatabase"));
                                        name = "MeshVariationDb_Win32";
                                    }
                                    else
                                    {
                                        blueprint = new EbxAsset(TypeLibrary.CreateObject("NetworkRegistryAsset"));
                                        name = $"{BundlePath.ToLower().Split('/').Last()}'_networkregistry_Win32";
                                    }
                                    blueprint.SetFileGuid(Guid.NewGuid());

                                    dynamic obj = blueprint.RootObject;
                                    obj.Name = name;

                                    AssetClassGuid guid = new AssetClassGuid(Utils.GenerateDeterministicGuid(blueprint.Objects, (Type)obj.GetType(), blueprint.FileGuid), -1);
                                    obj.SetInstanceGuid(guid);

                                    EbxAssetEntry newEntry = App.AssetManager.AddEbx(name, blueprint);

                                    newEntry.AddedBundles.Add(App.AssetManager.GetBundleId(newBundle));
                                    newEntry.ModifiedEntry.DependentAssets.AddRange(blueprint.Dependencies);
                                }
                            }
                        }
                        else
                        {
                            App.Logger.LogError("Bundle was invalid, could not create.");
                        }
                        break;
                    }
                #endregion

                #region --Operator Instructions--
                case "ResetPropertiesToDefault":
                    {
                        ResetPropertiesToDefault();
                        break;
                    }
                #endregion

                default:
                    {
                        App.Logger.LogError("{0} is not a valid instruction", InstructionName);
                        break;
                    }
            }
        }

        /// <summary>
        /// Resets all properties to default values
        /// </summary>
        public void ResetPropertiesToDefault()
        {
            #region --Asset Bools--
            AddToBundles = true;
            RemoveFromBundles = true;
            AddToNetregs = false;
            RemoveFromNetregs = false;
            AddToMeshVariations = false;
            RemoveFromMeshVariations = false;
            AddToUnlockIdTables = false; //Only applies to games with UnlockIdTables
            RemoveFromUnlockIdTables = false; //Only applies to games with UnlockIdTables
            IsRecursive = false;
            IgnoreTypes = false;
            ExclusiveTypes = false;
            AffectsRecursive = false;
            ForceAdd = false;
            OnlyModified = false;
            OnlyAdded = false;
            #endregion

            #region --Asset Lists--
            List<int> Bundles = new List<int>();
            List<Guid> Assets = new List<Guid>();
            List<string> Types = new List<string>();
            #endregion

            #region --Bundle Properties--
            BundlePath = ""; //The filepath of the bundle, not the name of the bundle itself
            SuperBundleName = ""; //Not used for swbf2
            BundleType = BundleType.None; //Shared, Sublevel, Blueprint
            GenerateBlueprints = true;
            BlueprintType = "BlueprintBundle"; //The type we should create for our new BlueprintBundle
            #endregion
        }
    }
}
