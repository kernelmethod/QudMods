using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XRL.CharacterBuilds.Qud;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

using Kernelmethod.ChooseYourFighter;
using Kernelmethod.ChooseYourFighter.UI;

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
                Id = "Tile",
                Prefix = "Tile: ",
                Description = (base.module.data?.model?.ColorizedName ?? "{{W|default}}")
            };
        }

        /// <summary>
        /// Randomize the choice of player tile.
        /// </summary>
        public override void RandomSelection()
        {
            FrameworkDataElement dataAt = base.prefabComponent.scrollContext.GetDataAt(base.prefabComponent.scrollContext.selectedPosition);

            if (dataAt?.Id == "Tile")
                base.module.data.model = base.module.Models.GetRandomElement();

            UpdateUI();
        }

        public async void SelectMenuOption(FrameworkDataElement dataElement)
        {
            if (dataElement.Id == "Tile") {
                var icon = TileMenu.MenuIconCharacterCreation(base.module);
                var model = await TileMenu.ChooseTileMenuAsync(Icon: icon);

                if (model != null)
                    base.module.data.model = (model.Id == null) ? null : model;
            }

            UpdateUI();
        }

        public override UIBreadcrumb GetBreadcrumb()
        {
            var tilePath = TileMenu.MenuIconCharacterCreation(base.module)?
                .getTile()
                ?? "items/sw_mask.bmp";

            return new UIBreadcrumb
            {
                Id = GetType().FullName,
                Title = "Player tile",
                IconPath = tilePath,
                IconDetailColor = Color.clear,
                IconForegroundColor = The.Color.Gray
            };
        }
    }
}
