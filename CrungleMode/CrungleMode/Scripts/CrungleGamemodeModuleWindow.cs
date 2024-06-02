using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud.UI;

namespace Kernelmethod.CrungleMode.UI {
    public class CrungleGamemodeModuleWindow : QudGamemodeModuleWindow {
        public override UIBreadcrumb GetBreadcrumb() {
            var breadcrumb = base.GetBreadcrumb();
            breadcrumb.Id = "Kernelmethod_CrungleMode_Gamemode";
            return breadcrumb;
        }
    }
}
