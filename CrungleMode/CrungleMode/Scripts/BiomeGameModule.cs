using Kernelmethod.CrungleMode;
using Kernelmethod.CrungleMode.Utilities;
using Kernelmethod.CrungleMode.ZoneSampling;

using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Effects;
using XRL.Names;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud
{
    public class Kernelmethod_CrungleMode_BiomeGameModule : EmbarkBuilderModule<Kernelmethod_CrungleMode_BiomeGameModuleData>
    {
        public class BiomeData : FrameworkDataElement
        {
            public string Name;
            public string Tile = "Terrain/sw_moonstair_b_worldmap_1.bmp";
            public string Foreground = "Y";
            public string Background;
            public string Detail = "Y";

            public List<string> Terrain = new List<string>();
            public List<AbstractTerrainFilter> TerrainFilters = new List<AbstractTerrainFilter>();

            public XYZSelector ZoneXYZ = new XYZSelector();

            public BiomeData(string name) {
                Name = name;
                TerrainFilters.Add(new TerrainListFilter());
            }
        }

        public Dictionary<string, BiomeData> biomes = new Dictionary<string, BiomeData>();
        private BiomeData currentReadingBiomeData = null;
        private GameObject Target;

        public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
        {
            get
            {
                Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
                xmlNodes.Add("biomes", delegate(XmlDataHelper xml)
                {
                    xml.HandleNodes(XmlNodeHandlers);
                });
                return xmlNodes;
            }
        }

        public Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
        {
            {
                "biome",
                delegate(XmlDataHelper xml)
                {
                    string attribute = xml.GetAttribute("Name");
                    if (!biomes.TryGetValue(attribute, out currentReadingBiomeData))
                    {
                        currentReadingBiomeData = new BiomeData(attribute);
                        biomes.Add(attribute, currentReadingBiomeData);
                    }

                    currentReadingBiomeData.Name = xml.GetAttribute("Name");
                    if (xml.HasAttribute("Tile"))
                        currentReadingBiomeData.Tile = xml.GetAttribute("Tile");
                    if (xml.HasAttribute("Foreground"))
                        currentReadingBiomeData.Foreground = xml.GetAttribute("Foreground");
                    if (xml.HasAttribute("Background"))
                        currentReadingBiomeData.Background = xml.GetAttribute("Background");
                    if (xml.HasAttribute("Detail"))
                        currentReadingBiomeData.Detail = xml.GetAttribute("Detail");
                    xml.HandleNodes(XmlNodeHandlers);
                    currentReadingBiomeData = null;
                }
            },
            {
                "description",
                delegate(XmlDataHelper xml)
                {
                    currentReadingBiomeData.Description = xml.GetTextNode();
                }
            },
            {
                "terrain",
                delegate(XmlDataHelper xml)
                {
                    TerrainListFilter filter = currentReadingBiomeData.TerrainFilters[0] as TerrainListFilter;

                    if (xml.HasAttribute("Name"))
                        filter.ValidTerrain.Add(xml.GetAttribute("Name"));

                    if (xml.HasAttribute("Filter")) {
                        var filterType = xml.GetAttribute("Filter");
                        Type t = Type.GetType($"Kernelmethod.CrungleMode.ZoneSampling.{filterType}");
                        ConstructorInfo ctor = t.GetConstructor(new Type[] {});
                        currentReadingBiomeData.TerrainFilters.Add(ctor.Invoke(new object[] {}) as AbstractTerrainFilter);
                    }
                }
            },
            {
                "xyzselector",
                delegate(XmlDataHelper xml)
                {
                    var selectorType = xml.GetAttribute("Name");
                    Type t = Type.GetType($"Kernelmethod.CrungleMode.ZoneSampling.{selectorType}");
                    ConstructorInfo ctor = t.GetConstructor(new Type[] {});
                    currentReadingBiomeData.ZoneXYZ = ctor.Invoke(new object[] {}) as XYZSelector;
                }
            }
        };

        public override bool shouldBeEnabled()
        {
            return builder.GetModule<Kernelmethod_CrungleMode_CrungleGamemodeModule>().IsDataValid();
        }

        public override bool shouldBeEditable()
        {
            return builder.IsEditableGameMode();
        }

        public void SelectBiome(string biome)
        {
            var data = new Kernelmethod_CrungleMode_BiomeGameModuleData {
                Biome = biome,
            };
            setData(data);
            builder.advanceToSummary();
        }

        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
        {
            LogInfo("handling boot event " + id);

            if (id == QudGameBootModule.BOOTEVENT_AFTERINITIALIZEHISTORY)
                return HandleAfterInitializeHistory(game, info, element);
            if (id == QudGameBootModule.BOOTEVENT_BOOTSTARTINGLOCATION && base.data != null)
                return HandleBootStartingLocation(game, info, element);
            if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT)
                return HandleBeforeBootPlayerObject(game, info, element);
            if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
                return HandleBootPlayerObject(game, info, element);
            if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING)
                return HandleBootGameStarting(game, info, element);

            return base.handleBootEvent(id, game, info, element);
        }

        public object HandleAfterInitializeHistory(XRLGame game, EmbarkInfo info, object element = null)
        {
            Target = GetSubject();

            if (!Target.HasProperName)
            {
                Target.DisplayName = NameMaker.MakeName(Target);
                Target.HasProperName = true;
            }

            if (Options.SpawnWithMakeCamp)
                Target.AddSkill("Survival_Camp");
            if (Options.SpawnWithSprint)
                Target.AddSkill("Tactics_Run");

            game.Player.Body = Target;
            game.PlayerName = Target.DisplayName;

            Target.pBrain.PartyLeader = null;
            Target.pBrain.Goals.Clear();
            Target.CurrentZone.SetActive();

            LogInfo($"Subject: {Target.Blueprint}");
            LogInfo($"Zone: {Target.CurrentZone.ZoneID}");
            return base.handleBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT, game, info, element);
        }

        public object HandleBootStartingLocation(XRLGame game, EmbarkInfo info, object element = null)
        {
            string randomDestinationZoneID = SpaceTimeVortex.GetRandomDestinationZoneID("JoppaWorld", Validate: false);
            var cell = Target.CurrentCell;
            var location = $"{Target.CurrentZone.ZoneID}@{cell.X},{cell.Y}";

            return new GlobalLocation(location);
        }

        /// <summary>
        /// Return the initial GameObject instance that should be used for the player object.
        /// </summary>
        public object HandleBeforeBootPlayerObject(XRLGame game, EmbarkInfo info, object element = null)
        {
            Target.RequirePart<Description>().Short = "It's you.";
            Target.pBrain.PartyLeader = null;
            Target.pBrain.Goals.Clear();

            // Remove problematic parts, if the player's body has them
            Target.RemovePart("CryptSitterBehavior");

            // Change faction relationships to reflect current faction affiliation...
            foreach (Faction faction in Factions.loop()) {
                int basePlayerReputation = faction.InitialPlayerReputation;
                int baseplayerFeeling = faction.GetFeelingTowardsObject(Target);

                int newReputation = basePlayerReputation;

                if (baseplayerFeeling < 0 && basePlayerReputation > 0)
                    newReputation = baseplayerFeeling * 6;
                if (baseplayerFeeling > 0 && basePlayerReputation < 0)
                    newReputation = 0;

                if (newReputation != basePlayerReputation) {
                    LogInfo($"HandleBootPlayerObject: changing reputation with {faction.DisplayName} from {basePlayerReputation} to {newReputation}");
                    Faction.PlayerReputation.set(faction, newReputation);
                }
            }

            // ... and then remove the current faction affiliation.
            LogInfo($"HandleBootPlayerObject: removing faction membership (was: {Target.pBrain.Factions})");
            Target.pBrain.FactionMembership.Clear();

            return Target;
        }

        public object HandleBootPlayerObject(XRLGame game, EmbarkInfo info, object element = null)
        {
            var player = element as GameObject;

            player.CurrentZone.SetActive();
            player.AddPart(new Kernelmethod_CrungleMode_CrungleStory());
            player.SetStringProperty("Subtype", "");

            if (Options.SpawnWithWater) {
                int numWaterskins = Kernelmethod_CrungleMode_Random.Next(3, 4);

                // Fill the first two waterskins with a random amount of water; remaining waterskins
                // should be empty.
                for (int i = 0; i < numWaterskins; i++) {
                    GameObject waterskin = GameObject.create("Waterskin");
                    LiquidVolume liquidVolume = waterskin.GetPart("LiquidVolume") as LiquidVolume;

                    if (liquidVolume == null)
                        goto AddObject;

                    liquidVolume.ComponentLiquids.Clear();

                    if (i < 2) {
                        liquidVolume.InitialLiquid = "water";
                        liquidVolume.Volume = Kernelmethod_CrungleMode_Random.Next(24, 32);
                    }

                    AddObject:
                    player.Inventory.AddObject(waterskin);
                }
            }

            if (Options.SpawnWithLightSources)
                RequireLightSource(player);

            return base.handleBootEvent(QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT, game, info, element);
        }

        public void RequireLightSource(GameObject Object)
        {
            var slots = Object.Body.GetPart("Floating Nearby");

            // Don't do anything if the player already has night vision
            if (Object.HasPart(typeof(DarkVision))) {
                LogInfo("RequireLightSource: skipping provisioning (has DarkVision)");
                return;
            }

            // Try to give the player some torches, if they can be equipped
            if (Object.Body.HasPart("Hand"))
            {
                LogInfo("RequireLightSource: provisioning player with torches");
                Object.ReceiveObject("Torch", Kernelmethod_CrungleMode_Random.Next(11, 14));
                Object.pBrain.PerformReequip(Silent: true, DoPrimaryChoice: false);
                return;
            }

            // Give the player a floating glowsphere if they have a floating nearby slot
            if (slots.Count > 0)
            {
                LogInfo("RequireLightSource: provisioning player with glowsphere");
                GameObject glowsphere = GameObject.create("Floating Glowsphere");
                Object.ForceEquipObject(glowsphere, slots[0], Silent: true);
                return;
            }

            LogInfo("RequireLightSource: not provisioning player with light source");
        }

        public object HandleBootGameStarting(XRLGame game, EmbarkInfo info, object element = null)
        {
            var renderPart = Target.GetPart("Render") as Render;
            renderPart.HFlip = true;

            Target.RemovePart("OpeningStory");

            return null;
        }

        /// <summary>
        /// Return the sampling weight for a randomly selected creature.
        /// </summary>
        public int GetObjectWeight(GameObject Object)
        {
            if (!Object.IsCombatObject())
                return 0;
            if (!Object.IsMobile())
                return 0;
            if (!Object.HasStat("XP"))
                return 0;
            if (Object.HasEffect(typeof(WakingDream)) || Object.HasEffect(typeof(DeepDream)))
                return 0;
            if (Options.NoAquatic && Object.HasPart("Aquatic"))
                return 0;
            if (Options.NoLivesOnWalls && Object.HasTag("LivesOnWalls"))
                return 0;

            if (Object.HasTag("Kernelmethod_CrungleMode_SamplingWeight")) {
                var tag = Object.GetTag("Kernelmethod_CrungleMode_SamplingWeight");
                if (!int.TryParse(tag, out var result))
                    throw new Exception("Kernelmethod_CrungleMode::GetObjectWeight: unable to parse Kernelmethod_CrungleMode_SamplingWeight tag: " + tag);

                return result;
            }

            return 100;
        }

        /// <summary>
        /// Pick a random entity whose body should be taken over.
        /// </summary>
        public GameObject GetSubject()
        {
            BallBag<GameObject> ballBag = new BallBag<GameObject>();

            BiomeData biome = biomes[data.Biome];
            foreach (string randomDestinationZoneID in StartingBiome.GetRandomDestinationZoneIDs(biome, 15))
            {
                if (The.ZoneManager.IsZoneLive(randomDestinationZoneID))
                {
                    continue;
                }
                Zone zone = The.ZoneManager.GetZone(randomDestinationZoneID);
                ballBag.Clear();
                ballBag.AddRange(zone.YieldObjects(), GetObjectWeight);
                if (ballBag.Count != 0)
                {
                    int num = ballBag.TotalWeight / ballBag.Count;
                    if (num.ChanceIn(100))
                        break;
                }
            }
            The.ZoneManager.Tick(bAllowFreeze: true);
            return ballBag.PeekOne();
        }

        private static void LogInfo(string message) {
            MetricsManager.LogInfo($"Kernelmethod_CrungleMode::Kernelmethod_CrungleMode_BiomeGameModule: {message}");
        }
    }
}
