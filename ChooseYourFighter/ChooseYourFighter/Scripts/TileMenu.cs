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

        public static IRenderable MenuIcon(GameObject Object) {
            return Object?.RenderForUI() ?? null;
        }

        public static string MenuTitle() {
            return "{{Y|Select player model}}";
        }

        public static List<string> MainMenuOptions(GameObject Object) {
            var options = new List<string> {
                "Enter blueprint ID for tile",
                "Castes and callings",
                "Presets",
            };

            if (TileFactory.HasExpansionModels())
                options.Add("Expansions");
            else
                options.Add("{{K|Expansions}}");

            if (Object?.HasPart(typeof(DefaultModel)) ?? true)
                options.Add("Reset to default");
            else
                options.Add("{{K|Reset to default}}");

            return options;
        }

        public static List<char> MainMenuHotkeys() {
            return new List<char> { 'b', 'c', 'p', 'x', 'd' };
        }

        /// <summary>
        /// Create a menu for the player to change the appearance of themselves or another object.
        /// </summary>
        public static PlayerModel ChooseTileMenu(GameObject Object) {
            PlayerModel model = null;
            bool RequireDefault = true;

            while (model == null) {
                int num = Popup.ShowOptionList(
                    MenuTitle(),
                    MainMenuOptions(Object).ToArray(),
                    Intro: "Choose an option to see available character tiles.",
                    Hotkeys: MainMenuHotkeys(),
                    AllowEscape: true,
                    IntroIcon: MenuIcon(Object),
                    centerIntro: true
                );

                if (num == 0)
                    model = GetModelFromBlueprint();
                else if (num == 1)
                    model = ChooseTileMenuFiltered(Object, ModelType.CasteOrCalling);
                else if (num == 2)
                    model = ChooseTileMenuFiltered(Object, ModelType.Preset);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        Popup.Show("You don't have any expansions enabled for Choose Your Fighter.");
                        continue;
                    }
                    model = ChooseTileMenuFiltered(Object, ModelType.Expansion);
                }
                else if (num == 4) {
                    DefaultModel defaultModel = null;
                    if (Object?.TryGetPart<DefaultModel>(out defaultModel) ?? false) {
                        model = defaultModel.Model;
                        Object.RemovePart<DefaultModel>();
                        RequireDefault = false;
                    }
                    else
                        Popup.Show("You are already using your character's original model.", LogMessage: false);
                }
                else
                    break;

                MetricsManager.LogInfo($"model = {model}");
            }

            TileFactory.ChangeAppearance(Object, model, RequireDefault);
            return model;
        }

        /// <summary>
        /// Player model selector for character creation.
        ///
        /// Returns `null` if no option was selected, and returns a `PlayerModel` with `model.Id == null`
        /// if "default" was selected.
        /// </summary>
        public static async Task<PlayerModel> CharacterCreationChooseTileMenu(Kernelmethod_ChooseYourFighter_PlayerModelModule module) {
            PlayerModel model = null;

            while (model == null) {
                int num = await Popup.ShowOptionListAsync(
                    MenuTitle(),
                    MainMenuOptions(null).ToArray(),
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
                    // Model ID will automatically be set to null
                    model = new PlayerModel();
                else
                    break;
            }

            return model;
        }

        public static PlayerModel ChooseTileMenuFiltered(GameObject Object, ModelType category, string Group = null) {
            while (true) {
                var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category && m.Group == Group));
                models.Sort();

                var names = models.Select((PlayerModel m) => m.ColorizedName);
                var icons = models.Select((PlayerModel m) => m.Icon());

                int num = Popup.ShowOptionList(
                    MenuTitle(),
                    names.ToArray(),
                    Intro: "Choose a tile for your character from the list below.",
                    AllowEscape: true,
                    Icons: icons.ToArray(),
                    IntroIcon: MenuIcon(Object),
                    centerIntro: true
                );

                if (num < 0)
                    return null;

                var choice = models[num];
                if (!choice.IsGroup)
                    return choice;

                choice = ChooseTileMenuFiltered(Object, category, Group: choice.Id);

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

                var names = models.Select((PlayerModel m) => m.ColorizedName);
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
