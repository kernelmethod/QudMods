using ConsoleLib.Console;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI;
using XRL.World;

namespace Kernelmethod.BetterPetSelector.Patches {
    [HarmonyPatch(typeof(QudCustomizeCharacterModuleWindow))]
    public class QudCustomizeCharacterModuleWindowPatches
    {
        public class Pet
        {
            public string Id;
            public string Name;
            public IRenderable Icon;
            public string Description;
        }

        static IEnumerable<Pet> GetPets()
        {
            foreach (GameObjectBlueprint item in GameObjectFactory.Factory.GetBlueprintsWithTag("StartingPet"))
            {
                string id = item.Name;
                string name = item.GetTag("PetName", item.DisplayName());
                string description = item.GetTag("Kernelmethod_BetterPetSelector_Description", null);
                string renderBlueprint = item.GetTag("Kernelmethod_BetterPetSelector_RenderBlueprint", null);

                IRenderable icon;
                if (renderBlueprint == null)
                    icon = new Renderable(item);
                else
                    icon = new Renderable(GameObjectFactory.Factory.GetBlueprint(renderBlueprint));

                yield return new Pet {
                    Id=id,
                    Name=name,
                    Description=description,
                    Icon=icon
                };
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(QudCustomizeCharacterModuleWindow.OnChoosePet))]
        static bool OnChoosePetPrefix(QudCustomizeCharacterModuleWindow __instance, out Task __result)
        {
            __result = OnChoosePet(__instance);
            return false;
        }

        static async Task OnChoosePet(QudCustomizeCharacterModuleWindow window)
        {
            var availablePets = new List<Pet>(GetPets());
            if (availablePets.Count == 0)
                return;

            availablePets.Insert(0, new Pet {
                Id=null,
                Name="<none>",
                Description=null,
                Icon=null
            });
            var descriptions = availablePets.Select((Pet p) => {
                string description = p.Name;
                if (p.Description != null)
                    description += "\n&c" + p.Description;
                return description;
            });
            var icons = availablePets.Select((Pet p) => p.Icon);

            int num = await Popup.ShowOptionListAsync("Choose Pet", descriptions.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, Icons: icons.ToArray());
            if (window.module.data == null)
            {
                window.module.data = new QudCustomizeCharacterModuleData();
            }
            if (num <= 0)
            {
                window.module.data.pet = null;
            }
            else
            {
                window.module.data.pet = availablePets[num].Id;
            }
            window.module.setData(window.module.data);
        }
    }
}
