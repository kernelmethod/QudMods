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
            return "{{W|Select player model}}";
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
                    Intro: "{{W|Choose an option to see available character tiles.}}",
                    Hotkeys: MainMenuHotkeys(),
                    AllowEscape: true,
                    IntroIcon: MenuIcon(),
                    centerIntro: true
                );

                if (num == 0)
                    model = GetModelFromBlueprint();
                else if (num == 1)
                    model = ChooseTileMenuWithCategory(ModelType.CasteOrCalling);
                else if (num == 2)
                    model = ChooseTileMenuWithCategory(ModelType.Preset);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        Popup.Show("You don't have any expansions installed for Choose Your Fighter.");
                        continue;
                    }
                    model = ChooseTileMenuWithCategory(ModelType.Expansion);
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
                    Intro: "{{W|Choose an option to see available character tiles.}}",
                    Hotkeys: MainMenuHotkeys().ToArray(),
                    AllowEscape: true,
                    IntroIcon: MenuIconGameStart(module),
                    centerIntro: true
                );

                if (num == 0) {
                    model = await GetModelFromBlueprintAsync();

                }
                else if (num == 1)
                    model = await ChooseTileMenuWithCategoryAsync(module, ModelType.CasteOrCalling);
                else if (num == 2)
                    model = await ChooseTileMenuWithCategoryAsync(module, ModelType.Preset);
                else if (num == 3) {
                    if (!TileFactory.HasExpansionModels()) {
                        await Popup.ShowAsync("You don't have any expansions installed for Choose Your Fighter.", LogMessage: false);
                        continue;
                    }
                    model = await ChooseTileMenuWithCategoryAsync(module, ModelType.Expansion);
                }
                else if (num == 4)
                    // Model will automatically be set to null
                    break;
                else
                    break;
            }

            return model;
        }

        public static PlayerModel ChooseTileMenuWithCategory(ModelType category) {
            var models = new List<PlayerModel>(TileFactory.ModelsFromCategory(category));
            models.Sort();

            var names = models.Select((PlayerModel m) => m.Name);
            var icons = models.Select((PlayerModel m) => m.Icon());

            int num = Popup.ShowOptionList(
                MenuTitle(),
                names.ToArray(),
                Intro: "{{W|Choose a tile for your character from the list below.}}",
                AllowEscape: true,
                Icons: icons.ToArray(),
                IntroIcon: MenuIcon(),
                centerIntro: true
            );

            if (num < 0)
                return null;

            return models[num];
        }

        public static async Task<PlayerModel> ChooseTileMenuWithCategoryAsync(Kernelmethod_ChooseYourFighter_PlayerModelModule module, ModelType category) {
            var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category));
            models.Sort();

            var names = models.Select((PlayerModel m) => m.Name);
            var icons = models.Select((PlayerModel m) => m.Icon());

            int num = await Popup.ShowOptionListAsync(
                MenuTitle(),
                names.ToArray(),
                Intro: "{{W|Choose a tile for your character from the list below.}}",
                AllowEscape: true,
                Icons: icons.ToArray(),
                IntroIcon: MenuIconGameStart(module),
                centerIntro: true
            );

            if (num < 0)
                return null;

            return models[num];
        }

        public static PlayerModel GetModelFromBlueprint() {
            var input = Popup.AskString("Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
            var blueprint = GameObjectFactory.Factory.GetBlueprintIfExists(input);

            if (blueprint == null)
                return null;

            var gameObject = blueprint.createOne();
            if (gameObject.GetTile() == null) {
                Popup.ShowFail($"No tile could be found for the blueprint {input}");
                return null;
            }

            var model = new PlayerModel(blueprint);
            model.Id = "BLUEPRINT:" + input;
            model.HFlip = true;
            return model;
        }

        public static async Task<PlayerModel> GetModelFromBlueprintAsync() {
            var input = await Popup.AskStringAsync(
                "Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false
            );
            var blueprint = GameObjectFactory.Factory.GetBlueprintIfExists(input);

            if (blueprint == null)
                return null;

            var gameObject = blueprint.createOne();
            if (gameObject.GetTile() == null) {
                await Popup.ShowAsync($"No tile could be found for the blueprint {input}", LogMessage: false);
                return null;
            }

            var model = new PlayerModel(blueprint);
            model.Id = "BLUEPRINT:" + input;
            model.HFlip = true;
            return model;
        }
    }
}