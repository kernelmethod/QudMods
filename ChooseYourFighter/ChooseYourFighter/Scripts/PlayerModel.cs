using ConsoleLib.Console;
using System;
using XRL.CharacterBuilds;
using XRL.World;

namespace Kernelmethod.ChooseYourFighter {
    public class PlayerModelData : AbstractEmbarkBuilderModuleData {
        public PlayerModel model = null;
    }

    [Serializable]
    public enum ModelType {
        Preset,
        CasteOrCalling,
        Expansion,
        Unknown
    }

    [Serializable]
    public class PlayerModel : IComparable<PlayerModel> {
        public string Id;
        // TODO: replace with a private field and getter / setter after next breaking
        // game update.
        public string Name;
        public string Tile = null;
        public string Foreground = "y";
        public string Background = "k";
        public string DetailColor = null;
        public bool HFlip = false;
        public ModelType Category = ModelType.Unknown;

        // TODO: serialize these fields after next breaking game update.
        [NonSerialized]
        public string Group = null;
        [NonSerialized]
        public bool IsGroup = false;

        public string ColorString {
            get {
                if (Background.IsNullOrEmpty())
                    return "&" + Foreground;

                return "&" + Foreground + "^" + Background;
            }
        }

        public string ColorizedName {
            get {
                var choiceName = Name;

                // Colorize name based on whether it corresponds to a group or a tile
                if (IsGroup && !ColorUtility.HasFormatting(choiceName)) {
                    choiceName = ColorUtility.ApplyColor(choiceName, "W");
                }
                else if (!IsGroup && !ColorUtility.HasFormatting(choiceName)) {
                    choiceName = ColorUtility.ApplyColor(choiceName, "M");
                }

                return choiceName;
            }
            set {
                Name = value;
            }
        }

        public PlayerModel() {}

        /// <summary>
        /// Construct a new PlayerModel from a blueprint.
        /// </summary>
        public PlayerModel(GameObjectBlueprint blueprint) {
            var gameObject = blueprint.createOne();
            Name = blueprint.DisplayName();
            Tile = gameObject.GetTile();
            Foreground = gameObject.Render.GetTileForegroundColor();
            Background = gameObject.GetBackgroundColor();
            DetailColor = gameObject.GetDetailColor();
        }

        public IRenderable Icon() {
            if (Tile == null)
              return null;

            var icon = new Renderable();
            icon.Tile = Tile;
            icon.ColorString = ColorString;
            icon.DetailColor = DetailColor[0];
            return icon;
        }

        public bool Equals(PlayerModel model) {
            return model.Id == Id;
        }

        public int CompareTo(PlayerModel model) {
            if (IsGroup && !model.IsGroup)
                return -1;

            if (!IsGroup && model.IsGroup)
                return 1;

            return -model.Id.CompareTo(Id);
        }
    }
}
