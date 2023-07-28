using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI
{
    [UIView(
        "CharacterCreation:Kernelmethod_CrungleMode:Biome",
        WantsTileOver = false,
        ForceFullscreen = false,
        IgnoreForceFullscreen = false,
        TakesScroll = false,
        ForceFullscreenInLegacy = false,
        NavCategory = "Menu",
        UICanvas = "Chargen/PickSubtype",
        UICanvasHost = 1
    )]
    public class Kernelmethod_CrungleMode_BiomeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<Kernelmethod_CrungleMode_BiomeGameModule, HorizontalScroller>
    {
        public IEnumerable<ChoiceWithColorIcon> GetSelections()
        {
            var values = base.module.biomes.Values;
            foreach (Kernelmethod_CrungleMode_BiomeGameModule.BiomeData item in values)
            {
                yield return new ChoiceWithColorIcon {
                    Id = item.Name,
                    Title = item.Name,
                    IconPath = item.Tile,
                    IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Detail[0]],
                    IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Foreground[0]],
                    Description = item.Description,
                };
            }
        }

        public void onSelectBiome(FrameworkDataElement choice)
        {
            base.module.SelectBiome(choice.Id);
        }

        public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
        {
            base.prefabComponent.autoHotkey = true;
            base.prefabComponent.onSelected.RemoveAllListeners();
            base.prefabComponent.onSelected.AddListener(onSelectBiome);
            base.prefabComponent.BeforeShow(descriptor, GetSelections());
            base.BeforeShow(descriptor);
        }

        public override IEnumerable<MenuOption> GetKeyLegend()
        {
            foreach (MenuOption item in base.GetKeyLegend())
                yield return item;
        }

        public override UIBreadcrumb GetBreadcrumb()
        {
            UIBreadcrumb obj = new UIBreadcrumb {
                Id = GetType().FullName,
                Title = "Biome"  
            };

            string iconPath = UIBreadcrumb.DEFAULT_ICON;
            if (base.module?.data?.Biome != null)
            {
                var biomeModule = base.module;
                if (biomeModule != null && biomeModule.biomes.ContainsKey(biomeModule.data?.Biome))
                {
                    iconPath = biomeModule.biomes[biomeModule.data?.Biome]?.Tile;
                }
            }

            obj.IconPath = iconPath;
            obj.IconDetailColor = Color.clear;
            obj.IconForegroundColor = The.Color.Gray;
            return obj;
        }
    }
}