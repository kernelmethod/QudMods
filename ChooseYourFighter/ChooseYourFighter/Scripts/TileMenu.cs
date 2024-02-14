using ConsoleLib.Console;
using XRL.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XRL;
using XRL.CharacterBuilds.Qud;
using XRL.World;

namespace Kernelmethod.ChooseYourFighter {
    public static class TileMenu {
        public static IRenderable MenuIconGameStart(Kernelmethod_ChooseYourFighter_PlayerModelModule module) {
            if (module.data.model != null)
                return module.data.model.Icon();

            var builder = module.builder;
            var tile = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE, null);
            var fgColor = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND, null) ?? "&Y";
            var bgColor = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEBACKGROUND, null);
            var detailColor = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL, null);

            if (tile == null)
                return null;

            var Icon = new Renderable();
            Icon.Tile = tile;
            Icon.ColorString = fgColor;
            Icon.DetailColor = detailColor[0];

            return Icon;
        }

        public static IRenderable MenuIcon() {
            return The.Player?.RenderForUI() ?? null;
        }

        public static string MenuTitle() {
            return "{{Y|Select player model}}";
        }

        public static List<string> MainMenuOptions() {
            var options = new List<string> {
                "Enter blueprint ID for tile",
                "Castes and callings",
                "Presets",
            };

            if (TileFactory.HasExpansionModels())
                options.Add("Expansions");
            else
                options.Add("{{K|Expansions}}");

            if (The.Player?.HasPart(typeof(DefaultModel)) ?? true)
                options.Add("Reset to default");
            else
                options.Add("{{K|Reset to default}}");

            return options;
        }

        public static List<char> MainMenuHotkeys() {
            return new List<char> { 'b', 'c', 'p', 'x', 'd' };
        }

        /// <summary>
        /// Create a menu for the player to change their appearance.
        /// </summary>
        public static PlayerModel ChooseTileMenu() {
            PlayerModel model = null;
            bool RequireDefault = true;

            while (model == null) {
                int num = Popup.ShowOptionList(
                    MenuTitle(),
                    MainMenuOptions().ToArray(),
                    Intro: "Choose an option to see available character tiles.",
                    Hotkeys: MainMenuHotkeys(),
                    AllowEscape: true,
                    IntroIcon: MenuIcon(),
                    centerIntro: true
                );

                if (num == 0)
                    model = GetModelFromBlueprint();
                else if (num == 1)
                    model = ChooseTileMenuFiltered(ModelType.CasteOrCalling);
                else if (num == 2)
                    model = ChooseTileMenuFiltered(ModelType.Preset);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        Popup.Show("You don't have any expansions enabled for Choose Your Fighter.");
                        continue;
                    }
                    model = ChooseTileMenuFiltered(ModelType.Expansion);
                }
                else if (num == 4) {
                    DefaultModel defaultModel = null;
                    if (The.Player?.TryGetPart<DefaultModel>(out defaultModel) ?? false) {
                        model = defaultModel.Model;
                        The.Player.RemovePart<DefaultModel>();
                        RequireDefault = false;
                    }
                    else
                        Popup.Show("You are already using your character's original model.", LogMessage: false);
                }
                else
                    break;

                MetricsManager.LogInfo($"model = {model}");
            }

            TileFactory.ChangePlayerAppearance(model, RequireDefault);
            return model;
        }

        public static async Task<PlayerModel> ChooseTileMenuAsync(Kernelmethod_ChooseYourFighter_PlayerModelModule module) {
            PlayerModel model = null;

            while (model == null) {
                int num = await Popup.ShowOptionListAsync(
                    MenuTitle(),
                    MainMenuOptions().ToArray(),
                    Intro: "Choose an option to see available character tiles.",
                    Hotkeys: MainMenuHotkeys().ToArray(),
                    AllowEscape: true,
                    IntroIcon: MenuIconGameStart(module),
                    centerIntro: true
                );

                if (num == 0) {
                    model = await GetModelFromBlueprintAsync();

                }
                else if (num == 1)
                    model = await ChooseTileMenuFilteredAsync(module, ModelType.CasteOrCalling);
                else if (num == 2)
                    model = await ChooseTileMenuFilteredAsync(module, ModelType.Preset);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        await Popup.ShowAsync("You don't have any expansions installed for Choose Your Fighter.", LogMessage: false);
                        continue;
                    }
                    model = await ChooseTileMenuFilteredAsync(module, ModelType.Expansion);
                }
                else if (num == 4)
                    // Model will automatically be set to null
                    break;
                else
                    break;
            }

            return model;
        }

        public static PlayerModel ChooseTileMenuFiltered(ModelType category, string Group = null) {
            while (true) {
                var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category && m.Group == Group));
                models.Sort();

                var names = new List<string>(GetChoiceNames(models));
                var icons = models.Select((PlayerModel m) => m.Icon());

                int num = Popup.ShowOptionList(
                    MenuTitle(),
                    names.ToArray(),
                    Intro: "Choose a tile for your character from the list below.",
                    AllowEscape: true,
                    Icons: icons.ToArray(),
                    IntroIcon: MenuIcon(),
                    centerIntro: true
                );

                if (num < 0)
                    return null;

                var choice = models[num];
                if (!choice.IsGroup)
                    return choice;

                choice = ChooseTileMenuFiltered(category, Group: choice.Id);

                if (choice != null)
                    return choice;
            }
        }

        public static async Task<PlayerModel> ChooseTileMenuFilteredAsync(
            Kernelmethod_ChooseYourFighter_PlayerModelModule module,
            ModelType category,
            string Group = null
        ) {
            while (true) {
                var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category && m.Group == Group));
                models.Sort();

                var names = new List<string>(GetChoiceNames(models));
                var icons = models.Select((PlayerModel m) => m.Icon());

                int num = await Popup.ShowOptionListAsync(
                    MenuTitle(),
                    names.ToArray(),
                    Intro: "Choose a tile for your character from the list below.",
                    AllowEscape: true,
                    Icons: icons.ToArray(),
                    IntroIcon: MenuIconGameStart(module),
                    centerIntro: true
                );

                if (num < 0)
                    return null;

                var choice = models[num];
                if (!choice.IsGroup)
                    return choice;

                choice = await ChooseTileMenuFilteredAsync(module, category, Group: choice.Id);

                if (choice != null)
                    return choice;
            }
        }

        public static PlayerModel GetModelFromBlueprint() {
            var input = Popup.AskString("Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
            var blueprint = WishSearcher.SearchForBlueprint(input).Result;

            if (blueprint == null)
                return null;

            var model = GetModelFromBlueprintName(blueprint);
            if (model == null) {
                Popup.ShowFail($"No tile could be found for the blueprint {input}");
            }

            return model;
        }

        public static async Task<PlayerModel> GetModelFromBlueprintAsync() {
            var input = await Popup.AskStringAsync(
                "Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false
            );
            var blueprint = WishSearcher.SearchForBlueprint(input).Result;

            if (blueprint == null)
                return null;

            var model = GetModelFromBlueprintName(blueprint);
            if (model == null) {
                await Popup.ShowAsync($"No tile could be found for the blueprint {input}", LogMessage: false);
            }

            return model;
        }

        private static IEnumerable<string> GetChoiceNames(List<PlayerModel> Choices) {
            foreach (var choice in Choices) {
                var choiceName = choice.Name;

                // Groups are colorized yellow by default
                // Tiles are colorized magenta by default
                if (choice.IsGroup && !ColorUtility.HasFormatting(choiceName)) {
                    choiceName = ColorUtility.ApplyColor(choiceName, "W");
                }
                else if (!choice.IsGroup && !ColorUtility.HasFormatting(choiceName)) {
                    choiceName = ColorUtility.ApplyColor(choiceName, "m");
                }

                yield return choiceName;
            }
        }

        private static PlayerModel GetModelFromBlueprintName(string Blueprint) {
            var gameObject = GameObjectFactory.Factory.CreateObject(Blueprint);
            if (gameObject.GetTile() == null)
                return null;

            var model = new PlayerModel(gameObject.GetBlueprint());
            model.Id = "BLUEPRINT:" + Blueprint;
            model.HFlip = true;
            return model;
        }
    }
}
