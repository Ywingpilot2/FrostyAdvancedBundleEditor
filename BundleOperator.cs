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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Python.Runtime;
using Frosty.Controls;
using System.Windows;
using AdvancedBundleEditorPlugin.Windows;

namespace AdvancedBundleEditorPlugin
{
    public class BundleOperator
    {
        public string BunOpName { get; private set; }
        private static string BunOpPath { get; set; }

        /// <summary>
        /// Initiate a new Bundle Operation
        /// </summary>
        public void InitiateBundleOperation()
        {
            //create a open file dialogue so the user can select a file
            FrostyOpenFileDialog ofd = new FrostyOpenFileDialog("Open Bundle Operation", "Bundle Operation Config (*.bunop)|*.bunop|Python Script (*.py)|*.py", "BundleOperation");
            if (!ofd.ShowDialog()) return;
            BunOpName = ofd.FileName.Replace("\\", "/").Split('/').Last();
            BunOpPath = ofd.FileName;
            FrostyTaskWindow.Show(BunOpName, "", (task) =>
            {
                if (BunOpPath.EndsWith(".bunop"))
                {
                    ReadBunop(task);
                }
                else if (BunOpName != "Frostpy.py")
                {
                    ReadPy(task);
                }
                else
                {
                    App.Logger.LogError("Can't open file {0}, it is not a valid operation.", BunOpName);
                }
            });
            App.EditorWindow.DataExplorer.RefreshAll();
        }

