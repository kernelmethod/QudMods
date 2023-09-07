using ConsoleLib.Console;
using System;
using XRL.CharacterBuilds;

namespace Kernelmethod.ChooseYourFighter {
    public class PlayerModelData : AbstractEmbarkBuilderModuleData {
        public PlayerModel model = null;
    }

    public class PlayerModel : IComparable<PlayerModel> {
        public string Id;
        public string Name;
        public string Tile = null;
        public string Foreground = "y";
        public string Background = "k";
        public string DetailColor = null;

        public IRenderable Icon() {
            if (Tile == null)
              return null;

            var icon = new Renderable();
            icon.Tile = Tile;
            icon.ColorString = Foreground;
            icon.DetailColor = DetailColor[0];
            return icon;
        }

        public bool Equals(PlayerModel model) {
            return model.Id == Id;
        }

        public int CompareTo(PlayerModel model) {
            return -model.Id.CompareTo(Id);
        }
    }
}
