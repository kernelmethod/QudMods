using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XRL.CharacterBuilds.Qud;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

using Kernelmethod.ChooseYourFighter;

namespace XRL.CharacterBuilds.Qud.UI {
    [UIView(
        "CharacterCreation:Kernelmethod_ChooseYourFighter:ModelSelector",
        WantsTileOver = false,
        ForceFullscreen = false,
        IgnoreForceFullscreen = false,
        TakesScroll = false,
        ForceFullscreenInLegacy = false,
        NavCategory = "Menu",
        UICanvas = "Chargen/CustomizeCharacter",
        UICanvasHost = 1)
    ]
    public class Kernelmethod_ChooseYourFighter_PlayerModelWindow : EmbarkBuilderModuleWindowPrefabBase<Kernelmethod_ChooseYourFighter_PlayerModelModule, FrameworkScroller>
    {
        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            if (base.module.data == null)
                base.module.data = new PlayerModelData();
            UpdateUI(descriptor);
            base.BeforeShow(descriptor);
        }

        public void UpdateUI(EmbarkBuilderModuleWindowDescriptor descriptor = null)
        {
            base.prefabComponent.BeforeShow(descriptor, GetSelections());
            base.prefabComponent.onSelected.RemoveAllListeners();
            base.prefabComponent.onSelected.AddListener(SelectMenuOption);
        }

        public IEnumerable<PrefixMenuOption> GetSelections()
        {
            yield return new PrefixMenuOption
            {
                Id = "Model",
                Prefix = "Model: ",
                Description = (base.module.data?.model?.Name ?? "{{W|default}}")
            };
        }

        /// <summary>
        /// Randomize the choice of player model.
        /// </summary>
        public override void RandomSelection()
        {
            FrameworkDataElement dataAt = base.prefabComponent.scrollContext.GetDataAt(base.prefabComponent.scrollContext.selectedPosition);

            if (dataAt?.Id == "Model")
                base.module.data.model = base.module.Models.GetRandomElement();

            UpdateUI();
        }

        public async void SelectMenuOption(FrameworkDataElement dataElement)
        {
            if (dataElement.Id == "Model") {
                await OnChooseModel();
            }

            UpdateUI();
        }

        /// <summary>
        /// Create a popup menu to select the player model that should be used.
        /// </summary>
        public async Task OnChooseModel()
        {
            var availableModels = new List<PlayerModel>(base.module.Models);
            availableModels.Sort();
            availableModels.Insert(0, new PlayerModel {
                Id="ENTER_FROM_BLUEPRINT",
                Name="{{W|Choose tile from blueprint}}",
            });
            availableModels.Insert(0, new PlayerModel {
                Id=null,
                Name="{{W|default}}",
            });

            var names = availableModels.Select((PlayerModel m) => m.Name);
            var icons = availableModels.Select((PlayerModel m) => m.Icon());

            int num = await Popup.ShowOptionListAsync("Choose model", names.ToArray(), null, 0, null, 60, Icons: icons.ToArray());

            if (base.module.data.model == null)
                base.module.data.model = new PlayerModel();

            if (num <= 0)
                base.module.data.model = null;
            else if (availableModels[num].Id == "ENTER_FROM_BLUEPRINT")
                await GetModelFromBlueprint();
            else
                base.module.data.model = availableModels[num];

            base.module.setData(base.module.data);
        }

        public async Task GetModelFromBlueprint() {
            var input = await Popup.AskStringAsync("Enter blueprint:", "", 999, 0, null, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
            var blueprint = GameObjectFactory.Factory.GetBlueprintIfExists(input);

            if (blueprint == null) {
                await Popup.ShowAsync($"The blueprint {input} could not be found.");
                return;
            }

            var gameObject = blueprint.createOne();
            if (gameObject.GetTile() == null) {
                await Popup.ShowAsync($"No tile could be found for the blueprint {input}");
                return;
            }

            base.module.data.model = new PlayerModel {
                Id="BLUEPRINT:" + input,
                Name="{{M|" + blueprint.DisplayName() + "}}",
                Tile=gameObject.GetTile(),
                Foreground=gameObject.GetForegroundColor(),
                Background=gameObject.GetBackgroundColor(),
                DetailColor=gameObject.GetDetailColor()
            };
        }

        public override UIBreadcrumb GetBreadcrumb()
        {
            return new UIBreadcrumb
            {
                Id = GetType().FullName,
                Title = "Player model",
                IconPath = "items/sw_mask.bmp",
                IconDetailColor = Color.clear,
                IconForegroundColor = The.Color.Gray
            };
        }
    }
}
