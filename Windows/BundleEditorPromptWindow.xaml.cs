using System.Windows;
using Frosty.Controls;
using Frosty.Core.Controls;

namespace AdvancedBundleEditorPlugin.Windows
{
    public partial class BundleEditorPromptWindow : FrostyDockableWindow
    {
        public MessageBoxResult MessageBoxResult { get; set; }
        
        public BundleEditorPromptWindow()
        {
            InitializeComponent();
            MessageBoxResult = MessageBoxResult.None;
        }

        public static MessageBoxResult Show(object prompt, string title)
        {
            BundleEditorPromptWindow promptWindow = new BundleEditorPromptWindow()
            {
                Title = title
            };
            promptWindow.PropertyGrid.Object = prompt;
            promptWindow.ShowDialog();
            
            return promptWindow.MessageBoxResult;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.OK;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Cancel;
            Close();
        }
    }
}