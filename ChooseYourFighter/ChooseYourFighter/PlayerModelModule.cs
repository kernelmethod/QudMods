using System;
using System.Collections.Generic;

using Kernelmethod.ChooseYourFighter;

namespace XRL.CharacterBuilds.Qud {
    public class Kernelmethod_ChooseYourFighter_PlayerModelModule : EmbarkBuilderModule<PlayerModelData> {
        private Dictionary<string, PlayerModel> _Models = null;
        private PlayerModel currentReadingModelData = null;

        /// <summary>
        /// Return a dictionary containing all of the possible player models, keyed to
        /// model IDs.
        /// </summary>
        public Dictionary<string, PlayerModel> ModelDict {
            get {
                if (_Models != null)
                    return _Models;

                // Initialize dictionary and populate with castes/callings
                _Models = new Dictionary<string, PlayerModel>();

                foreach (var entry in SubtypeFactory.Subtypes) {
                    var id = entry.Name;
                    var model = new PlayerModel {
                        Id=id,
                        Name="{{M|" + entry.DisplayName + "}}",
                        Tile=entry.Tile,
                        DetailColor=entry.DetailColor
                    };
                    _Models.Add(id, model);
                }

                return _Models;
            }
        }

        /// <summary>
        /// Return an iterator over all of the usable player models.
        /// </summary>
        public IEnumerable<PlayerModel> Models {
            get {
                foreach (var model in ModelDict.Values)
                    yield return model;
            }
        }

        public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
        {
            get
            {
                Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
                xmlNodes.Add("models", delegate(XmlDataHelper xml)
                {
                    xml.HandleNodes(XmlNodeHandlers);
                });
                return xmlNodes;
            }
        }

        public Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
        {
            {
                "model",
                delegate(XmlDataHelper xml)
                {
                    string id = xml.GetAttribute("Id");

                    if (!ModelDict.TryGetValue(id, out currentReadingModelData)) {
                        currentReadingModelData = new PlayerModel();
                        ModelDict.Add(id, currentReadingModelData);
                    }

                    currentReadingModelData.Id = id;
                    currentReadingModelData.Name = xml.GetAttribute("Name");

                    xml.HandleNodes(XmlNodeHandlers);
                    currentReadingModelData = null;
                }
            },
            {
                "tile",
                delegate(XmlDataHelper xml)
                {
                    if (xml.HasAttribute("Path"))
                        currentReadingModelData.Tile = xml.GetAttribute("Path");
                    if (xml.HasAttribute("Foreground"))
                        currentReadingModelData.Foreground = xml.GetAttribute("Foreground");
                    if (xml.HasAttribute("Background"))
                        currentReadingModelData.Background = xml.GetAttribute("Background");
                    if (xml.HasAttribute("DetailColor"))
                        currentReadingModelData.DetailColor = xml.GetAttribute("DetailColor");
                }
            }
        };

        /// <summary>
        /// Do not include the information from this module in build codes.
        /// </summary>
        public override bool IncludeInBuildCodes() {
            return false;
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

            return base.handleBootEvent(id, game, info, element);
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_ChooseYourFighter::Kernelmethod_ChooseYourFighter_PlayerModelModule: {message}");
        }
    }
}
