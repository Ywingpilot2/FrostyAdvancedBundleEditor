using BundleEditPlugin;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
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
                BunOpName = ofd.FileName.Replace(@"\\", "/").Split('/').Last();
                BunOpPath = ofd.FileName;
                FrostyTaskWindow.Show(BunOpName, "", (task) =>
                {
                    ReadBundle(task);
                });
            }
            return this;
        }

        /// <summary>
        /// The brains behind the operation
        /// </summary>
        private void ReadBundle(FrostyTaskWindow task)
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
                else if (commentposition == 0)
                {
                    line = sr.ReadLine();
                    continue;
                }

                if (line.StartsWith("["))
                {
                    switch (line.Replace("[", "").Replace("]", "").Split('=').First())
                    {
                        #region --Parse Bools--
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
                        case "FollowUserPreferences ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.FollowUserPreferences = value;
                                break;
                            }
                        case "IgnoreTypes ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.IgnoreTypes = value;
                                break;
                            }
                        case "UseExclusiveTypes ":
                            {
                                Boolean.TryParse(line.Split('=').Last().TrimStart(' ').TrimEnd(']'), out bool value);
                                opProperties.UseExclusiveTypes = value;
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
                                    values.Add(App.AssetManager.GetBundleId(bundle));
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
                                    values.Add(App.AssetManager.GetEbxEntry(asset).Guid);
                                }
                                opProperties.Assets = values;
                                break;
                            }
                        case "IgnoredTypes ":
                            {
                                opProperties.IgnoredTypes = line.Split('=').Last().TrimStart(' ').TrimEnd(']').Split(',').ToList();
                                break;
                            }
                        case "ExclusiveTypes ":
                            {
                                opProperties.ExclusiveTypes = line.Split('=').Last().TrimStart(' ').TrimEnd(']').Split(',').ToList();
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
            sr.Close(); //Close the file once we are done
        }
    }

    public class BunOpProperties
    {
        #region --Bools--
        public bool AddToNetregs = false;
        public bool RemoveFromNetregs = false;
        public bool AddToMeshVariations = false;
        public bool RemoveFromMeshVariations = false;
        public bool IsRecursive = false;
        public bool FollowUserPreferences = true;
        public bool IgnoreTypes = true;
        public bool UseExclusiveTypes = false;
        public bool ForceAdd = false;
        #endregion

        #region --Lists--
        public List<int> Bundles = new List<int>();
        public List<Guid> Assets = new List<Guid>();
        public List<string> IgnoredTypes = new List<string>() { "ShaderGraph" }; //Relies on IgnoreTypes
        public List<string> ExclusiveTypes = new List<string>(); //Relies on UseExclusiveTypes
        #endregion

        public void TriggerInstruction(string InstructionName, FrostyTaskWindow task)
        {
            switch (InstructionName)
            {
                case "AddAssetsToBundle":
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
                                if ((BundleEditors.AssetRecAddValid(currentAsset, bundle) || ForceAdd) && !bundledAssets.Contains(currentAsset.Guid))
                                {
                                    if ((IgnoreTypes && !IgnoredTypes.Contains(currentAsset.Type)) || (UseExclusiveTypes && ExclusiveTypes.Contains(currentAsset.Type)) || (!IgnoreTypes && !UseExclusiveTypes))
                                    {
                                        BundleEditors.AddAssetToBundle(currentAsset, bundle);
                                        if (AddToNetregs && BundleEditors.AssetAddNetworkValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToNetRegs(currentAsset, bundle);
                                        }
                                        else if (AddToMeshVariations && BundleEditors.AssetAddMeshVariationValid(currentAsset, bundle))
                                        {
                                            BundleEditors.AddAssetToMVDBs(currentAsset, bundle, task);
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
                default:
                    {
                        App.Logger.LogError("{0} is not a valid instruction", InstructionName);
                        break;
                    }
            }
        }
    }
}
