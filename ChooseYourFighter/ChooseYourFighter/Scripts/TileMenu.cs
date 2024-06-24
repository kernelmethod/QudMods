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
        public static IRenderable MenuIconCharacterCreation(Kernelmethod_ChooseYourFighter_PlayerModelModule module) {
            if (module.data?.model != null)
                return module.data.model.Icon();

            var builder = module.builder;
            var tile = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE, null);
            var fgColor = builder.fireBootEvent<string>(QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEFOREGROUND, null) ?? "&y";
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

        public static string MenuTitle() {
            return "{{Y|Select player tile}}";
        }

        public static List<string> MainMenuOptions(PlayerModel Default = null) {
            var options = new List<string> {
                "Enter blueprint ID for tile",
                "Castes and callings",
                "Presets",
            };

            if (TileFactory.HasExpansionModels())
                options.Add("Expansions");
            else
                options.Add("{{K|Expansions}}");

            options.Add(Default != null ? "Reset to default" : "{{K|Reset to default}}");
            return options;
        }

        public static List<char> MainMenuHotkeys() {
            return new List<char> { 'b', 'c', 'p', 'x', 'd' };
        }

        /// <summary>
        /// Create a menu for the player to select between themselves or a follower for a change
        /// of appearance.
        /// </summary>
        public static void ChangeAppearanceMenu() {
            GameObject obj = null;

            var objects = The.Player.GetCompanionsReadonly();
            if (objects.Count == 0) {
                obj = The.Player;
            }
            else {
                objects.Insert(0, The.Player);

                int selection = Popup.PickOption(
                    Title: "Whose appearance would you like to change?",
                    Options: objects.Select((GameObject o) => o.DisplayName).ToArray(),
                    Icons: objects.Select((GameObject o) => o.RenderForUI()).ToArray(),
                    AllowEscape: true,
                    CenterIntro: true
                );

                if (selection == -1)
                    return;

                obj = objects[selection];
            }

            if (obj == null)
                return;

            var model = ChooseTileMenu(
                Icon: obj.RenderForUI(),
                Default: obj.GetPart<DefaultModel>()?.Model
            );
            TileFactory.ChangeAppearance(obj, model);
        }

        /// <summary>
        /// Create a menu for the player to change the appearance of themselves or another object.
        /// </summary>
        public static PlayerModel ChooseTileMenu(IRenderable Icon = null, PlayerModel Default = null) {
            PlayerModel model = null;

            while (model == null) {
                int num = Popup.PickOption(
                    Title: MenuTitle(),
                    Options: MainMenuOptions(Default).ToArray(),
                    Intro: "Choose an option to see available character tiles.",
                    Hotkeys: MainMenuHotkeys(),
                    AllowEscape: true,
                    IntroIcon: Icon,
                    CenterIntro: true
                );

                if (num == 0)
                    model = GetModelFromBlueprint();
                else if (num == 1)
                    model = ChooseTileMenuFiltered(ModelType.CasteOrCalling, Icon: Icon);
                else if (num == 2)
                    model = ChooseTileMenuFiltered(ModelType.Preset, Icon: Icon);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        Popup.Show("You don't have any expansions enabled for Choose Your Fighter.");
                        continue;
                    }
                    model = ChooseTileMenuFiltered(ModelType.Expansion, Icon: Icon);
                }
                else if (num == 4) {
                    if (Default != null)
                        model = Default;
                    else
                        Popup.Show("You are already using your character's original tile.", LogMessage: false);
                }
                else
                    break;
            }

            return model;
        }

        /// <summary>
        /// Player model selector for character creation.
        ///
        /// Returns `null` if no option was selected, and returns a `PlayerModel` with `model.Id == null`
        /// if "default" was selected.
        /// </summary>
        public static async Task<PlayerModel> CharacterCreationChooseTileMenuAsync(IRenderable Icon = null, PlayerModel Default = null) {
            PlayerModel model = null;

            while (model == null) {
                int num = await Popup.PickOptionAsync(
                    Title: MenuTitle(),
                    Options: MainMenuOptions(Default).ToArray(),
                    Intro: "Choose an option to see available character tiles.",
                    Hotkeys: MainMenuHotkeys().ToArray(),
                    AllowEscape: true,
                    IntroIcon: Icon,
                    CenterIntro: true
                );

                if (num == 0) {
                    model = await GetModelFromBlueprintAsync();

                }
                else if (num == 1)
                    model = await ChooseTileMenuFilteredAsync(ModelType.CasteOrCalling, Icon: Icon);
                else if (num == 2)
                    model = await ChooseTileMenuFilteredAsync(ModelType.Preset, Icon: Icon);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        await Popup.ShowAsync("You don't have any expansions installed for Choose Your Fighter.", LogMessage: false);
                        continue;
                    }
                    model = await ChooseTileMenuFilteredAsync(ModelType.Expansion, Icon: Icon);
                }
                else if (num == 4)
                    // Model ID will automatically be set to null
                    model = new PlayerModel();
                else
                    break;
            }

            return model;
        }

        public static PlayerModel ChooseTileMenuFiltered(ModelType category, IRenderable Icon = null, string Group = null) {
            while (true) {
                var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category && m.Group == Group));
                models.Sort();

                var names = models.Select((PlayerModel m) => m.ColorizedName);
                var icons = models.Select((PlayerModel m) => m.Icon());

                int num = Popup.PickOption(
                    Title: MenuTitle(),
                    Options: names.ToArray(),
                    Intro: "Choose a tile for your character from the list below.",
                    AllowEscape: true,
                    Icons: icons.ToArray(),
                    IntroIcon: Icon,
                    CenterIntro: true
                );

                if (num < 0)
                    return null;

                var choice = models[num];
                if (!choice.IsGroup)
                    return choice;

                choice = ChooseTileMenuFiltered(category, Icon: Icon, Group: choice.Id);

                if (choice != null)
                    return choice;
            }
        }

        public static async Task<PlayerModel> ChooseTileMenuFilteredAsync(
            ModelType category,
            IRenderable Icon = null,
            string Group = null
        ) {
            while (true) {
                var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category && m.Group == Group));
                models.Sort();

                var names = models.Select((PlayerModel m) => m.ColorizedName);
                var icons = models.Select((PlayerModel m) => m.Icon());

                int num = await Popup.PickOptionAsync(
                    Title: MenuTitle(),
                    Options: names.ToArray(),
                    Intro: "Choose a tile for your character from the list below.",
                    AllowEscape: true,
                    Icons: icons.ToArray(),
                    IntroIcon: Icon,
                    CenterIntro: true
                );

                if (num < 0)
                    return null;

                var choice = models[num];
                if (!choice.IsGroup)
                    return choice;

                choice = await ChooseTileMenuFilteredAsync(category, Icon: Icon, Group: choice.Id);

                if (choice != null)
                    return choice;
            }
        }

        public static PlayerModel GetModelFromBlueprint() {
            var input = Popup.AskString(
                "Enter blueprint:",
                Default: "",
                MinLength: 0,
                MaxLength: 999,
                ReturnNullForEscape: true,
                EscapeNonMarkupFormatting: true,
                AllowColorize: false
            );
            if (input.IsNullOrEmpty())
                return null;

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
                "Enter blueprint:",
                Default: "",
                MinLength: 0,
                MaxLength: 999,
                ReturnNullForEscape: true,
                EscapeNonMarkupFormatting: true,
                AllowColorize: false
            );
            if (input.IsNullOrEmpty())
                return null;

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
