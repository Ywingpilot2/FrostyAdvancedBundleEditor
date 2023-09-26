using Frosty.Controls;
using Frosty.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdvancedBundleEditorPlugin
{
    public class SuperBundlesTabExtension : TabExtension
    {
        public override string TabItemName => "Super Bundles";

        public override FrostyTabItem TabContent => new SuperBundleTabItem();
    }
}
