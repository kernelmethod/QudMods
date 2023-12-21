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
            if (dataElement.Id == "Model")
                base.module.data.model = await TileMenu.ChooseTileMenuAsync(base.module);

            UpdateUI();
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
