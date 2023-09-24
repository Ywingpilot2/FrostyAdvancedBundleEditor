using Frosty.Controls;
using Frosty.Core;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdvancedBundleEditorPlugin
{
    [TemplatePart(Name = PART_BundlesSelected, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_BundlesList, Type = typeof(TextBox))]
    public class BundleTabItem : FrostyTabItem
    {
        private const string PART_BundlesSelected = "PART_BundlesSelected";
        private const string PART_BundlesList = "PART_BundlesList";

        private TextBlock bundlesSelectedTextBlock;
        private TextBox bundlesListTextBox;

        private TabControl miscTabControl;

        static BundleTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BundleTabItem), new FrameworkPropertyMetadata(typeof(BundleTabItem)));
        }

        public BundleTabItem()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            bundlesSelectedTextBlock = GetTemplateChild(PART_BundlesSelected) as TextBlock;
            bundlesListTextBox = GetTemplateChild(PART_BundlesList) as TextBox;

            App.EditorWindow.DataExplorer.SelectionChanged += dataExplorer_SelectionChanged;
            App.EditorWindow.MiscTabControl.SelectionChanged += miscTabControl_SelectionChanged;

            miscTabControl = App.EditorWindow.MiscTabControl;
        }

        private void dataExplorer_SelectionChanged(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry selectedEntry = App.SelectedAsset;

            RefreshBundles(selectedEntry);
        }

        private void miscTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EbxAssetEntry selectedEntry = App.SelectedAsset;

            RefreshBundles(selectedEntry);
        }

        public void RefreshBundles(EbxAssetEntry entry)
        {
            // TODO: Figure out how to get tab item ref
            //if (!bundleTabItem.IsSelected)
            //    return;

            if (entry == null)
            {
                bundlesSelectedTextBlock.Text = "No asset selected";
                bundlesListTextBox.Text = "";
                return;
            }

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            foreach (int bundleId in entry.Bundles)
                sb1.AppendLine(App.AssetManager.GetBundleEntry(bundleId).Name);
            foreach (int bundleId in entry.AddedBundles)
                sb2.AppendLine(App.AssetManager.GetBundleEntry(bundleId).Name);

            bundlesSelectedTextBlock.Text = "Bundles for " + entry.Filename;
            bundlesListTextBox.Text = sb1.ToString();
            if (sb2.Length > 0)
                bundlesListTextBox.Text += "\r\nAdded to bundles:\r\n" + sb2.ToString();
        }
    }

    [TemplatePart(Name = PART_SuperBundlesSelected, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_SuperBundlesList, Type = typeof(TextBox))]
    public class SuperBundleTabItem : FrostyTabItem
    {
        private const string PART_SuperBundlesSelected = "PART_SuperBundlesSelected";
        private const string PART_SuperBundlesList = "PART_SuperBundlesList";

        private TextBlock superBundlesSelectedTextBlock;
        private TextBox superBundlesListTextBox;

        private TabControl miscTabControl;

        static SuperBundleTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuperBundleTabItem), new FrameworkPropertyMetadata(typeof(SuperBundleTabItem)));
        }

        public SuperBundleTabItem()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            superBundlesSelectedTextBlock = GetTemplateChild(PART_SuperBundlesSelected) as TextBlock;
            superBundlesListTextBox = GetTemplateChild(PART_SuperBundlesList) as TextBox;

            App.EditorWindow.DataExplorer.SelectionChanged += dataExplorer_SelectionChanged;
            App.EditorWindow.MiscTabControl.SelectionChanged += miscTabControl_SelectionChanged;

            miscTabControl = App.EditorWindow.MiscTabControl;
        }

        private void dataExplorer_SelectionChanged(object sender, RoutedEventArgs e)
        {
            EbxAssetEntry selectedEntry = App.SelectedAsset;

            RefreshSuperBundles(selectedEntry);
        }

        private void miscTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EbxAssetEntry selectedEntry = App.SelectedAsset;

            RefreshSuperBundles(selectedEntry);
        }

        public void RefreshSuperBundles(EbxAssetEntry entry)
        {
            // TODO: Figure out how to get tab item ref
            //if (!bundleTabItem.IsSelected)
            //    return;

            if (entry == null)
            {
                superBundlesSelectedTextBlock.Text = "No asset selected";
                superBundlesListTextBox.Text = "";
                return;
            }

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            foreach (int bundleId in entry.Bundles)
                sb1.AppendLine(App.AssetManager.GetSuperBundle(App.AssetManager.GetBundleEntry(bundleId).SuperBundleId).Name);
            foreach (int bundleId in entry.AddedBundles)
                sb2.AppendLine(App.AssetManager.GetSuperBundle(App.AssetManager.GetBundleEntry(bundleId).SuperBundleId).Name);

            superBundlesSelectedTextBlock.Text = "Super Bundles for " + entry.Filename;
            superBundlesListTextBox.Text = sb1.ToString();
            if (sb2.Length > 0)
                superBundlesListTextBox.Text += "\r\nAdded to Super bundles:\r\n" + sb2.ToString();
        }
    }
}
