using AdvancedBundleEditorPlugin.Options;
using BundleEditPlugin;
using Frosty.Core.Attributes;
using FrostySdk;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page, 
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f9cb2893-2811-4d4c-9d94-073ba4cd0bf4")]

[assembly: PluginDisplayName("Advanced Bundle Editor")]
[assembly: PluginAuthor("GalaxyMan2015, CosmicDreams, and Y wingpilot2")]
[assembly: PluginVersion("0.0.0.2")]

[assembly: RegisterMenuExtension(typeof(BundleEditorMenuExtension))]
[assembly: RegisterMenuExtension(typeof(GenerateNetworkCache))]
[assembly: RegisterMenuExtension(typeof(GenerateMvdbBundleCache))]
[assembly: RegisterMenuExtension(typeof(OpenBundleOperation))]
[assembly: RegisterTabExtension(typeof(BundlesTabExtension))]
[assembly: RegisterTabExtension(typeof(SuperBundlesTabExtension))]
[assembly: RegisterOptionsExtension(typeof(BundleEditorOptions), Frosty.Core.PluginManagerType.Both)]
[assembly: RegisterDataExplorerContextMenu(typeof(IgnoreAssetContextMenuItem))]