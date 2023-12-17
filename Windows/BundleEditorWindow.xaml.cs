using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk.Managers;

namespace AdvancedBundleEditorPlugin.Windows
{
    public partial class BundleEditorWindow : UserControl
    {
        public static BundleEntry CurrentBundle;
        public BundleEditorWindow()
        {
            InitializeComponent();
            foreach (BundleEntry bundle in App.AssetManager.EnumerateBundles())
            {
                switch (bundle.Type)
                {
                    case BundleType.SharedBundle:
                    {
                        SharedBundlesList.Items.Add(bundle);
                    } break;
                    case BundleType.SubLevel:
                    {
                        SublevelBundlesList.Items.Add(bundle);
                    } break;
                    case BundleType.BlueprintBundle:
                    {
                        BlueprintBundlesList.Items.Add(bundle);
                    } break;
                    default:
                    {
                        SharedBundlesList.Items.Add(bundle);
                    } break;
                }
            }

            SharedFilterBox.KeyUp += BundleFilterBox_KeyUp;
            LevelFilterBox.KeyUp += BundleFilterBox_KeyUp;
            BlueprintFilterBox.KeyUp += BundleFilterBox_KeyUp;

            SharedFilterBox.LostFocus += (sender, args) =>
            {
                BundleListFilter(SharedBundlesList, SharedFilterBox.Text);
            };
            LevelFilterBox.LostFocus += (sender, args) =>
            {
                BundleListFilter(SublevelBundlesList, LevelFilterBox.Text);
            };
            BlueprintFilterBox.LostFocus += (sender, args) =>
            {
                BundleListFilter(BlueprintBundlesList, BlueprintFilterBox.Text);
            };

            SharedBundlesList.SelectionChanged += (sender, args) =>
            {
                CurrentBundle = SharedBundlesList.SelectedItem as BundleEntry;
                RefreshEbxExplorer();
                RefreshResExplorer();
            };
            SublevelBundlesList.SelectionChanged += (sender, args) =>
            {
                CurrentBundle = SublevelBundlesList.SelectedItem as BundleEntry;
                RefreshEbxExplorer();
                RefreshResExplorer();
            };
            BlueprintBundlesList.SelectionChanged += (sender, args) =>
            {
                CurrentBundle = BlueprintBundlesList.SelectedItem as BundleEntry;
                RefreshEbxExplorer();
                RefreshResExplorer();
            };

            AssetDataExplorer.SelectedAssetDoubleClick += (sender, args) =>
            {
                EbxAssetEntry entry = AssetDataExplorer.SelectedAsset as EbxAssetEntry;
                App.EditorWindow.OpenAsset(entry);
            };
        }

        #region Assets Preview

        private void RefreshEbxExplorer()
        {
            if (CurrentBundle == null) return;

            AssetDataExplorer.ItemsSource = App.AssetManager.EnumerateEbx(CurrentBundle);
        }
        
        private void RefreshResExplorer()
        {
            if (CurrentBundle == null) return;

            ResDataExplorer.ItemsSource = App.AssetManager.EnumerateRes(CurrentBundle);
        }

        #endregion

        #region Filtering

