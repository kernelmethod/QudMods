using System;
using XRL.World;

namespace Kernelmethod.ChooseYourFighter {
    /// <summary>
    /// A part that stores the original tile that was used by an object.
    /// </summary>
    [Serializable]
    public class DefaultModel : IPart {
        public PlayerModel Model = null;

        public override void Initialize() {
            base.Initialize();
            Model = new PlayerModel();
            Model.Tile = ParentObject.GetTile();
            Model.Foreground = ParentObject.GetForegroundColor();
            Model.Background = ParentObject.GetBackgroundColor();
            Model.DetailColor = ParentObject.GetDetailColor();
        }
    }
}