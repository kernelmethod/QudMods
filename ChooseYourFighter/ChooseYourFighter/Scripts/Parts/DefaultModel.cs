using System;
using XRL;
using XRL.World;
using XRL.World.Parts;

namespace Kernelmethod.ChooseYourFighter {
    /// <summary>
    /// A part that stores the original tile that was used by an object.
    /// </summary>
    [Serializable]
    public class DefaultModel : IScribedPart {
        public PlayerModel Model = null;

        public override void Initialize() {
            base.Initialize();
            Model = new PlayerModel();
            Model.Tile = ParentObject.GetTile();
            Model.Foreground = ParentObject.GetForegroundColor();
            Model.Background = ParentObject.GetBackgroundColor();
            Model.DetailColor = ParentObject.GetDetailColor();

            var pRender = ParentObject.GetPart<Render>();
            if (pRender != null) {
                Model.HFlip = TileFactory.CheckFlip(ParentObject) ? pRender.HFlip : !pRender.HFlip;
            }
        }

        public override void Read(GameObject Basis, SerializationReader Reader) {
            var modVersion = Reader.ModVersions["Kernelmethod_ChooseYourFighter"];

            if (modVersion >= (new XRL.Version("0.7.0"))) {
                base.Read(Basis, Reader);
                return;
            }

            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter: migrating DefaultModel instance from {modVersion}");

            // Versions < 0.7.0 used a non-IScribed interface
            Model = Reader.ReadObject() as PlayerModel;
        }
    }
}
