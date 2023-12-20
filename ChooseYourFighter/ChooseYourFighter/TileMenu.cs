using ConsoleLib.Console;
using XRL.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XRL.World;

namespace Kernelmethod.ChooseYourFighter {
    public static class TileMenu {
        public static IRenderable MenuIcon() {
            var Icon = new Renderable();
            Icon.Tile = "items/sw_mask_kesil.bmp";
            Icon.ColorString = "&M";
            Icon.DetailColor = 'K';
            return Icon;
        }

        public static string MenuTitle() {
            return "{{W|Select player model}}";
        }

        /// <summary>
        /// Create a menu for the player to change their appearance.
        /// </summary>
        public static PlayerModel ChooseTileMenu() {
            PlayerModel model = null;

            while (model == null) {
                var options = new List<string> {
                    "{{W|Choose tile from blueprint}}",
                    "Castes and callings",
                    "Presets"
                };

                if (TileFactory.HasExpansionModels()) {
                    options.Add("Expansions");
                }

                int num = Popup.ShowOptionList(
                    MenuTitle(),
                    options.ToArray(),
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
                else if (num == 3)
                    model = ChooseTileMenuWithCategory(ModelType.Expansion);
                else
                    break;

                MetricsManager.LogInfo($"model = {model}");
            }

            return model;
        }

        public static async Task<PlayerModel> ChooseTileMenuAsync() {
            PlayerModel model = null;

            while (model == null) {
                var options = new List<string> {
                    "{{W|Choose tile from blueprint}}",
                    "Castes and callings",
                    "Presets"
                };

                if (TileFactory.HasExpansionModels()) {
                    options.Add("Expansions");
                }

                int num = await Popup.ShowOptionListAsync(
                    MenuTitle(),
                    options.ToArray(),
                    AllowEscape: true,
                    IntroIcon: MenuIcon(),
                    centerIntro: true
                );

                if (num == 0) {
                    model = await GetModelFromBlueprintAsync();

                }
                else if (num == 1)
                    model = await ChooseTileMenuWithCategoryAsync(ModelType.CasteOrCalling);
                else if (num == 2)
                    model = await ChooseTileMenuWithCategoryAsync(ModelType.Preset);
                else if (num == 3)
                    model = await ChooseTileMenuWithCategoryAsync(ModelType.Expansion);
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
                AllowEscape: true,
                Icons: icons.ToArray(),
                IntroIcon: MenuIcon(),
                centerIntro: true
            );

            if (num < 0)
                return null;

            return models[num];
        }

        public static async Task<PlayerModel> ChooseTileMenuWithCategoryAsync(ModelType category) {
            var models = new List<PlayerModel>(TileFactory.Models.Where(m => m.Category == category));
            models.Sort();

            var names = models.Select((PlayerModel m) => m.Name);
            var icons = models.Select((PlayerModel m) => m.Icon());

            int num = await Popup.ShowOptionListAsync(
                MenuTitle(),
                names.ToArray(),
                AllowEscape: true,
                Icons: icons.ToArray(),
                IntroIcon: MenuIcon(),
                centerIntro: true
            );

            if (num < 0)
                return null;

            return models[num];
        }

        public static PlayerModel GetModelFromBlueprint() {
            var input = Popup.AskString("Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
            var blueprint = GameObjectFactory.Factory.GetBlueprintIfExists(input);

            if (blueprint == null) {
                Popup.ShowFail($"The blueprint {input} could not be found.");
                return null;
            }

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

            if (blueprint == null) {
                await Popup.ShowAsync($"The blueprint {input} could not be found.", LogMessage: false);
                return null;
            }

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