        private void BundleFilterBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FrostyWatermarkTextBox textBox = sender as FrostyWatermarkTextBox;
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void BundleListFilter(ItemsControl list, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                list.Items.Filter = null;
            }
            else
            {
                list.Items.Filter = (a) => ((BundleEntry)a).Name.IndexOf(filterText.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        #endregion

        #region Buttons

        #region Add & Remove

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
            if (entry == null || CurrentBundle == null) return;

            //Lazy way to support multi select and also not needing PluginM
            List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
            if (App.EditorWindow.DataExplorer.SelectedAssets != null)
            {
                selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
            }

            FrostyTaskWindow.Show("Adding asset to bundle", "", (task) =>
            {
                foreach (EbxAssetEntry selectedAsset in selectedAssets) 
                {
                    //Check to see if this bundle already has our asset
                    if (!selectedAsset.Bundles.Contains(App.AssetManager.GetBundleId(CurrentBundle)) && !selectedAsset.AddedBundles.Contains(App.AssetManager.GetBundleId(CurrentBundle)))
                    {
                        BundleEditors.AddAssetToBundle(selectedAsset, CurrentBundle);

                        //We also need to check if its networked
                        if (BundleEditors.AssetAddNetworkValid(selectedAsset, CurrentBundle))
                        {
                            BundleEditors.AddAssetToNetRegs(selectedAsset, CurrentBundle);
                        }
                        //If not, check if its a mesh object
                        else if (BundleEditors.AssetAddMeshVariationValid(selectedAsset, CurrentBundle))
                        {
                            BundleEditors.AddAssetToMvdBs(selectedAsset, CurrentBundle, task);
                        }
                    }

                    else
                    {
                        App.Logger.LogError("Asset is already in {0}", CurrentBundle.Name);
                    }
                }
            });

            RefreshEbxExplorer();
            App.EditorWindow.DataExplorer.RefreshItems();

            AssetDataExplorer.SelectAsset(entry);
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
            if (entry == null || CurrentBundle == null) return;
            
            //Lazy way to support multi select and also not needing PluginM
            List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
            if (App.EditorWindow.DataExplorer.SelectedAssets != null)
            {
                selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
            }

            FrostyTaskWindow.Show("Removing asset from bundle", "", (task) =>
            {
                foreach (EbxAssetEntry assetEntry in selectedAssets)
                {
                    if (assetEntry.AddedBundles.Contains(App.AssetManager.GetBundleId(CurrentBundle)))
                    {
                        BundleEditors.RemoveAssetFromBundle(assetEntry, CurrentBundle);

                        if (BundleEditors.AssetRemNetworkValid(assetEntry, CurrentBundle))
                        {
                            BundleEditors.RemoveAssetFromNetRegs(assetEntry, CurrentBundle);
                        }
                        else if (BundleEditors.AssetRemMeshVariationValid(assetEntry, CurrentBundle))
                        {
                            BundleEditors.RemoveAssetFromMeshVariations(assetEntry, CurrentBundle);
                        }
                    }

                    else
                    {
                        App.Logger.LogError("{0} cannot be removed from this asset, are you sure its an added bundle?", CurrentBundle.Name);
                    }
                }
            });

            RefreshEbxExplorer();
            App.EditorWindow.DataExplorer.RefreshItems();
        }

        #endregion

        #region Rec Add & Remove

        private void RecAddButton_OnClick(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
            if (entry == null || CurrentBundle == null) return;
            
            //Lazy way to support multi select and also not needing PluginM
            List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
            if (App.EditorWindow.DataExplorer.SelectedAssets != null)
            {
                selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
            }

            FrostyTaskWindow.Show("Recursively adding to bundles", "Initiating...", (task) =>
            {
                //We keep track of our stats for the user
                int foundCount = 0;
                int addedCount = 0;
                int errorCount = 0;

                EbxAssetEntry assetToCheck = null; //This is the asset we are currently checking
                List<Guid> assetsToCheck = new List<Guid>(); //The list of assets we need to check. As we are searching through files this gets added to
                List<Guid> addedAssets = new List<Guid>(); //Lazy hack to make sure we don't check an asset twice
                foreach (EbxAssetEntry selectedAsset in selectedAssets)
                {
                    //We need to add our selected asset and its references to the list of things for us to check
                    assetsToCheck.Add(selectedAsset.Guid);
                    assetsToCheck.AddRange(selectedAsset.EnumerateDependencies());
                    while (assetsToCheck.Count != 0) //When this reaches 0, we have exhausted all possible file paths and we are done
                    {
                        foundCount++;
                        if (!addedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                        {
                            assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked
                            task.Update($"Adding {assetToCheck.DisplayName} to Bundle");

                            if (BundleEditors.AssetRecAddValid(assetToCheck, CurrentBundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                            {
                                addedCount++;
                                //If it is, add it to bundles and netregs
                                BundleEditors.AddAssetToBundle(assetToCheck, CurrentBundle);

                                if (BundleEditors.AssetAddNetworkValid(assetToCheck, CurrentBundle))
                                {
                                    task.Update($"Adding {assetToCheck.DisplayName} to Net Registry");
                                    BundleEditors.AddAssetToNetRegs(assetToCheck, CurrentBundle);
                                }
                                else if (BundleEditors.AssetAddMeshVariationValid(assetToCheck, CurrentBundle))
                                {
                                    BundleEditors.AddAssetToMvdBs(assetToCheck, CurrentBundle, task);
                                }
                            } 
                            else if (assetToCheck.Type == "ShaderGraph")
                            {
                                App.Logger.LogError("Couldn't add {0}", assetToCheck.Name);
                                errorCount++;
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

                App.Logger.Log("I have added {0} assets out of {1} found referenced. With {2} not being added due to an error.", addedCount.ToString(), foundCount.ToString(), errorCount.ToString());
            });

            RefreshEbxExplorer();
            App.EditorWindow.DataExplorer.RefreshItems();
        }

        private void RecRemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
            if (entry == null || CurrentBundle == null) return;
            
            List<AssetEntry> SelectedAssets = new List<AssetEntry> { entry };
            if (App.EditorWindow.DataExplorer.SelectedAssets != null)
            {
                SelectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList();
            }

            FrostyTaskWindow.Show("Recursively adding to bundles", "Initiating...", (task) =>
            {
                //Keep track of our stats
                int foundCount = 0;
                int addedCount = 0;
                int errorCount = 0;

                EbxAssetEntry assetToCheck = null; //This is the asset we are currently checking
                List<Guid> assetsToCheck = new List<Guid>(); //The list of assets we need to check. As we are searching through files this gets added to
                List<Guid> addedAssets = new List<Guid>(); //Lazy hack to make sure we don't check an asset twice
                foreach (EbxAssetEntry SelectedAsset in SelectedAssets)
                {
                    //We need to add our selected asset and its references to the list of things for us to check
                    assetsToCheck.Add(SelectedAsset.Guid);
                    assetsToCheck.AddRange(SelectedAsset.EnumerateDependencies());
                    while (assetsToCheck.Count != 0)
                    {
                        foundCount++;
                        if (!addedAssets.Contains(assetsToCheck[0])) //If this is not an asset we have already checked/added
                        {
                            assetToCheck = App.AssetManager.GetEbxEntry(assetsToCheck[0]); //Get what is next in line to be checked in
                            task.Update($"Removing {assetToCheck.DisplayName} from Bundle");

                            if (BundleEditors.AssetRecRemValid(assetToCheck, CurrentBundle) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                            {
                                addedCount++;
                                //If it is, add it to bundles and netregs
                                BundleEditors.RemoveAssetFromBundle(assetToCheck, CurrentBundle);

                                if (BundleEditors.AssetRemNetworkValid(assetToCheck, CurrentBundle))
                                {
                                    task.Update($"Removing {assetToCheck.DisplayName} from Net Registry");
                                    BundleEditors.RemoveAssetFromNetRegs(assetToCheck, CurrentBundle);
                                }
                                else if (BundleEditors.AssetRemMeshVariationValid(assetToCheck, CurrentBundle))
                                {
                                    task.Update($"Removing {assetToCheck.DisplayName} from Mesh Variation");
                                    BundleEditors.RemoveAssetFromMeshVariations(assetToCheck, CurrentBundle);
                                }
                            }
                            //We should probably create an unsupported list instead, but 
                            else if (assetToCheck.Type == "ShaderGraph" || !assetToCheck.AddedBundles.Contains(App.AssetManager.GetBundleId(CurrentBundle)))
                            {
                                App.Logger.LogError("Couldn't add {0}", assetToCheck.Name);
                                errorCount++;
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

                App.Logger.Log("I have removed {0} assets out of {1} found referenced. With {2} not being removed due to an error.", addedCount.ToString(), foundCount.ToString(), errorCount.ToString());
            });

            RefreshEbxExplorer();
            App.EditorWindow.DataExplorer.RefreshItems();
        }

        #endregion

        #region Bundles

        private void AddBundleButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddBundleArgs args = new AddBundleArgs();
            var result = BundleEditorPromptWindow.Show(args, "Add Bundle");
            if (result != MessageBoxResult.OK) return;

            App.AssetManager.AddBundle(args.Name, args.Type, App.AssetManager.GetSuperBundleId(args.SuperBundle));
        }

        #endregion

        #region Bundle Operatiions

        private void BunOpButton_OnClick(object sender, RoutedEventArgs e)
        {
            BundleOperator bundleOperator = new BundleOperator();
            bundleOperator.InitiateBundleOperation();
        }

        #endregion

        #endregion
    }
    
    public class AddBundleArgs
    {
        public string Name { get; set; } = "";
        public string SuperBundle { get; set; } = "";
        public BundleType Type { get; set; }
    }
}