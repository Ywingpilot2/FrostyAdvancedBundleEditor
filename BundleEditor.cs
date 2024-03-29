﻿using AtlasTexturePlugin;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Viewport;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using MeshSetPlugin.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RootInstanceEntiresPlugin;
using System.Security.Cryptography;

namespace AdvancedBundleEditorPlugin
{
    #region --Remove From Bundle extensions--

    public class RemovePathfindingExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "PathfindingBlobAsset";

        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset Pathfindingasset = App.AssetManager.GetEbx(entry);
            dynamic Pathfindingobject = Pathfindingasset.RootObject;

            foreach (var Blob in Pathfindingobject.Blobs)
            {
                ChunkAssetEntry ChunkEntry = App.AssetManager.GetChunkEntry(Blob.BlobId);
                ChunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
                entry.LinkAsset(ChunkEntry);
            }
        }
    }

    public class RemoveAtlasTexureExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "AtlasTextureAsset";
        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic textureAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(textureAsset.Resource);
            resEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));

            AtlasTexture texture = App.AssetManager.GetResAs<AtlasTexture>(resEntry);
            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(texture.ChunkId);

            chunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));

            resEntry.LinkAsset(chunkEntry);
            entry.LinkAsset(resEntry);
        }
    }

    public class RemoveMeshExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "MeshAsset";
        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic meshAsset = asset.RootObject;

            //Add res to BUNDLES AND LINK
            ResAssetEntry resEntry = App.AssetManager.GetResEntry(meshAsset.MeshSetResource);
            resEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
            entry.LinkAsset(resEntry);

            MeshSet meshSetRes = App.AssetManager.GetResAs<MeshSet>(resEntry);

            //Double check if there are any LODs the mesh, if there are, bundle and link them
            if (meshSetRes.Lods.Count > 0)
            {
                foreach (MeshSetLod lod in meshSetRes.Lods)
                {
                    if (lod.ChunkId != Guid.Empty)
                    {
                        ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(lod.ChunkId);
                        chunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
                        resEntry.LinkAsset(chunkEntry);
                    }
                }
            }

            //SWBF2 has a fancy setup with ShaderBlockDepots, we need to bundle those too
            if (ProfilesLibrary.IsLoaded(ProfileVersion.StarWarsBattlefrontII))
            {
                ResAssetEntry block = App.AssetManager.GetResEntry(entry.Name.ToLower() + "_mesh/blocks");
                block.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
            }

            base.RemoveFromBundle(entry, bentry);
        }
    }

    public class RemoveSvgImageExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "SvgImage";
        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic svgAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(svgAsset.Resource);
            resEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));

            entry.LinkAsset(resEntry);
        }
    }

    public class RemoveTextureExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "TextureBaseAsset";
        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic textureAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(textureAsset.Resource);
            resEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));

            Texture texture = App.AssetManager.GetResAs<Texture>(resEntry);
            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(texture.ChunkId);

            chunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
            chunkEntry.FirstMip = texture.FirstMip;

            resEntry.LinkAsset(chunkEntry);
            entry.LinkAsset(resEntry);
        }
    }

    public class RemoveMovieTextureExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "MovieTextureBaseAsset";

        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset movieasset = App.AssetManager.GetEbx(entry);
            dynamic movieobject = movieasset.RootObject;

            ChunkAssetEntry MovieChunkEntry = App.AssetManager.GetChunkEntry(movieobject.ChunkGuid);
            MovieChunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
            entry.LinkAsset(MovieChunkEntry);

            ChunkAssetEntry SubtitleChunkEntry = App.AssetManager.GetChunkEntry(movieobject.SubtitleChunkGuid);
            if (SubtitleChunkEntry != null)
            {
                SubtitleChunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
                entry.LinkAsset(SubtitleChunkEntry);
            }
        }
    }

    public class RemoveSoundWaveExtension : RemoveFromBundleExtension
    {
        public override string AssetType => "SoundWaveAsset";

        public override void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.RemoveFromBundle(entry, bentry);

            EbxAsset soundasset = App.AssetManager.GetEbx(entry);
            dynamic soundobject = soundasset.RootObject;

            foreach (var soundChunk in soundobject.Chunks)
            {
                ChunkAssetEntry ChunkEntry = App.AssetManager.GetChunkEntry(soundChunk.ChunkId);
                ChunkEntry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
                entry.LinkAsset(ChunkEntry);
            }
        }
    }

    public class RemoveFromBundleExtension
    {
        public virtual string AssetType => null;
        public virtual void RemoveFromBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            entry.AddedBundles.Remove(App.AssetManager.GetBundleId(bentry));
        }
    }

    #endregion

    #region --Add to bundle extensions

    public class PathfindingExtension : AddToBundleExtension
    {
        public override string AssetType => "PathfindingBlobAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic BlobAsset = asset.RootObject;

            foreach (var Blob in BlobAsset.Blobs)
            {
                ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(Blob.BlobId);
                chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
                using (NativeReader nativeReader = new NativeReader(App.AssetManager.GetChunk(chunkEntry)))
                {
                    App.AssetManager.ModifyChunk(chunkEntry.Id, nativeReader.ReadToEnd());
                }
            }
        }
    }

    public class AtlasTexureExtension : AddToBundleExtension
    {
        public override string AssetType => "AtlasTextureAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic textureAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(textureAsset.Resource);
            resEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));

            AtlasTexture texture = App.AssetManager.GetResAs<AtlasTexture>(resEntry);
            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(texture.ChunkId);

            chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));

            resEntry.LinkAsset(chunkEntry);
            entry.LinkAsset(resEntry);
        }
    }

    public class MeshExtension : AddToBundleExtension
    {
        public override string AssetType => "MeshAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic meshAsset = asset.RootObject;

            //Add res to BUNDLES AND LINK
            ResAssetEntry resEntry = App.AssetManager.GetResEntry(meshAsset.MeshSetResource);
            resEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
            entry.LinkAsset(resEntry);

            MeshSet meshSetRes = App.AssetManager.GetResAs<MeshSet>(resEntry);
            App.AssetManager.ModifyRes(resEntry.ResRid, meshSetRes);
            //Double check if there are any LODs in the Rigid Mesh, if there are, bundle and link them. Else, just bundle the EBX and move on.
            if (meshSetRes.Lods.Count > 0)
            {
                foreach (MeshSetLod lod in meshSetRes.Lods)
                {
                    if (lod.ChunkId != Guid.Empty)
                    {
                        ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(lod.ChunkId);
                        chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
                        resEntry.LinkAsset(chunkEntry);
                    }
                }
            }

            //SWBF2 has a fancy setup with SBDs, we need to bundle those too
            if (ProfilesLibrary.IsLoaded(ProfileVersion.StarWarsBattlefrontII))
            {
                ResAssetEntry block = App.AssetManager.GetResEntry(entry.Name.ToLower() + "_mesh/blocks");
                block.AddToBundle(App.AssetManager.GetBundleId(bentry));
            }

            base.AddToBundle(entry, bentry);
        }
    }

    public class SvgImageExtension : AddToBundleExtension
    {
        public override string AssetType => "SvgImage";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic svgAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(svgAsset.Resource);
            resEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));

            entry.LinkAsset(resEntry);
        }
    }

    public class TextureExtension : AddToBundleExtension
    {
        public override string AssetType => "TextureBaseAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic textureAsset = asset.RootObject;

            ResAssetEntry resEntry = App.AssetManager.GetResEntry(textureAsset.Resource);
            resEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));

            Texture texture = App.AssetManager.GetResAs<Texture>(resEntry);
            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(texture.ChunkId);

            chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
            chunkEntry.FirstMip = texture.FirstMip;

            resEntry.LinkAsset(chunkEntry);
            entry.LinkAsset(resEntry);
        }
    }

    public class MovieTextureExtension : AddToBundleExtension
    {
        public override string AssetType => "MovieTextureBaseAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic movieAsset = asset.RootObject;

            ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(movieAsset.ChunkGuid);
            chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
            entry.LinkAsset(chunkEntry);

            chunkEntry = App.AssetManager.GetChunkEntry(movieAsset.SubtitleChunkGuid);
            if (chunkEntry != null)
            {
                chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
                entry.LinkAsset(chunkEntry);
            }
        }
    }

    public class SoundWaveExtension : AddToBundleExtension
    {
        public override string AssetType => "SoundWaveAsset";
        public override void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            base.AddToBundle(entry, bentry);

            EbxAsset asset = App.AssetManager.GetEbx(entry);
            dynamic soundAsset = asset.RootObject;

            foreach (var soundDataChunk in soundAsset.Chunks)
            {
                ChunkAssetEntry chunkEntry = App.AssetManager.GetChunkEntry(soundDataChunk.ChunkId);
                chunkEntry.AddToBundle(App.AssetManager.GetBundleId(bentry));
                entry.LinkAsset(chunkEntry);
            }
        }
    }

    public class AddToBundleExtension
    {
        public virtual string AssetType => null;
        public virtual void AddToBundle(EbxAssetEntry entry, BundleEntry bentry)
        {
            entry.AddToBundle(App.AssetManager.GetBundleId(bentry));
        }
    }

    #endregion

    // [TemplatePart(Name = PART_BundleTypeComboBox, Type = typeof(ComboBox))]
    // [TemplatePart(Name = PART_BundlesListBox, Type = typeof(ListBox))]
    // [TemplatePart(Name = PART_DataExplorer, Type = typeof(FrostyDataExplorer))]
    // [TemplatePart(Name = PART_SuperBundleTextBox, Type = typeof(TextBox))]
    // [TemplatePart(Name = PART_BundleFilterTextBox, Type = typeof(TextBox))]
    public class BundleEditor : FrostyBaseEditor
    {
        public override ImageSource Icon => BundleEditorMenuExtension.iconImageSource;
        
        #region Old

        /*private const string PART_BundleTypeComboBox = "PART_BundleTypeComboBox";
        private const string PART_BundlesListBox = "PART_BundlesListBox";
        private const string PART_DataExplorer = "PART_DataExplorer";
        private const string PART_SuperBundleTextBox = "PART_SuperBundleTextBox";
        private const string PART_BundleFilterTextBox = "PART_BundleFilterTextBox";

        public override ImageSource Icon => BundleEditorMenuExtension.iconImageSource;
        public RelayCommand AddToBundleCommand { get; }
        public RelayCommand RemoveFromBundleCommand { get; }
        public RelayCommand RecAddBundleCommand { get; }
        public RelayCommand RecRemBundleCommand { get; }

        private ComboBox bundleTypeComboBox;
        private ListBox bundlesListBox;
        private FrostyDataExplorer dataExplorer;
        private TextBox superBundleTextBox;
        private TextBox bundleFilterTextBox;

        private BundleType selectedBundleType = BundleType.SharedBundle;

        static BundleEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BundleEditor), new FrameworkPropertyMetadata(typeof(BundleEditor)));
        }

        public BundleEditor()
        {

            #region --Bundle Editing Commands--
            AddToBundleCommand = new RelayCommand(
                (o) =>
                {
                    EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;

                    //Lazy way to support multi select and also not needing PluginM
                    List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
                    if (App.EditorWindow.DataExplorer.SelectedAssets != null)
                    {
                        selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
                    }
                    BundleEntry bentry = bundlesListBox.SelectedItem as BundleEntry;

                    FrostyTaskWindow.Show("Adding asset to bundle", "", (task) =>
                    {
                        foreach (EbxAssetEntry selectedAsset in selectedAssets) 
                        {
                            //Check to see if this bundle already has our asset
                            if (!selectedAsset.Bundles.Contains(App.AssetManager.GetBundleId(bentry)) && !selectedAsset.AddedBundles.Contains(App.AssetManager.GetBundleId(bentry)))
                            {
                                BundleEditors.AddAssetToBundle(selectedAsset, bentry);

                                //We also need to check if its networked
                                if (BundleEditors.AssetAddNetworkValid(selectedAsset, bentry))
                                {
                                    BundleEditors.AddAssetToNetRegs(selectedAsset, bentry);
                                }
                                //If not, check if its a mesh object
                                else if (BundleEditors.AssetAddMeshVariationValid(selectedAsset, bentry))
                                {
                                    BundleEditors.AddAssetToMvdBs(selectedAsset, bentry, task);
                                }
                            }

                            else
                            {
                                App.Logger.LogError("Asset is already in {0}", bentry.Name);
                            }
                        }
                    });

                    RefreshExplorer();
                    App.EditorWindow.DataExplorer.RefreshItems();

                    dataExplorer.SelectAsset(entry);
                },
                (o) =>
                {
                    return App.EditorWindow.DataExplorer.SelectedAsset != null && bundlesListBox.SelectedItem != null;
                });

            RemoveFromBundleCommand = new RelayCommand(
                (o) =>
                {
                    EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
                    //Lazy way to support multi select and also not needing PluginM
                    List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
                    if (App.EditorWindow.DataExplorer.SelectedAssets != null)
                    {
                        selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
                    }
                    BundleEntry bentry = bundlesListBox.SelectedItem as BundleEntry;

                    FrostyTaskWindow.Show("Removing asset from bundle", "", (task) =>
                    {
                        foreach (EbxAssetEntry assetEntry in selectedAssets)
                        {
                            if (assetEntry.AddedBundles.Contains(App.AssetManager.GetBundleId(bentry)))
                            {
                                BundleEditors.RemoveAssetFromBundle(assetEntry, bentry);

                                if (BundleEditors.AssetRemNetworkValid(assetEntry, bentry))
                                {
                                    BundleEditors.RemoveAssetFromNetRegs(assetEntry, bentry);
                                }
                                else if (BundleEditors.AssetRemMeshVariationValid(assetEntry, bentry))
                                {
                                    BundleEditors.RemoveAssetFromMeshVariations(assetEntry, bentry);
                                }
                            }

                            else
                            {
                                App.Logger.LogError("{0} cannot be removed from this asset, are you sure its an added bundle?", bentry.Name);
                            }
                        }
                    });

                    RefreshExplorer();
                    App.EditorWindow.DataExplorer.RefreshItems();
                },
                (o) =>
                {
                    return App.EditorWindow.DataExplorer.SelectedAsset != null && bundlesListBox.SelectedItem != null;
                });

            RecAddBundleCommand = new RelayCommand(
                (o) =>
                {
                    EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
                    //Lazy way to support multi select and also not needing PluginM
                    List<AssetEntry> selectedAssets = new List<AssetEntry> { entry }; //If we only have 1 asset selected we just want 1 item in the list
                    if (App.EditorWindow.DataExplorer.SelectedAssets != null)
                    {
                        selectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList(); //This means we have PluginM installed, meaning many assets can be here
                    }
                    BundleEntry bentry = bundlesListBox.SelectedItem as BundleEntry;

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

                                    if (BundleEditors.AssetRecAddValid(assetToCheck, bentry) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                                    {
                                        addedCount++;
                                        //If it is, add it to bundles and netregs
                                        BundleEditors.AddAssetToBundle(assetToCheck, bentry);

                                        if (BundleEditors.AssetAddNetworkValid(assetToCheck, bentry))
                                        {
                                            task.Update($"Adding {assetToCheck.DisplayName} to Net Registry");
                                            BundleEditors.AddAssetToNetRegs(assetToCheck, bentry);
                                        }
                                        else if (BundleEditors.AssetAddMeshVariationValid(assetToCheck, bentry))
                                        {
                                            BundleEditors.AddAssetToMvdBs(assetToCheck, bentry, task);
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

                    RefreshExplorer();
                    App.EditorWindow.DataExplorer.RefreshItems();
                },
                (o) =>
                {
                    return App.EditorWindow.DataExplorer.SelectedAsset != null && bundlesListBox.SelectedItem != null;
                });

            RecRemBundleCommand = new RelayCommand(
                (o) =>
                {
                    EbxAssetEntry entry = App.EditorWindow.DataExplorer.SelectedAsset as EbxAssetEntry;
                    List<AssetEntry> SelectedAssets = new List<AssetEntry> { entry };
                    if (App.EditorWindow.DataExplorer.SelectedAssets != null)
                    {
                        SelectedAssets = App.EditorWindow.DataExplorer.SelectedAssets.ToList();
                    }
                    BundleEntry bentry = bundlesListBox.SelectedItem as BundleEntry;

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

                                    if (BundleEditors.AssetRecRemValid(assetToCheck, bentry) && assetToCheck.Type != "ShaderGraph") //Now check if its valid
                                    {
                                        addedCount++;
                                        //If it is, add it to bundles and netregs
                                        BundleEditors.RemoveAssetFromBundle(assetToCheck, bentry);

                                        if (BundleEditors.AssetRemNetworkValid(assetToCheck, bentry))
                                        {
                                            task.Update($"Removing {assetToCheck.DisplayName} from Net Registry");
                                            BundleEditors.RemoveAssetFromNetRegs(assetToCheck, bentry);
                                        }
                                        else if (BundleEditors.AssetRemMeshVariationValid(assetToCheck, bentry))
                                        {
                                            task.Update($"Removing {assetToCheck.DisplayName} from Mesh Variation");
                                            BundleEditors.RemoveAssetFromMeshVariations(assetToCheck, bentry);
                                        }
                                    }
                                    //We should probably create an unsupported list instead, but 
                                    else if (assetToCheck.Type == "ShaderGraph" || !assetToCheck.AddedBundles.Contains(App.AssetManager.GetBundleId(bentry)))
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

                    RefreshExplorer();
                    App.EditorWindow.DataExplorer.RefreshItems();
                },
                (o) =>
                {
                    return App.EditorWindow.DataExplorer.SelectedAsset != null && bundlesListBox.SelectedItem != null;
                });
            #endregion
        }

        #region --UI stuff--

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            bundleTypeComboBox = GetTemplateChild(PART_BundleTypeComboBox) as ComboBox;
            bundlesListBox = GetTemplateChild(PART_BundlesListBox) as ListBox;
            dataExplorer = GetTemplateChild(PART_DataExplorer) as FrostyDataExplorer;
            superBundleTextBox = GetTemplateChild(PART_SuperBundleTextBox) as TextBox;
            bundleFilterTextBox = GetTemplateChild(PART_BundleFilterTextBox) as TextBox;

            bundleTypeComboBox.SelectionChanged += bundleTypeComboBox_SelectionChanged;
            bundlesListBox.SelectionChanged += bundlesListBox_SelectionChanged;
            dataExplorer.SelectedAssetDoubleClick += dataExplorer_SelectedAssetDoubleClick;

            bundleFilterTextBox.KeyUp += BundleFilterTextBox_KeyUp;
            bundleFilterTextBox.LostFocus += BundleFilterTextBox_LostFocus;

            bundleTypeComboBox.SelectedIndex = 2;
        }

        private void BundleFilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (bundleFilterTextBox.Text == "")
                bundlesListBox.Items.Filter = null;
            else
            {
                string filterText = bundleFilterTextBox.Text.ToLower();
                bundlesListBox.Items.Filter = (object a) => { return ((BundleEntry)a).Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0; };
            }
        }

        private void BundleFilterTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bundleFilterTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void bundlesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshExplorer();
        }

        private void bundleTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bundlesListBox.Items.Filter = null;
            bundleFilterTextBox.Text = "";

            int index = bundleTypeComboBox.SelectedIndex;
            selectedBundleType = (new BundleType[] { BundleType.SubLevel, BundleType.BlueprintBundle, BundleType.SharedBundle })[index];
            RefreshList();
        }

        private void RefreshList()
        {
            bundlesListBox.ItemsSource = App.AssetManager.EnumerateBundles(selectedBundleType);
            bundlesListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("DisplayName", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void RefreshExplorer()
        {
            BundleEntry entry = bundlesListBox.SelectedItem as BundleEntry;
            if (entry == null)
                return;
            dataExplorer.ItemsSource = App.AssetManager.EnumerateEbx(entry);
            superBundleTextBox.Text = App.AssetManager.GetSuperBundle(entry.SuperBundleId).Name;
            if (entry.Type != BundleType.SharedBundle)
                dataExplorer.SelectAsset(entry.Blueprint);
        }

        private void dataExplorer_SelectedAssetDoubleClick(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry entry = dataExplorer.SelectedAsset as EbxAssetEntry;
            App.EditorWindow.OpenAsset(entry);
        }
        #endregion*/

        #endregion
        
        static BundleEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BundleEditor), new FrameworkPropertyMetadata(typeof(BundleEditor)));
        }
    }

    /// <summary>
    /// A static class which contains all of the information and methods on Bundle Editing this game.
    /// </summary>
    public static class BundleEditors
    {
        private static List<string> networkedTypes = new List<string>();
        private static List<string> networkedBundles = new List<string>();
        private static List<EbxAsset> networkRegistries = new List<EbxAsset>();

        private static List<string> mvdbBundles = new List<string>();
        private static List<EbxAsset> mvdbs = new List<EbxAsset>();

        public static bool HasTextureParams = true; //If false that means the game doesn't have texture params
        public static bool HasSurfaceGuidSetup = false; //If true the game has extra fields for setting things like SurfaceShaderId or SurfaceShaderGuid
        public static bool HasMaterialId = false; //Only used by swbf2 afaik, whether or not Materials have a MaterialId field
        public static bool HasUnlockIdTable = false;
        public static bool HasSuperBundles = true;

        public static Dictionary<string, AddToBundleExtension> AddToBundleExtensions = new Dictionary<string, AddToBundleExtension>();
        public static Dictionary<string, RemoveFromBundleExtension> RemoveFromBundleExtensions = new Dictionary<string, RemoveFromBundleExtension>();

        static BundleEditors()
        {
            App.Logger.Log("First time load, need to wake up ABE. This may take a bit...");

            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(AddToBundleExtension)))
                {
                    var extension = (AddToBundleExtension)Activator.CreateInstance(type);
                    AddToBundleExtensions.Add(extension.AssetType, extension);
                }
                else if (type.IsSubclassOf(typeof(RemoveFromBundleExtension)))
                {
                    var extension = (RemoveFromBundleExtension)Activator.CreateInstance(type);
                    RemoveFromBundleExtensions.Add(extension.AssetType, extension);
                }
            }
            AddToBundleExtensions.Add("null", new AddToBundleExtension());
            RemoveFromBundleExtensions.Add("null", new RemoveFromBundleExtension());

            #region --Cache reading--
            //Read the cache file
            StreamReader sr = null;
            string currentLine = "";
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedTypes.txt"))
            {
                sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedTypes.txt");
                currentLine = sr.ReadLine(); //Set the first line as the current line
                while (currentLine != null)
                {
                    networkedTypes.Add(currentLine); //This will add the current line 
                    currentLine = sr.ReadLine(); //This will set the current line to the next line
                }
                sr.Close(); //Close the file once we are done
            }
            else
            {
                App.Logger.LogWarning("Networked Types cache doesn't exist! This will mean the Bundle Editor will not be able to properly network items. Please generate the cache.");
            }

            //Read the cache file
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedBundles.txt"))
            {
                sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_NetworkedBundles.txt");
                currentLine = sr.ReadLine(); //Set the first line as the current line
                while (currentLine != null)
                {
                    networkedBundles.Add(currentLine.Split(',')[0]);
                    networkRegistries.Add(App.AssetManager.GetEbx(currentLine.Split(',')[1]));
                    currentLine = sr.ReadLine(); //This will set the current line to the next line
                }
                sr.Close(); //Close the file once we are done
            }
            else
            {
                App.Logger.LogWarning("Networked Bundles cache doesn't exist! This will mean the Bundle Editor will not be able to properly network items. Please generate the cache.");
            }

            //Read the cache file
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_MvdbBundles.txt"))
            {
                sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"\Caches\" + ProfilesLibrary.ProfileName + "_MvdbBundles.txt");
                currentLine = sr.ReadLine(); //Set the first line as the current line
                while (currentLine != null)
                {
                    mvdbBundles.Add(currentLine.Split(',')[0]);
                    mvdbs.Add(App.AssetManager.GetEbx(currentLine.Split(',')[1]));
                    currentLine = sr.ReadLine(); //This will set the current line to the next line
                }
                sr.Close(); //Close the file once we are done
            }
            else
            {
                App.Logger.LogWarning("Mesh Database cache doesn't exist! This will mean meshes cannot be loaded into new bundles. Please geneerate the cache,");
            }
            #endregion

            #region --MVDB Setup--

            //MVDBs are setup differently across games, these checks will tell the Bundle Editor what to do when handling them
            if (Config.Get("AllowMVDB", false))
            {
                //Texture params
                switch (ProfilesLibrary.DataVersion)
                {
                    case (int)ProfileVersion.NeedForSpeedHeat:
                    case (int)ProfileVersion.PlantsVsZombiesBattleforNeighborville:
                    case (int)ProfileVersion.Fifa20:
                    case (int)ProfileVersion.Battlefield5:
                    case (int)ProfileVersion.Anthem:
                    case (int)ProfileVersion.Battlefield1:
                        {
                            HasTextureParams = false;
                            break;
                        }
                }

                //shader params
                switch (ProfilesLibrary.DataVersion)
                {
                    case (int)ProfileVersion.Anthem:
                    case (int)ProfileVersion.Fifa19:
                    case (int)ProfileVersion.NeedForSpeedHeat:
                    case (int)ProfileVersion.PlantsVsZombiesBattleforNeighborville:
                    case (int)ProfileVersion.Madden20:
                    case (int)ProfileVersion.StarWarsSquadrons:
                    case (int)ProfileVersion.StarWarsBattlefrontII:
                        {
                            HasSurfaceGuidSetup = true;
                            break;
                        }
                }
                switch (ProfilesLibrary.DataVersion)
                {
                    case (int)ProfileVersion.StarWarsBattlefrontII:
                        {
                            HasMaterialId = true;
                            break;
                        }
                }
            }
            #endregion

            #region --LevelData--
            //UnlockIdTables
            switch (ProfilesLibrary.DataVersion)
            {
                case (int)ProfileVersion.Battlefield4:
                case (int)ProfileVersion.Battlefield1:
                case (int)ProfileVersion.MassEffectAndromeda:
                case (int)ProfileVersion.MirrorsEdgeCatalyst:
                case (int)ProfileVersion.NeedForSpeedEdge:
                case (int)ProfileVersion.DragonAgeInquisition:
                case (int)ProfileVersion.StarWarsBattlefront:
                case (int)ProfileVersion.Fifa17:
                case (int)ProfileVersion.PlantsVsZombiesGardenWarfare:
                case (int)ProfileVersion.PlantsVsZombiesGardenWarfare2:
                case (int)ProfileVersion.NeedForSpeed:
                case (int)ProfileVersion.NeedForSpeedRivals:
                case (int)ProfileVersion.NeedForSpeedPayback:
                case (int)ProfileVersion.Battlefield5:
                    {
                        HasUnlockIdTable = true;
                        break;
                    }
            }
            #endregion

            #region --Super Bundles--

            switch (ProfilesLibrary.DataVersion)
            {
                case (int)ProfileVersion.Battlefield5:
                case (int)ProfileVersion.StarWarsSquadrons:
                case (int)ProfileVersion.StarWarsBattlefrontII:
                {
                    HasSuperBundles = false;
                } break;
            }

            #endregion

            App.Logger.Log("Done!");
        }

        #region --Add to Bundle Methods--

        /// <summary>
        /// Adds an asset to a bundle with all associated data(E.G chunks, res, if it has any).
        /// </summary>
        /// <param name="assetToBundle">Asset to add</param>
        /// <param name="selectedBundle">Bundle to add to</param>
        public static void AddAssetToBundle(EbxAssetEntry assetToBundle, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToBundle);
            App.AssetManager.ModifyEbx(assetToBundle.Name, ebx);

            string key = assetToBundle.Type;
            if (!AddToBundleExtensions.ContainsKey(assetToBundle.Type))
            {
                key = "null";
                foreach (string typekey in AddToBundleExtensions.Keys)
                {
                    if (TypeLibrary.IsSubClassOf(assetToBundle.Type, typekey))
                    {
                        key = typekey;
                        break;
                    }
                }
            }
            AddToBundleExtensions[key].AddToBundle(assetToBundle, selectedBundle);
        }

        /// <summary>
        /// Adds an asset to a network registry and UnlockIdTable if it is a subtype of UnlockBaseAsset
        /// </summary>
        /// <param name="assetToBundle"></param>
        /// <param name="selectedBundle"></param>
        public static void AddAssetToNetRegs(EbxAssetEntry assetToBundle, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToBundle);

            if (!selectedBundle.Added)
            {
                EbxAsset netRegEbx = networkRegistries[networkedBundles.IndexOf(selectedBundle.Name)];
                List<PointerRef> objects = ((dynamic)netRegEbx.RootObject).Objects;

                foreach (object obj in ebx.ExportedObjects)
                {
                    PointerRef pointer = new PointerRef(new EbxImportReference()
                        { FileGuid = assetToBundle.Guid, ClassGuid = ((dynamic)obj).GetInstanceGuid().ExportedGuid });
                    if (networkedTypes.Contains(obj.GetType().Name) && !objects.Contains(pointer))
                    {
                        objects.Add(pointer);
                    }
                }

                netRegEbx.AddDependency(assetToBundle.Guid);
                App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(netRegEbx.FileGuid).Name, netRegEbx);
            }
            else
            {
                foreach (EbxAssetEntry networkRegistry in App.AssetManager.EnumerateEbx(selectedBundle))
                {
                    if (networkRegistry.Type == "NetworkRegistryAsset")
                    {
                        EbxAsset netRegEbx = App.AssetManager.GetEbx(networkRegistry);
                        List<PointerRef> objects = ((dynamic)netRegEbx.RootObject).Objects;

                        foreach (object obj in ebx.ExportedObjects)
                        {
                            PointerRef pointer = new PointerRef(new EbxImportReference()
                            {
                                FileGuid = assetToBundle.Guid, ClassGuid = ((dynamic)obj).GetInstanceGuid().ExportedGuid
                            });
                            if (networkedTypes.Contains(obj.GetType().Name) && !objects.Contains(pointer))
                            {
                                objects.Add(pointer);
                            }
                        }

                        netRegEbx.AddDependency(assetToBundle.Guid);
                        App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(netRegEbx.FileGuid).Name, netRegEbx);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an asset to a Mesh Variation Database
        /// </summary>
        /// <param name="assetToBundle"></param>
        /// <param name="selectedBundle"></param>
        /// <param name="task">Requires a stupid task window because cache loading requires it</param>
        public static void AddAssetToMvdBs(EbxAssetEntry assetToBundle, BundleEntry selectedBundle, FrostyTaskWindow task = null)
        {
            //Make sure the MVDB cache is loaded
            if (!MeshVariationDb.IsLoaded)
            {
                MeshVariationDb.LoadVariations();
            }

            task?.Update("Adding to MVDB...");

            EbxAsset ebxToBundle = App.AssetManager.GetEbx(assetToBundle);

            EbxAsset mvdb = null; //Its possible we maybe working with a duped bundle and MVDB, if thats the case, we cannot use cache.
            if (!selectedBundle.Added)
            {
                mvdb = mvdbs[mvdbBundles.IndexOf(selectedBundle.Name)];
            }
            else
            {
                //Extremely unoptimized way of brute forcing our way to the MVDB
                foreach (EbxAssetEntry assetEntry in App.AssetManager.EnumerateEbx(selectedBundle))
                {
                    if (assetEntry.Type == "MeshVariationDatabase")
                    {
                        mvdb = App.AssetManager.GetEbx(assetEntry.Name);
                        break;
                    }
                }
            }
            if (mvdb == null) return;
            dynamic mvdbObject = mvdb.RootObject as dynamic;
            dynamic newMvEntry = TypeLibrary.CreateObject("MeshVariationDatabaseEntry"); //Create a new entry for us to add to the MVDB

            //This is the fastest way I could think of to do this
            //If someone reads this please find a better way to do this!
            //This goes through each object in the Ebx we are bundle editing and checks the type it is
            //Different types need different sets of actions, so we use a switch case to check which type it is
            foreach (object obj in ebxToBundle.Objects)
            {
                switch (obj.GetType().Name)
                {
                    default: //A mesh asset
                        {
                            newMvEntry.Mesh = new PointerRef(new EbxImportReference() { FileGuid = assetToBundle.Guid, ClassGuid = ebxToBundle.RootInstanceGuid });
                            break;
                        }
                    case ("ObjectVariation"): //A variation of an object
                        {
                            dynamic objectVariation = obj as dynamic;
                            EbxAssetEntry meshAsset = null; //We set this value later

                            if (MeshVariationDb.FindVariations(objectVariation.NameHash, true) != null) //If its in mvdb cache
                            {
                                //Get the original mesh
                                MeshVariation refVariation = MeshVariationDb.FindVariations((uint)objectVariation.NameHash).First();
                                meshAsset = App.AssetManager.GetEbxEntry(refVariation.MeshGuid);
                            }
                            else //If not, its probably duped and should follow our rules
                            {
                                if (Guid.TryParse(assetToBundle.DisplayName.Split('_').ToList().First(), out Guid originalMeshGuid))
                                {
                                    meshAsset = RootInstanceEbxEntryDb.GetEbxEntryByRootInstanceGuid(originalMeshGuid);
                                }
                                else
                                {
                                    App.Logger.LogWarning("Cannot find mesh asset associated to {0}, are you sure the name of the variation is formatted correctly(`GuidOfMeshHere`_`NameOfVariationHere`)?", assetToBundle.DisplayName);
                                }
                            }

                            //References
                            if (meshAsset != null)
                            {
                                newMvEntry.Mesh = new PointerRef(new EbxImportReference() { FileGuid = meshAsset.Guid, ClassGuid = App.AssetManager.GetEbx(meshAsset).RootInstanceGuid });
                                newMvEntry.VariationAssetNameHash = (uint)objectVariation.NameHash;
                            }

                            break;
                        }
                    case ("MeshMaterial"): //A material in a regular mesh
                        {
                            dynamic meshMaterial = obj as dynamic;
                            dynamic newMaterialEntry = TypeLibrary.CreateObject("MeshVariationDatabaseMaterial"); //Create a new material object

                            newMaterialEntry.Material = new PointerRef(new EbxImportReference() { FileGuid = assetToBundle.Guid, ClassGuid = meshMaterial.GetInstanceGuid().ExportedGuid });

                            if (HasTextureParams) //Check if it has texture params before meddling with them
                            {
                                if (MeshVariationDb.GetVariations(assetToBundle.Guid) != null) //Check if cache is an option. If not the entry is duped(probably)
                                {
                                    MeshVariation refVariation = MeshVariationDb.GetVariations(assetToBundle.Guid).Variations[0];
                                    foreach (MeshVariationMaterial mvm in refVariation.Materials) // For each material in the original assets MVDB Entry
                                    {
                                        if (mvm.MaterialGuid == meshMaterial.GetInstanceGuid().ExportedGuid) // If it has the same guid
                                        {
                                            // We then use its texture params as the texture params in the variation
                                            foreach (dynamic texParam in (dynamic)mvm.TextureParameters)
                                            {
                                                newMaterialEntry.TextureParameters.Add(texParam);
                                            }
                                            break;
                                        }
                                    }
                                }
                                else if (meshMaterial.Shader.TextureParameters.Count > 0) //We are just going to check the material in the mesh for reference instead
                                {
                                    newMaterialEntry.TextureParameters = (meshMaterial).Shader.TextureParameters; //Copy the texture parameters over 
                                }
                                else //May as well just log a warning to the user in this event
                                {
                                    App.Logger.LogWarning("I am Unable to find any texture parameters for {0}, did you forget to add them to the mesh?", assetToBundle.DisplayName);
                                }
                            }

                            if (HasSurfaceGuidSetup)
                            {
                                foreach (Guid refguid in assetToBundle.EnumerateDependencies())
                                {
                                    if (App.AssetManager.GetEbxEntry(refguid).Type == "ShaderGraph")
                                    {
                                        newMaterialEntry.SurfaceShaderGuid = refguid;
                                        newMaterialEntry.SurfaceShaderId = (uint)Utils.HashString(refguid.ToString());
                                    }
                                }
                            }
                            if (HasMaterialId)
                            {
                                //Hacky way to generate a random int64
                                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                                {
                                    byte[] randomBytes = new byte[8];
                                    rng.GetBytes(randomBytes);
                                    newMaterialEntry.MaterialId = (Int64)BitConverter.ToInt64(randomBytes, 0);
                                }
                            }
                            newMvEntry.Materials.Add(newMaterialEntry);

                            break;
                        }

                    case ("MeshMaterialVariation"): //A variation of a material
                        {
                            dynamic objectVariation = ebxToBundle.RootObject as dynamic;

                            EbxAssetEntry meshAsset = null; //We set this value later
                            List<MeshVariationMaterial> meshVariationMaterials = null; //This will be set if its in the mvdb cache

                            if (MeshVariationDb.FindVariations(objectVariation.NameHash, true) != null) //If its in mvdb cache
                            {
                                //Get the original mesh
                                MeshVariation refVariation = MeshVariationDb.FindVariations((uint)objectVariation.NameHash).First();
                                meshAsset = App.AssetManager.GetEbxEntry(refVariation.MeshGuid);

                                meshVariationMaterials = refVariation.Materials;
                            }
                            else //If not, its probably duped and should follow our rules
                            {
                                if (Guid.TryParse(assetToBundle.DisplayName.Split('_').ToList().First(), out Guid originalMeshGuid))
                                {
                                    meshAsset = RootInstanceEbxEntryDb.GetEbxEntryByRootInstanceGuid(originalMeshGuid);
                                }
                                else
                                {
                                    App.Logger.LogWarning("Cannot find mesh asset associated to {0}, are you sure the name of the variation is formatted correctly(`GuidOfMeshHere`_`NameOfVariationHere`)?", assetToBundle.DisplayName);
                                }
                            }

                            //Materials
                            if (meshVariationMaterials != null && meshAsset != null)
                            {
                                foreach (MeshVariationMaterial material in meshVariationMaterials)
                                {
                                    dynamic newMaterialEntry = TypeLibrary.CreateObject("MeshVariationDatabaseMaterial"); //Create a new material object
                                    newMaterialEntry.Material = new PointerRef(new EbxImportReference() { FileGuid = meshAsset.Guid, ClassGuid = material.MaterialGuid });
                                    newMaterialEntry.MaterialVariation = new PointerRef(new EbxImportReference() { FileGuid = assetToBundle.Guid, ClassGuid = material.MaterialVariationClassGuid });

                                    if (HasTextureParams)
                                    {
                                        // We then use its texture params as the texture params in the variation
                                        foreach (dynamic texParam in (dynamic)material.TextureParameters)
                                        {
                                            newMaterialEntry.TextureParameters.Add(texParam);
                                        }
                                    }
                                    if (HasSurfaceGuidSetup)
                                    {
                                        foreach (Guid refguid in assetToBundle.EnumerateDependencies())
                                        {
                                            if (App.AssetManager.GetEbxEntry(refguid).Type == "ShaderGraph")
                                            {
                                                newMaterialEntry.SurfaceShaderGuid = refguid;
                                                newMaterialEntry.SurfaceShaderId = (uint)Utils.HashString(refguid.ToString());
                                            }
                                        }
                                    }
                                    if (HasMaterialId)
                                    {
                                        //Hacky way to generate a random int64
                                        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                                        {
                                            byte[] randomBytes = new byte[5];
                                            rng.GetBytes(randomBytes);
                                            newMaterialEntry.MaterialId = BitConverter.ToInt64(randomBytes, 0);
                                        }
                                    }
                                    newMvEntry.Materials.Add(newMaterialEntry);
                                }
                            }
                            else if (meshAsset != null) //its probably duped
                            {
                                foreach (dynamic material in App.AssetManager.GetEbx(meshAsset).Objects)
                                {
                                    dynamic newMaterialEntry = TypeLibrary.CreateObject("MeshVariationDatabaseMaterial"); //Create a new material object
                                    dynamic materialVariation = ebxToBundle.Objects.ToList()[App.AssetManager.GetEbx(meshAsset).Objects.ToList().IndexOf((object)material)];

                                    newMaterialEntry.Material = new PointerRef(new EbxImportReference() { FileGuid = meshAsset.Guid, ClassGuid = material.GetInstanceGuid().ExportedGuid });
                                    newMaterialEntry.MaterialVariation = new PointerRef(new EbxImportReference() { FileGuid = assetToBundle.Guid, ClassGuid = materialVariation.GetInstanceGuid().ExportedGuid });

                                    if (HasTextureParams)
                                    {
                                        newMaterialEntry.TextureParameters = (materialVariation).Shader.TextureParameters; //Copy the texture parameters over 
                                    }
                                    if (HasSurfaceGuidSetup)
                                    {
                                        foreach (Guid refguid in assetToBundle.EnumerateDependencies())
                                        {
                                            if (App.AssetManager.GetEbxEntry(refguid).Type == "ShaderGraph")
                                            {
                                                newMaterialEntry.SurfaceShaderGuid = refguid;
                                                newMaterialEntry.SurfaceShaderId = (uint)Utils.HashString(refguid.ToString());
                                            }
                                        }
                                    }
                                    if (HasMaterialId)
                                    {
                                        //Hacky way to generate a random int64
                                        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                                        {
                                            byte[] randomBytes = new byte[5];
                                            rng.GetBytes(randomBytes);
                                            newMaterialEntry.MaterialId = BitConverter.ToInt64(randomBytes, 0);
                                        }
                                    }
                                    newMvEntry.Materials.Add(newMaterialEntry);
                                }
                            }
                            break;
                        }
                }
            }

            mvdbObject.Entries.Add(newMvEntry);
            mvdb.AddDependency(assetToBundle.Guid);
            App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(mvdb.FileGuid).Name, mvdb);
        }

        /// <summary>
        /// Adds an unlock asset to a UnlockIdTable if the game has one and its a UnlockAssetBase subtype
        /// </summary>
        /// <param name="assetToBundle"></param>
        /// <param name="selectedBundle"></param>
        public static void AddAssetToTables(EbxAssetEntry assetToBundle, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToBundle);
            if (selectedBundle.Type == BundleType.SharedBundle || selectedBundle.Type == BundleType.BlueprintBundle)
            {
                if (HasUnlockIdTable && TypeLibrary.IsSubClassOf(assetToBundle.Type, "UnlockAssetBase"))
                {
                    foreach (EbxAssetEntry lAssetEntry in App.AssetManager.EnumerateEbx("LevelData"))
                    {
                        EbxAsset lvlEbx = App.AssetManager.GetEbx(lAssetEntry);
                        uint unlockId = ((dynamic)ebx.RootObject).Identifier;
                        
                        if (((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Contains(unlockId)) continue;
                        
                        ((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Add(unlockId);
                        App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(lvlEbx.FileGuid).Name, lvlEbx);
                    }
                }
            }
            
            //If its not a shared/blueprint bundle then its a sublevel
            EbxAssetEntry lvlAssetEntry = App.AssetManager.GetEbxEntry(App.AssetManager.GetSuperBundle(selectedBundle.SuperBundleId).Name.Remove(0, 6));
            if (HasUnlockIdTable && TypeLibrary.IsSubClassOf(assetToBundle.Type, "UnlockAssetBase") && lvlAssetEntry != null)
            {
                EbxAsset lvlEbx = App.AssetManager.GetEbx(lvlAssetEntry);
                uint unlockId = ((dynamic)ebx.RootObject).Identifier;
                if (((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Contains(unlockId)) return;
                
                ((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Add(unlockId);
                App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(lvlEbx.FileGuid).Name, lvlEbx);
            }
        }
        #endregion

        #region --Remove from Bundle Methods--

        /// <summary>
        /// Removes an asset from a bundle
        /// </summary>
        /// <param name="assetToRemove"></param>
        /// <param name="selectedBundle"></param>
        public static void RemoveAssetFromBundle(EbxAssetEntry assetToRemove, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToRemove);
            App.AssetManager.ModifyEbx(assetToRemove.Name, ebx);

            string key = assetToRemove.Type;
            if (!RemoveFromBundleExtensions.ContainsKey(assetToRemove.Type))
            {
                key = "null";
                foreach (string typekey in RemoveFromBundleExtensions.Keys)
                {
                    if (TypeLibrary.IsSubClassOf(assetToRemove.Type, typekey))
                    {
                        key = typekey;
                        break;
                    }
                }
            }
            RemoveFromBundleExtensions[key].RemoveFromBundle(assetToRemove, selectedBundle);
        }

        /// <summary>
        /// Removes an asset from Network Registries
        /// </summary>
        /// <param name="assetToRemove"></param>
        /// <param name="selectedBundle"></param>
        public static void RemoveAssetFromNetRegs(EbxAssetEntry assetToRemove, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToRemove);
            List<object> ebxObjects = ebx.Objects.ToList();

            if (!selectedBundle.Added)
            {
                EbxAsset netRegEbx = networkRegistries[networkedBundles.IndexOf(selectedBundle.Name)];
                List<PointerRef> objects = ((dynamic)netRegEbx.RootObject).Objects;

                foreach (dynamic obj in ebxObjects)
                {
                    objects.Remove(new PointerRef(new EbxImportReference() { FileGuid = assetToRemove.Guid, ClassGuid = ((dynamic)obj).GetInstanceGuid().ExportedGuid }));
                }
                App.AssetManager.GetEbxEntry(netRegEbx.FileGuid).ModifiedEntry.DependentAssets.Remove(assetToRemove.Guid);
                App.AssetManager.ModifyEbx(assetToRemove.Name, ebx);
            }
            else
            {
                foreach (EbxAssetEntry networkRegistry in App.AssetManager.EnumerateEbx(selectedBundle))
                {
                    if (networkRegistry.Type == "NetworkRegistryAsset" && networkRegistry.EnumerateDependencies().Contains(assetToRemove.Guid))
                    {
                        EbxAsset netRegEbx = App.AssetManager.GetEbx(networkRegistry);
                        List<PointerRef> objects = ((dynamic)netRegEbx.RootObject).Objects;

                        foreach (dynamic obj in ebxObjects)
                        {
                            objects.Remove(new PointerRef(new EbxImportReference() { FileGuid = assetToRemove.Guid, ClassGuid = ((dynamic)obj).GetInstanceGuid().ExportedGuid }));
                        }
                        networkRegistry.ModifiedEntry.DependentAssets.Remove(assetToRemove.Guid);
                        App.AssetManager.ModifyEbx(assetToRemove.Name, ebx);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an asset from Network Registries
        /// </summary>
        /// <param name="assetToRemove"></param>
        /// <param name="selectedBundle"></param>
        public static void RemoveAssetFromMeshVariations(EbxAssetEntry assetToRemove, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToRemove);
            List<object> ebxObjects = ebx.Objects.ToList();

            if (!selectedBundle.Added)
            {
                EbxAsset mvdbEbx = mvdbs[mvdbBundles.IndexOf(selectedBundle.Name)];
                dynamic mvdbObject = mvdbEbx.RootObject as dynamic;

                for (int i = 0; true; i++)
                {
                    if (i >= mvdbObject.Entries.Count)
                    {
                        break;
                    }
                    dynamic entry = mvdbObject.Entries[i];
                    if (assetToRemove.Type != "ObjectVariation" && entry.Mesh.External.FileGuid == assetToRemove.Guid) //We want to remove ALL entries with the mesh, including variations
                    {
                        mvdbObject.Entries.RemoveAt(i);
                    }
                    else if (entry.VariationAssetNameHash == ((dynamic)ebx.RootObject).NameHash) //If its a variation though, we just remove the variation
                    {
                        mvdbObject.Entries.RemoveAt(i);
                        break;
                    }
                }

                App.AssetManager.GetEbxEntry(mvdbEbx.FileGuid).ModifiedEntry.DependentAssets.Remove(assetToRemove.Guid);
                App.AssetManager.ModifyEbx(assetToRemove.Name, ebx);
            }
            else
            {
                foreach (EbxAssetEntry mvdbRegistry in App.AssetManager.EnumerateEbx("MeshVariationDatabase"))
                {
                    if (mvdbRegistry.AddedBundles[0] == App.AssetManager.GetBundleId(selectedBundle))
                    {
                        EbxAsset mvdbEbx = App.AssetManager.GetEbx(mvdbRegistry);
                        dynamic mvdbObject = mvdbEbx.RootObject as dynamic;

                        for (int i = 0; true; i++)
                        {
                            if (i >= mvdbObject.Entries.Count)
                            {
                                break;
                            }
                            dynamic entry = mvdbObject.Entries[i];
                            if (assetToRemove.Type != "ObjectVariation" && entry.Mesh.External.FileGuid == assetToRemove.Guid) //We want to remove ALL entries with the mesh, including variations
                            {
                                mvdbObject.Entries.RemoveAt(i);
                            }
                            else if (entry.VariationAssetNameHash == ((dynamic)ebx.RootObject).NameHash) //If its a variation though, we just remove the variation
                            {
                                mvdbObject.Entries.RemoveAt(i);
                                break;
                            }
                        }

                        mvdbRegistry.ModifiedEntry.DependentAssets.Remove(assetToRemove.Guid);
                        App.AssetManager.ModifyEbx(assetToRemove.Name, ebx);
                    }
                }
            }

        }

        /// <summary>
        /// Removes an unlock asset from a UnlockIdTable if the game has them and its a UnlockAssetBase subtype
        /// </summary>
        /// <param name="assetToRemove"></param>
        /// <param name="selectedBundle"></param>
        public static void RemoveAssetFromTables(EbxAssetEntry assetToRemove, BundleEntry selectedBundle)
        {
            EbxAsset ebx = App.AssetManager.GetEbx(assetToRemove);
            EbxAssetEntry lvlAssetEntry = App.AssetManager.GetEbxEntry(App.AssetManager.GetSuperBundle(selectedBundle.SuperBundleId).Name.Remove(0, 6));
            if (HasUnlockIdTable && TypeLibrary.IsSubClassOf(assetToRemove.Type, "UnlockAssetBase") && lvlAssetEntry != null)
            {
                EbxAsset lvlEbx = App.AssetManager.GetEbx(lvlAssetEntry);
                uint unlockId = ((dynamic)ebx.RootObject).Identifier;
                if (((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Contains(unlockId))
                {
                    ((dynamic)lvlEbx.RootObject).UnlockIdTable.Identifiers.Remove(unlockId);
                    App.AssetManager.ModifyEbx(App.AssetManager.GetEbxEntry(lvlEbx.FileGuid).Name, lvlEbx);
                }
            }
        }
        #endregion

        #region --Add to Bundle Requirement Checks--

        /// <summary>
        /// This will check if an asset is valid for recursive adding
        /// </summary>
        /// <param name="assetToCheck"></param>
        /// <param name="bundleToCheck"></param>
        /// <returns>A bool on whether or not the asset is valid</returns>
        public static bool AssetRecAddValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            bool isValid = true;

            List<int> bundles = new List<int>(assetToCheck.Bundles);
            bundles.AddRange(assetToCheck.AddedBundles);

            foreach (int bundle in bundles)
            {
                BundleEntry bundleEntry = App.AssetManager.GetBundleEntry(bundle);
                switch (bundleToCheck.Type)
                {
                    case BundleType.SubLevel:
                    {
                        //If its a shared bundle its probably going to be already loaded
                        //And if its a subworld bundle its definitely going to be already loaded
                        switch (bundleEntry.Type)
                        {
                            case BundleType.SharedBundle:
                            {
                                isValid = false;
                            } break;
                            case BundleType.SubLevel:
                            {
                                if (HasSuperBundles)
                                {
                                    int levelBundle = App.AssetManager.GetBundleId(App.AssetManager.GetSuperBundle(bundleEntry.SuperBundleId).Name);
                                    isValid = levelBundle != bundle;
                                }
                                else
                                {
                                    //TODO: Get parent bundle
                                }
                            } break;
                        }
                    } break;
                }
            }

            return isValid;
        }

        /// <summary>
        /// This will check if an asset is valid for networking
        /// </summary>
        /// <param name="assetToCheck"></param>
        /// <param name="bundleToCheck"></param>
        /// <returns>A bool on whether or not the asset is valid</returns>
        public static bool AssetAddNetworkValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            return networkedTypes.Contains(assetToCheck.Type) && (networkedBundles.Contains(bundleToCheck.Name) || bundleToCheck.Added) && Config.Get("AllowRootNetreg", false);
        }

        public static bool AssetAddMeshVariationValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            return (TypeLibrary.IsSubClassOf(assetToCheck.Type, "MeshAsset") || assetToCheck.Type == "ObjectVariation") && (mvdbBundles.Contains(bundleToCheck.Name) || bundleToCheck.Added) && Config.Get("AllowMVDB", false);
        }

        #endregion

        #region --Remove from Bundle Requirement Checks--

        /// <summary>
        /// This will check if an asset is valid for recursive adding
        /// </summary>
        /// <param name="assetToCheck"></param>
        /// <param name="bundleToCheck"></param>
        /// <returns>A bool on whether or not the asset is valid</returns>
        public static bool AssetRecRemValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            bool isValid = false;

            //Enumerate over all of the bundles the asset has
            foreach (int bunId in assetToCheck.AddedBundles)
            {
                BundleEntry bentry = App.AssetManager.GetBundleEntry(bunId);
                
                if (bundleToCheck.DisplayName != bentry.DisplayName) continue;
                
                isValid = true; //The asset is valid
                break;
            }
            return isValid;
        }

        /// <summary>
        /// This will check if an asset is valid for networking
        /// </summary>
        /// <param name="assetToCheck"></param>
        /// <param name="bundleToCheck"></param>
        /// <returns>A bool on whether or not the asset is valid</returns>
        public static bool AssetRemNetworkValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            if (!networkedBundles.Contains(bundleToCheck.Name)) return false;
            
            if (bundleToCheck.Added)
            {
                return networkedTypes.Contains(assetToCheck.Type) && Config.Get("AllowRootNetreg", false);
            }
            EbxAssetEntry registry = App.AssetManager.GetEbxEntry(networkRegistries[networkedBundles.IndexOf(bundleToCheck.Name)].FileGuid);
            return networkedTypes.Contains(assetToCheck.Type) && (networkedBundles.Contains(bundleToCheck.Name) || bundleToCheck.Added) && Config.Get("AllowRootNetreg", false) && registry.EnumerateDependencies().Contains(assetToCheck.Guid);
        }

        public static bool AssetRemMeshVariationValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            return (TypeLibrary.IsSubClassOf(assetToCheck.Type, "MeshAsset") || assetToCheck.Type == "ObjectVariation") && (mvdbBundles.Contains(bundleToCheck.Name) && App.AssetManager.GetEbxEntry(mvdbs[mvdbBundles.IndexOf(bundleToCheck.Name)].FileGuid).EnumerateDependencies().Contains(assetToCheck.Guid) || bundleToCheck.Added) && Config.Get("AllowMVDB", false);
        }

        #endregion

        #region BunOp Requirement Checks

        /// <summary>
        /// This will check if an asset is valid for the AddAsset Bundle Operation
        /// </summary>
        /// <param name="assetToCheck"></param>
        /// <param name="bundleToCheck"></param>
        /// <returns>A bool on whether or not the asset is valid</returns>
        public static bool AssetOpAddValid(EbxAssetEntry assetToCheck, BundleEntry bundleToCheck)
        {
            bool isValid = false;

            //Enumerate over all of the bundles the asset has
            foreach (int bunId in assetToCheck.Bundles)
            {
                BundleEntry bentry = App.AssetManager.GetBundleEntry(bunId);

                //If this bundle isn't a shared bundle(unless we are trying to load it into another shared bundle for whatever reason)
                //AND if this asset's super bundle name doesn't equal the bundles name(meaning this asset is loaded into the leveldata)
                //AND this bundle isn't the same as the one we are adding to
                if ((bentry.Type != BundleType.SharedBundle) && (App.AssetManager.GetSuperBundle(bundleToCheck.SuperBundleId).Name != bentry.Name) && bentry != bundleToCheck)
                {
                    isValid = true; //The asset is valid 
                }
                else
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid) return isValid || assetToCheck.AddedBundles.Count + assetToCheck.Bundles.Count == 0;
            foreach (int bunId in assetToCheck.AddedBundles)
            {
                BundleEntry bentry = App.AssetManager.GetBundleEntry(bunId);

                //If this bundle isn't a shared bundle(unless we are trying to load it into another shared bundle for whatever reason)
                //AND if this asset's super bundle name doesn't equal the bundles name(meaning this asset is loaded into the leveldata)
                //AND this bundle isn't the same as the one we are adding to
                if ((bentry.Type != BundleType.SharedBundle) && (App.AssetManager.GetSuperBundle(bundleToCheck.SuperBundleId).Name != bentry.Name) && bentry != bundleToCheck)
                {
                    isValid = true; //The asset is valid 
                }
                else
                {
                    isValid = false;
                    break;
                }
            }

            return isValid || assetToCheck.AddedBundles.Count + assetToCheck.Bundles.Count == 0;
        }      

        #endregion
    }
}