        #region --Bunop(pseudo xml reading)--
        /// <summary>
        /// Reads a .bunop file. A .bunop is a pseudo xml style configuration file for Bundle Operations.
        /// </summary>
        private void ReadBunop(FrostyTaskWindow task)
        {
            BunOpProperties opProperties = new BunOpProperties();

            StreamReader sr = new StreamReader(BunOpPath);
            string line = sr.ReadLine();
            while (line != null)
            {
                //First we need to setup the line for reading
                int commentPosition = line.IndexOf("//"); //Remove comments
                if (commentPosition != -1 && commentPosition != 0)
                {
                    line = line.Remove(commentPosition).TrimEnd(' ');
                }
                //If the line is a comment, then we don't bother parsing it
                if (commentPosition == 0 || string.IsNullOrEmpty(line))
                {
                    line = sr.ReadLine();
                    continue;
                }

                //If it starts with a [ that means its a Property
                if (line.StartsWith("["))
                {
                    //Extract the name of the property, probably a better way to do this.
                    switch (line.Replace("[", "").Replace("]", "").Split('=').First().Trim(' '))
                    {
                        #region --Parse Asset Bools--
                        //Each of these extracts then parses the bools from the line
                        case "AddToNetregs":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.AddToNetregs = value;
                                break;
                            }
                        case "RemoveFromNetregs":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.RemoveFromNetregs = value;
                                break;
                            }
                        case "AddToUnlockIdTables":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.AddToUnlockIdTables = value;
                                break;
                            }
                        case "RemoveFromUnlockIdTables":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.RemoveFromUnlockIdTables = value;
                                break;
                            }
                        case "AddToMeshVariations":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.AddToMeshVariations = value;
                                break;
                            }
                        case "RemoveFromMeshVariations":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.RemoveFromMeshVariations = value;
                                break;
                            }
                        case "IsRecursive":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.IsRecursive = value;
                                break;
                            }
                        case "IgnoreTypes":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.IgnoreTypes = value;
                                if (opProperties.ExclusiveTypes)
                                {
                                    App.Logger.LogWarning("Exclusive types and IgnoreTypes are active at the same time, issues may occur.");
                                }
                                break;
                            }
                        case "ExclusiveTypes":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.ExclusiveTypes = value;
                                if (opProperties.IgnoreTypes)
                                {
                                    App.Logger.LogWarning("Exclusive types and IgnoreTypes are active at the same time, issues may occur.");
                                }
                                break;
                            }
                        case "OnlyModified":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.OnlyModified = value;
                                break;
                            }
                        case "OnlyAdded":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.OnlyAdded = value;
                                break;
                            }
                        case "ForceAdd":
                        {
                            bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                            opProperties.ForceAdd = value;
                            break;
                        }
                        #endregion

                        #region --Parse Lists--
                        case "Bundles":
                            {
                                //Boolean.TryParse(line.Split('=').Last().TrimStart(' '), out bool value);
                                List<int> values = new List<int>();
                                foreach (string bundle in line.Split('=').Last().TrimEnd(']').Trim(' ').Split(',').ToList())
                                {
                                    if (App.AssetManager.GetBundleId(bundle.Trim(' ')) != -1)
                                    {
                                        values.Add(App.AssetManager.GetBundleId(bundle.Trim(' ')));
                                    }
                                    else
                                    {
                                        App.Logger.LogError("{0} is not a valid bundle", bundle.Trim(' '));
                                    }
                                }
                                opProperties.Bundles = values;
                                break;
                            }
                        case "Assets":
                            {
                                //Boolean.TryParse(line.Split('=').Last().TrimStart(' '), out bool value);
                                List<Guid> values = new List<Guid>();
                                foreach (string asset in line.Split('=').Last().TrimEnd(']').Trim(' ').Split(',').ToList())
                                {
                                    switch (asset.Trim(' '))
                                    {
                                        case "(Selected)":
                                            {
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                    if (App.EditorWindow.DataExplorer.SelectedAssets == null)
                                                    {
                                                        values.Add(((EbxAssetEntry)App.EditorWindow.DataExplorer.SelectedAsset).Guid);
                                                    }
                                                    else
                                                    {
                                                        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                                                        // ^ SelectedAssets will never be invalid in this case(at least as far as I have tested)
                                                        foreach (EbxAssetEntry assetEntry in App.EditorWindow.DataExplorer.SelectedAssets)
                                                        {
                                                            values.Add(assetEntry.Guid);
                                                        }
                                                    }        
                                                });
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
                                                if (App.AssetManager.GetEbxEntry(asset.Trim(' ')) != null)
                                                {
                                                    values.Add(App.AssetManager.GetEbxEntry(asset.Trim(' ')).Guid);
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
                        case "Types":
                            {
                                opProperties.Types = line.Split('=').Last().TrimEnd(']').Trim(' ').Split(',').ToList();
                                break;
                            }
                        #endregion

                        #region --Parse Bundle Properties--
                        case "BundlePath":
                            {
                                opProperties.BundlePath = line.Split('=').Last().TrimEnd(']').Trim(' ');
                                break;
                            }
                        case "SuperBundleName":
                            {
                                //swbf2 doesn't have super bundles, so no need to bother loading them
                                if (!ProfilesLibrary.IsLoaded(ProfileVersion.StarWarsBattlefrontII))
                                {
                                    if (App.AssetManager.GetSuperBundleId(line.Split('=').Last().TrimEnd(']').Trim(' ')) != -1)
                                    {
                                        opProperties.SuperBundleName = line.Split('=').Last().TrimEnd(']').Trim(' ');
                                    }
                                    else
                                    {
                                        App.Logger.LogError("{0} is not a valid super bundle.", line.Split('=').Last().TrimEnd(']').Trim(' '));
                                    }
                                }
                                else
                                {
                                    App.Logger.LogError("{0} does not have Super Bundles, as a result name is not needed.", ProfilesLibrary.DisplayName);
                                }
                                break;
                            }
                        case "BundleType":
                            {
                                //Annoying setup to extract the BundleType from the string
                                switch (line.Split('=').Last().TrimEnd(']').Trim(' '))
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
                                            App.Logger.LogError("{0} is not a valid bundle type.", line.Split('=').Last().TrimEnd(']').Trim(' '));
                                            break;
                                        }
                                }
                                break;
                            }
                        case "GenerateBlueprints":
                            {
                                bool.TryParse(line.Split('=').Last().TrimEnd(']').Trim(' '), out bool value);
                                opProperties.GenerateBlueprints = value;
                                break;
                            }
                        case "BlueprintType":
                        {
                            opProperties.BlueprintType = line.Split('=').Last().TrimEnd(']').Trim(' ');
                            break;
                        }
                        #endregion
                    }
                }
                else //It has to be an Instruction if its not a Property
                {
                    //We need to remove curly brackets from the Instruction so TriggerInstruction() can parse it
                    opProperties.TriggerInstruction(line.Replace("{", "").Replace("}", ""), task);
                }

                line = sr.ReadLine(); //This will set the current line to the next line
            }
            sr.Close(); //Close the file once we are done
        }
        #endregion

        #region --Bunpy(Python API reading)--
        private static void ReadPy(FrostyTaskWindow task)
        {
            //Check if we need to Initialize the BunpyApi or not
            if (!BunpyApi.PythonInstallValid)
            {
                BunpyApi.Initialize(task);   
            }
            if (BunpyApi.PythonInstallValid)
            {
                BunpyApi.ExecutePy(BunOpPath, task);
            }
        }
        #endregion
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
        /// <param name="instructionName">AddAssets, RemoveAssets, CreateBundle, RemoveCreatedBundle</param>
        /// <param name="task"></param>
        public void TriggerInstruction(string instructionName, FrostyTaskWindow task)
        {
            switch (instructionName)
            {
                #region --Asset Instructions--
                case "AddAssets":
                    {
                        List<Guid> assetsToBundle = new List<Guid>(Assets);
                        List<Guid> bundledAssets = new List<Guid>();

                        while (assetsToBundle.Count != 0)
                        {
                            EbxAssetEntry currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            foreach (int bunId in Bundles)
                            {
                                BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                //Adding
                                if ((BundleEditors.AssetOpAddValid(currentAsset, bundle) || ForceAdd) && ((!OnlyModified || currentAsset.IsModified) || (!OnlyAdded || currentAsset.IsAdded)) && !bundledAssets.Contains(currentAsset.Guid))
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
                                            BundleEditors.AddAssetToMvdBs(currentAsset, bundle, task);
                                        }
                                        if (AddToUnlockIdTables && BundleEditors.AssetAddNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToTables(currentAsset, bundle);
                                        }
                                    }
                                }
                                if (IsRecursive)
                                {
                                    if (!AffectsRecursive)
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
                                                assetsToBundle.Add(reference);
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

                        while (assetsToBundle.Count != 0)
                        {
                            EbxAssetEntry currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            foreach (int bunId in Bundles)
                            {
                                BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                //Removing
                                if (((!OnlyModified || currentAsset.IsModified) || (!OnlyAdded || currentAsset.IsAdded)) && !bundledAssets.Contains(currentAsset.Guid))
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
                                    if (!AffectsRecursive)
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
                                                assetsToBundle.Add(reference);
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
                case "ClearAssets":
                    {
                        List<Guid> assetsToBundle = new List<Guid>(Assets);
                        List<Guid> bundledAssets = new List<Guid>();

                        while (assetsToBundle.Count != 0)
                        {
                            EbxAssetEntry currentAsset = App.AssetManager.GetEbxEntry(assetsToBundle[0]);
                            if (((!OnlyModified || currentAsset.IsModified) || (!OnlyAdded || currentAsset.IsAdded)) && !bundledAssets.Contains(currentAsset.Guid))
                            {
                                if ((IgnoreTypes && !Types.Contains(currentAsset.Type)) || (ExclusiveTypes && Types.Contains(currentAsset.Type)) || (!IgnoreTypes && !ExclusiveTypes))
                                {
                                    while (currentAsset.AddedBundles.Count != 0)
                                    {
                                        int bunId = currentAsset.AddedBundles[0];
                                        BundleEntry bundle = App.AssetManager.GetBundleEntry(bunId);

                                        //Removing
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
                            }
                            if (IsRecursive)
                            {
                                if (!AffectsRecursive)
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
                                            assetsToBundle.Add(reference);
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
                                    newBundle.Blueprint = newEntry;
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
                                    newBundle.Blueprint = newEntry;
                                }

                                //Create 2 additional blueprints: mvdb and netreg
                                for (int i = 0; i != 2; i++)
                                {
                                    EbxAsset blueprint;
                                    string name;
                                    if (i == 1)
                                    {
                                        blueprint = new EbxAsset(TypeLibrary.CreateObject("MeshVariationDatabase"));
                                        name = BundlePath + "/MeshVariationDb_Win32";
                                    }
                                    else
                                    {
                                        blueprint = new EbxAsset(TypeLibrary.CreateObject("NetworkRegistryAsset"));
                                        name = BundlePath.Replace(BundlePath.Split().Last(), "") + BundlePath.Split().Last().ToLower() + "_networkregistry_Win32";
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
                        App.Logger.LogError("{0} is not a valid instruction", instructionName);
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
            Bundles = new List<int>();
            Assets = new List<Guid>();
            Types = new List<string>();
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

    #region --Python API--
    
    public static class BunpyApi
    {
        private static string pythonDll = ""; //Path gets set later down the line
        public static bool PythonInstallValid;

        #region --Executing Python Code(C# implementation)--
        /// <summary>
        /// Initiate the BunpyApi. This involves tracking down the Python 3.8 installation, then finding/installing Pythonnet 2.5
        /// </summary>
        /// <param name="task"></param>
        public static void Initialize(FrostyTaskWindow task)
        {
            task.Update("Determining Python 3.8 installation...");
            
            //Really annoying way of brute forcing our way to the installations
            if (File.Exists($@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\python38.dll"))
            {
                //If the pythonDll is in its default install location
                pythonDll = $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\python38.dll";
                Config.Add("BunpyLocation", pythonDll);
                task.Update("Python.dll found!");

                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            else if (File.Exists(@Config.Get("BunpyLocation", "") + @"\python38.dll")) //Get the path from the user
            {
                //If its not in the default install location then the user has to configure it manually
                pythonDll = @Config.Get("BunpyLocation", "") + @"\python38.dll";
                task.Update("Python.dll found!");

                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            else
            {
                Process.Start("https://www.python.org/downloads/release/python-380/");
                App.Logger.Log("Failed to find python38.dll! Please either install Python 3.8.x, or if its already installed, set the 'Python 3.8 location' setting in Bundle Editor Options.");
            }

            task.Update("Determining Pythonnet status...");
            if (File.Exists($@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\Lib\site-packages\clr.pyd"))
            {
                task.Update("Pythonnet valid!");
                Config.Add("PynetLocation",
                    $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\Lib\site-packages\clr.pyd");
                PythonInstallValid = true;
            }
            else if (File.Exists(@Config.Get("BunpyLocation", "") + @"\Lib\site-packages\clr.pyd")) //Get the path from the user
            {
                task.Update("Pythonnet valid!");
                PythonInstallValid = true;
            }
            else
            {
                task.Update("Invalid Pythonnet installation...");
                MessageBoxResult result = FrostyMessageBox.Show("Pythonnet's installation seems to be invalid or I have been unable to find it. Would you like me to install it for you?", "Bundle Operator", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    //We first try to uninstall pythonnet incase the user has an invalid installation
                    //This will help with errors
                    Process.Start("CMD.exe", "/C py -3.8 -m pip uninstall pythonnet");
                    Process cmd = Process.Start("CMD.exe", "/C py -3.8 -m pip install pythonnet==2.5.0");
                    if (cmd != null && cmd.HasExited && File.Exists($@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\Lib\site-packages\clr.pyd"))
                    {
                        task.Update("Pythonnet valid!");
                        Config.Add("PynetLocation",
                            $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python\Python38\Lib\site-packages\clr.pyd");
                        PythonInstallValid = true;
                    }
                    else if (File.Exists(@Config.Get("BunpyLocation", "") + @"\Lib\site-packages\clr.pyd"))
                    {
                        task.Update("Pythonnet valid!");
                        PythonInstallValid = true;
                    }
                    else
                    {
                        App.Logger.Log("Error installing/finding Pythonnet");
                    }
                }
            }
        }

        /// <summary>
        /// This will process then run a set of Python code from a .py file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="task"></param>
        public static void ExecutePy(string path, FrostyTaskWindow task)
        {
            //First we need to parse the python file
            task.Update("Parsing Python...");
            StreamReader sr = new StreamReader(path);
            string line = @sr.ReadLine();
            string code = ""; //Empty for now, this will be filled with the code we need PythonEngine to execute

            while (line != null)
            {
                code += "\n" + line; //We go through line by line adding it to the Code
                //We add \n to it that way it gets put on a new line each time
                line = @sr.ReadLine();
            }
            sr.Close();
            
            task.Update("Executing python...");
            var gil = Py.GIL(); //No idea what the purpose of this is tbh, I just know its important
            try
            {
                //Process the code so PythonEngine can actually execute it
                //(by default it uses dummy code from Frostpy.BunpyApi, we need to replace this with our C# code)
                string processedCode = ProcessCode(code);
                PythonEngine.Exec(processedCode); //This will run the processed code
            }
            catch (Exception e)
            {
                App.Logger.LogError("Execution of python code failed with exception: {0}", e.Message);
                gil.Dispose();
            }
            finally
            {
                gil.Dispose(); //This should always be the last thing we do when executing python code
            }
        }

        /// <summary>
        /// This processes a line of code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string ProcessCode(string code)
        {
            //Replace all of our strings with the actual ones
            //Very lengthy way of doing this, I'm sure it can be done better but it works for now
            string processedCode = "";
            foreach (string line in code.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("#")) continue; //strip code comments so they don't get in the way
                
                int commentPosition = line.IndexOf("#"); //Remove comments
                string cleanLine = line;
                if (commentPosition != -1 && commentPosition != 0)
                {
                    cleanLine = line.Remove(commentPosition).TrimEnd(' ');
                }
                
                switch (cleanLine)
                {
                    case "import Frostpy":
                        processedCode += "import clr\nclr.AddReference(\"AdvancedBundleEditorPlugin\")\nimport AdvancedBundleEditorPlugin\n";
                        break;
                    case "from Frostpy import BunpyApi":
                        processedCode += "import clr\nclr.AddReference(\"AdvancedBundleEditorPlugin\")\nfrom AdvancedBundleEditorPlugin import BunpyApi\n";
                        break;
                    default:
                        processedCode += $"{line.Replace("Frostpy.BunpyApi.", "AdvancedBundleEditorPlugin.BunpyApi.")}\n";
                        break;
                }
            }
            //string processedCode = code.Replace("import Frostpy", "import clr\nclr.AddReference(\"AdvancedBundleEditorPlugin\")\nimport AdvancedBundleEditorPlugin").Replace("from Frostpy import BunpyApi", "import clr\nclr.AddReference(\"AdvancedBundleEditorPlugin\")\nfrom AdvancedBundleEditorPlugin import BunpyApi");
            return processedCode;
        }
        #endregion

        #region --API(Python implementation)--

        #region --Classes--
        /// <summary>
        /// Dummy class for an asset that Bunpy can interact with. Keep it nice and simple as to not upset pythonnet too much
        /// (so e.g use standard ints or floats and avoid uints or decimals, also try not to pass frosty's classes through)        
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public class Asset
        {
            #region Properties

            public string FilePath { get; set; }

            public string Filename => FilePath.Split('/').Last();

            public bool IsModified
            {
                get
                {
                    //First we check if our asset is valid or not
                    if (App.AssetManager.GetEbxEntry(FilePath) == null)
                    {
                        App.Logger.LogError("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                        return false;
                    }
                    EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                    return assetEntry.IsModified;
                }
            }

            public bool IsAdded
            {
                get
                {
                    //First we check if our asset is valid or not
                    if (App.AssetManager.GetEbxEntry(FilePath) == null)
                    {
                        App.Logger.LogError("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                        return false;
                    }
                    EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                    return assetEntry.IsAdded;
                }
            }

            public string Type
            {
                get
                {
                    //First we check if our asset is valid or not
                    if (App.AssetManager.GetEbxEntry(FilePath) == null)
                    {
                        App.Logger.LogError("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                        return "Invalid";
                    }
                    EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                    return assetEntry.Type;
                }
            }

            public List<Bundle> OriginalBundles
            {
                get
                {
                    if (App.AssetManager.GetEbxEntry(FilePath) == null)
                    {
                        App.Logger.Log("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                        return new List<Bundle>();
                    }
                    EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                    List<Bundle> bundles = new List<Bundle>();
                    foreach (int bunId in assetEntry.Bundles)
                    {
                        bundles.Add((Bundle)App.AssetManager.GetBundleEntry(bunId));
                    }

                    return bundles;
                }
            }
            
            public List<Bundle> AddedBundles
            {
                get
                {
                    if (App.AssetManager.GetEbxEntry(FilePath) == null)
                    {
                        App.Logger.Log("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                        return new List<Bundle>();
                    }
                    EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                    List<Bundle> bundles = new List<Bundle>();
                    foreach (int bunId in assetEntry.AddedBundles)
                    {
                        bundles.Add((Bundle)App.AssetManager.GetBundleEntry(bunId));
                    }

                    return bundles;
                }
            }

            public List<Bundle> AllBundles
            {
                get
                {
                    List<Bundle> bundles = new List<Bundle>(OriginalBundles);
                    bundles.AddRange(AddedBundles);
                    return bundles;
                }
            }

            #endregion

            #region Methods

            public List<Asset> GetReferences()
            {
                //First we check if our asset is valid or not
                if (App.AssetManager.GetEbxEntry(FilePath) == null)
                {
                    App.Logger.LogError("{0} is not a valid asset. Did you forget to set the FilePath?", FilePath);
                    return new List<Asset>();
                }
                EbxAssetEntry assetEntry = App.AssetManager.GetEbxEntry(FilePath);
                List<Asset> assets = new List<Asset>();
                foreach (Guid reference in assetEntry.EnumerateDependencies())
                {
                    EbxAssetEntry referencedAsset = App.AssetManager.GetEbxEntry(reference);
                    assets.Add((Asset)referencedAsset);
                }

                return assets;
            }

            #endregion

            public static implicit operator EbxAssetEntry(Asset asset) => App.AssetManager.GetEbxEntry(asset.FilePath);
            public static explicit operator Asset(string filePath) => new Asset(filePath);
            public static explicit operator Asset(EbxAssetEntry assetEntry) => new Asset(assetEntry.Name);
            
            /// <summary>
            /// This acts as the C# equivalent to python's __init__ method
            /// When creating constructors, make sure __init__ exists with the same params and such for the python dummy classes
            /// </summary>
            public Asset(string filePath)
            {
                FilePath = filePath;
            }
        }

        /// <summary>
        /// Dummy class for a bundle that Bunpy can interact with. Keep it nice and simple as to not upset pythonnet too much
        /// (so e.g use standard ints or floats and avoid uints or decimals, also try not to pass frosty's classes through)
        /// </summary>
        public class Bundle
        {
            public string Name { get; set; }
            public string Type => GetBundleType();
            public string SuperBundle => GetSuperBundle();

            public Asset GetBlueprint()
            {
                BundleEntry bundleEntry = App.AssetManager.GetBundleEntry(App.AssetManager.GetBundleId(Name));
                return (Asset)bundleEntry.Blueprint;
            }

            private string GetBundleType()
            {
                if (App.AssetManager.GetBundleId(Name) == -1)
                {
                    App.Logger.LogError("{0} is not a valid bundle. Did you forget to set the Name?", Name);
                    return "Invalid";
                }
                BundleEntry bundle = App.AssetManager.GetBundleEntry(App.AssetManager.GetBundleId(Name));
                switch(bundle.Type)
                {
                    case BundleType.SubLevel:
                        {
                            return "Sublevel";
                        }
                    case BundleType.BlueprintBundle:
                        {
                            return "Blueprint";
                        }
                    case BundleType.SharedBundle:
                        {
                            return "Shared";
                        }
                    case BundleType.None:
                    default:
                        {
                            return "Invalid";
                        }
                }
            }

            private string GetSuperBundle()
            {
                if (App.AssetManager.GetBundleId(Name) == -1)
                {
                    App.Logger.LogError("{0} is not a valid bundle. Did you forget to set the Name?", Name);
                    return "<none>";
                }
                BundleEntry bundle = App.AssetManager.GetBundleEntry(App.AssetManager.GetBundleId(Name));
                return App.AssetManager.GetSuperBundle(bundle.SuperBundleId).Name;
            }
            
            public static implicit operator BundleEntry(Bundle bundle) => App.AssetManager.GetBundleEntry(App.AssetManager.GetBundleId(bundle.Name));
            public static explicit operator Bundle(string filePath) => new Bundle(filePath);
            public static explicit operator Bundle(BundleEntry bundleEntry) => new Bundle(bundleEntry.Name);

            /// <summary>
            /// This acts as the C# equivelant to python's __init__ method
            /// When creating constructors, make sure __init__ exists with the same params and such for the python dummy classes
            /// </summary>
            public Bundle(string name)
            {
                Name = name;
            }
        }
        #endregion

        #region --Methods--

        #region --Assets--

        #region --Adding and Removing--

        #region Adding

        public static void AddToBundle(Asset assetToAdd, Bundle bundle)
        {
            if (!assetToAdd.AllBundles.Contains(bundle))
            {
                BundleEditors.AddAssetToBundle(assetToAdd, bundle);
            }
        }
        
        public static void AddToReg(Asset assetToAdd, Bundle bundle)
        {
            if (BundleEditors.AssetAddNetworkValid(assetToAdd, bundle))
            {
                BundleEditors.AddAssetToNetRegs(assetToAdd, bundle);
            }
        }

        public static void AddToMvdb(Asset asset, Bundle bundle)
        {
            if (BundleEditors.AssetAddMeshVariationValid(asset, bundle))
            {
                BundleEditors.AddAssetToMvdBs(asset, bundle);
            }
        }

        #endregion

        #region Removing

        public static void RemFromBundle(Asset asset, Bundle bundle)
        {
            if (asset.AddedBundles.Contains(bundle))
            {
                BundleEditors.RemoveAssetFromBundle(asset, bundle);
            }
        }
        
        public static void RemFromReg(Asset asset, Bundle bundle)
        {
            if (BundleEditors.AssetRemNetworkValid(asset, bundle))
            {
                BundleEditors.RemoveAssetFromNetRegs(asset, bundle);
            }
        }
        
        public static void RemFromMvdb(Asset asset, Bundle bundle)
        {
            if (BundleEditors.AssetRemMeshVariationValid(asset, bundle))
            {
                BundleEditors.RemoveAssetFromMeshVariations(asset, bundle);
            }
        }

        #endregion

        #endregion

        #region --Recursive Editing--

        #region Adding

        public static void RecAddToBundle(Asset assetToAdd, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = assetToAdd;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> addedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!addedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetRecAddValid(assetToCheck, bundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.AddAssetToBundle(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid); //This asset no longer needs to be checked in so it can be removed
                    addedAssets.Add(assetToCheck.Guid); //Since it has already been checked it should be added to the checked list
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies()); //We need to check its references too though, so add them
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }
        
        public static void RecAddToReg(Asset asset, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = asset;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> addedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!addedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetAddNetworkValid(assetToCheck, bundle) && BundleEditors.AssetRecAddValid(assetToCheck, bundle)) //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.AddAssetToNetRegs(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid); //This asset no longer needs to be checked in so it can be removed
                    addedAssets.Add(assetToCheck.Guid); //Since it has already been checked it should be added to the checked list
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies()); //We need to check its references too though, so add them
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }
        
        public static void RecAddToMvdb(Asset asset, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = asset;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> addedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!addedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetAddMeshVariationValid(assetToCheck, bundle) && BundleEditors.AssetRecAddValid(assetToCheck, bundle)) //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.AddAssetToMvdBs(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid); //This asset no longer needs to be checked in so it can be removed
                    addedAssets.Add(assetToCheck.Guid); //Since it has already been checked it should be added to the checked list
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies()); //We need to check its references too though, so add them
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }

        #endregion

        #region Removing

        public static void RecRemFromBundle(Asset assetToRemove, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = assetToRemove;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> checkedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!checkedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetRecRemValid(assetToCheck, bundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.RemoveAssetFromBundle(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid);
                    checkedAssets.Add(assetToCheck.Guid);
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }
        
        public static void RecRemFromReg(Asset assetToRemove, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = assetToRemove;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> checkedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!checkedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetRemNetworkValid(assetToCheck, bundle) && BundleEditors.AssetRecRemValid(assetToCheck, bundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.RemoveAssetFromNetRegs(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid);
                    checkedAssets.Add(assetToCheck.Guid);
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }
        
        public static void RecRemFromMvdb(Asset assetToRemove, Bundle bundle)
        {
            EbxAssetEntry assetToCheck = assetToRemove;
            List<Guid> assetsToCheck = new List<Guid>();
            List<Guid> checkedAssets = new List<Guid>();
                
            assetsToCheck.Add(assetToCheck.Guid);
            assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
            while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
            {
                if (!checkedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                {
                    assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked

                    if (BundleEditors.AssetRemMeshVariationValid(assetToCheck, bundle) && BundleEditors.AssetRecRemValid(assetToCheck, bundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                    {
                        //If it is, add it to bundles and netregs
                        BundleEditors.RemoveAssetFromMeshVariations(assetToCheck, bundle);
                    }

                    assetsToCheck.Remove(assetToCheck.Guid);
                    checkedAssets.Add(assetToCheck.Guid);
                    assetsToCheck.AddRange(assetToCheck.EnumerateDependencies());
                }
                else
                {
                    assetsToCheck.Remove(assetsToCheck[0]);
                }
            }
        }

        #endregion

        #endregion

        #region --Completing--

        public static void CompletelyAddAsset(Asset asset, Bundle bundle, bool recursive = true)
        {
            if (recursive)
            {
                RecAddToBundle(asset, bundle);
                RecAddToReg(asset, bundle);
                RecAddToMvdb(asset, bundle);
            }
            else
            {
                AddToBundle(asset, bundle);
                AddToReg(asset, bundle);
                AddToMvdb(asset, bundle);
            }
        }

        public static void CompletelyRemoveAsset(Asset asset, Bundle bundle, bool recursive = true)
        {
            if (recursive)
            {
                RecRemFromBundle(asset, bundle);
                RecRemFromReg(asset, bundle);
                RecRemFromMvdb(asset, bundle);
            }
            else
            {
                RemFromBundle(asset, bundle);
                RemFromReg(asset, bundle);
                RemFromMvdb(asset, bundle);
            }
        }

        #endregion

        #endregion

        #region Bundles

        public static void AddBundle(string bundlePath, string superBundleName, string bundleType = "Shared",
            bool generateBlueprints = true, string blueprintType = "BlueprintBundle")
        {
            //If the BundlePath is invalid then we don't do anything
            //TODO: Log an error in this case instead of just returning
            if (string.IsNullOrEmpty(bundlePath)) return;
            
            switch (bundleType)
            {
                case "Shared":
                {
                    BundleEntry newBundle = App.AssetManager.AddBundle("win32/" + bundlePath.ToLower(), BundleType.SharedBundle, App.AssetManager.GetSuperBundleId(superBundleName));
                    if (!generateBlueprints) return; //If we aren't generating any blueprints then our job here is done
                    //Since this is a shared bundle, we only need to create the Network Registries and Meshvariations
                    for (int i = 0; i != 2; i++)
                    {
                        EbxAsset blueprint;
                        string name;
                        if (i == 1)
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("MeshVariationDatabase"));
                            name = bundlePath + "/MeshVariationDb_Win32";
                        }
                        else
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("NetworkRegistryAsset"));
                            name = bundlePath.Replace(bundlePath.Split('/').Last(), "") + bundlePath.Split('/').Last().ToLower() + "_networkregistry_Win32";
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
                    break;
                }
                case "Sublevel":
                {
                    BundleEntry newBundle = App.AssetManager.AddBundle("win32/" + bundlePath.ToLower(), BundleType.SubLevel, App.AssetManager.GetSuperBundleId(superBundleName));
                    if (!generateBlueprints) return; //If we aren't generating any blueprints then our job here is done
                    
                    //Since this is a sublevel, we first need to create the Subworld
                    //TODO: move this process into a new method, that way we don't need to do this long setup every time we want to create ebx
                    EbxAsset sublevel = new EbxAsset(TypeLibrary.CreateObject("SubWorldData"));
                    sublevel.SetFileGuid(Guid.NewGuid());

                    dynamic sublevelobj = sublevel.RootObject;
                    sublevelobj.Name = bundlePath;

                    AssetClassGuid sublevelguid = new AssetClassGuid(Utils.GenerateDeterministicGuid(sublevel.Objects, (Type)sublevelobj.GetType(), sublevel.FileGuid), -1);
                    sublevelobj.SetInstanceGuid(sublevelguid);

                    EbxAssetEntry newSubLevelEntry = App.AssetManager.AddEbx(bundlePath, sublevel);

                    newSubLevelEntry.AddedBundles.Add(App.AssetManager.GetBundleId(newBundle));
                    newSubLevelEntry.ModifiedEntry.DependentAssets.AddRange(sublevel.Dependencies);
                    newBundle.Blueprint = newSubLevelEntry;
                    
                    for (int i = 0; i != 2; i++)
                    {
                        EbxAsset blueprint;
                        string name;
                        if (i == 1)
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("MeshVariationDatabase"));
                            name = bundlePath + "/MeshVariationDb_Win32";
                        }
                        else
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("NetworkRegistryAsset"));
                            name = bundlePath.Replace(bundlePath.Split().Last(), "") + bundlePath.Split().Last().ToLower() + "_networkregistry_Win32";
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
                    break;
                }
                case "Blueprint":
                {
                    BundleEntry newBundle = App.AssetManager.AddBundle("win32/" + bundlePath.ToLower(), BundleType.SubLevel, App.AssetManager.GetSuperBundleId(superBundleName));
                    if (!generateBlueprints) return;
                    
                    EbxAsset bpb = new EbxAsset(TypeLibrary.CreateObject(blueprintType));
                    bpb.SetFileGuid(Guid.NewGuid());

                    dynamic bpbRootObject = bpb.RootObject;
                    bpbRootObject.Name = bundlePath;

                    AssetClassGuid bpbguid = new AssetClassGuid(Utils.GenerateDeterministicGuid(bpb.Objects, (Type)bpbRootObject.GetType(), bpb.FileGuid), -1);
                    bpbRootObject.SetInstanceGuid(bpbguid);

                    EbxAssetEntry newSubLevelEntry = App.AssetManager.AddEbx(bundlePath, bpb);

                    newSubLevelEntry.AddedBundles.Add(App.AssetManager.GetBundleId(newBundle));
                    newSubLevelEntry.ModifiedEntry.DependentAssets.AddRange(bpb.Dependencies);
                    newBundle.Blueprint = newSubLevelEntry;
                    
                    for (int i = 0; i != 2; i++)
                    {
                        EbxAsset blueprint;
                        string name;
                        if (i == 1)
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("MeshVariationDatabase"));
                            name = bundlePath + "/MeshVariationDb_Win32";
                        }
                        else
                        {
                            blueprint = new EbxAsset(TypeLibrary.CreateObject("NetworkRegistryAsset"));
                            name = bundlePath.Replace(bundlePath.Split().Last(), "") + bundlePath.Split().Last().ToLower() + "_networkregistry_Win32";
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
                    break;
                }                
            }
        }

        #endregion

        #region Logging

        public static void Log(string stringToLog)
        {
            App.Logger.Log(stringToLog);
        }

        #endregion

        #region Getting Assets

        public static List<Asset> GetAllOfType(string type, bool onlyModified = false, bool onlyAdded = false)
        {
            List<Asset> assets = new List<Asset>();
            foreach (EbxAssetEntry assetEntry in App.AssetManager.EnumerateEbx(type, onlyModified))
            {
                if (onlyAdded && !assetEntry.IsAdded) continue;
                assets.Add((Asset)assetEntry);
            }

            return assets;
        }
        
        public static List<Asset> GetAllInBundle(Bundle bundle, bool onlyModified = false, bool onlyAdded = false)
        {
            List<Asset> assets = new List<Asset>();
            BundleEntry bundleEntry = App.AssetManager.GetBundleEntry(App.AssetManager.GetBundleId(bundle.Name));
            foreach (EbxAssetEntry assetEntry in App.AssetManager.EnumerateEbx(bundleEntry))
            {
                if ((onlyAdded && !assetEntry.IsAdded) || (onlyModified && !assetEntry.IsModified)) continue;
                assets.Add((Asset)assetEntry);
            }

            return assets;
        }

        #endregion
        
        #endregion

        #region Properties

        public static Asset SelectedAsset
        {
            get
            {
                Asset asset = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    asset = (Asset)App.EditorWindow.DataExplorer.SelectedAsset;
                });
                return asset;
            }

            set
            {
                if (App.AssetManager.GetEbxEntry(value.FilePath) != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.EditorWindow.DataExplorer.SelectedAsset = App.AssetManager.GetEbxEntry(value.FilePath);
                    });
                }
                else
                {
                    App.Logger.LogError("{0} is not a valid asset. Did you forget to set the FilePath?", value.FilePath);
                }
            }
        }

        public static Bundle SelectedBundle
        {
            get => (Bundle)BundleEditorWindow.CurrentBundle;
        }

        #endregion
        
        #endregion

    }

    #endregion
}
