using System;
using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

using Kernelmethod.ChooseYourFighter;

namespace XRL.CharacterBuilds.Qud {
    public class Kernelmethod_ChooseYourFighter_PlayerModelModule : EmbarkBuilderModule<PlayerModelData> {
        /// <summary>
        /// Return a dictionary containing all of the possible player tiles, keyed to
        /// tile IDs.
        /// </summary>
        public Dictionary<string, PlayerModel> ModelDict {
            get {
                return TileFactory.ModelDict;
            }
        }

        /// <summary>
        /// Return an iterator over all of the usable player tiles.
        /// </summary>
        public IEnumerable<PlayerModel> Models {
            get {
                foreach (var model in ModelDict.Values)
                    yield return model;
            }
        }

        /// <summary>
        /// Do not include the information from this module in build codes.
        /// </summary>
        public override bool IncludeInBuildCodes() {
            return true;
        }

        /// <summary>
        /// Enable the module after the character caste/calling has been selected.
        /// </summary>
        public override bool shouldBeEnabled() {
            return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
        }

        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null) {
            var model = info.getData<PlayerModelData>()?.model;

            if (model == null || model.Id == null)
                return base.handleBootEvent(id, game, info, element);

            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE)
                return model.Tile;
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND)
                return model.Foreground;
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEBACKGROUND)
                return model.Background;
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL)
                return model.DetailColor;
            if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING) {
                try {
                    The.Player.Render.HFlip = model.HFlip;
                }
                catch (Exception ex) {
                    Utils.LogInfo("Error setting final tile properties: " + ex.ToString());
                }
            }

            return base.handleBootEvent(id, game, info, element);
        }
    }
}
