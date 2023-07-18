using ConsoleLib.Console;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI;
using XRL.UI.Framework;
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

            public Pet(string id, string name, IRenderable icon, string description)
            {
                Id = id;
                Name = name;
                Icon = icon;
                Description = description;
            }
        }

        static IEnumerable<Pet> GetPets()
        {
            foreach (GameObjectBlueprint item in GameObjectFactory.Factory.GetBlueprintsWithTag("StartingPet"))
            {
                string id = item.Name;
                string name = item.GetTag("PetName", item.DisplayName());
                IRenderable icon = new Renderable(item);
                string description = item.GetPartParameter<string>("Kernelmethod_BetterPetSelector", "Description");

                yield return new Pet(id, name, icon, description);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(QudCustomizeCharacterModuleWindow.OnChoosePet))]
        static bool OnChoosePetPrefix(QudCustomizeCharacterModuleWindow __instance, Task __result)
        {
            __result = OnChoosePet(__instance);
            return false;
        }

        static async Task OnChoosePet(QudCustomizeCharacterModuleWindow window)
        {
            var availablePets = new List<Pet>(GetPets());
            if (availablePets.Count == 0)
                return;

            availablePets.Insert(0, new Pet(null, "<none>", null, null));
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

        static async Task OnChoosePetOriginal(QudCustomizeCharacterModuleWindow window)
        {
            List<FrameworkDataElement> availablePets = new List<FrameworkDataElement>(window.GetPets());
            if (availablePets.Count == 0)
                return;

            availablePets.Insert(0, new FrameworkDataElement
            {
                Id = null,
                Description = "<none>"
            });

            var petNames = availablePets.Select((FrameworkDataElement o) => o.Description);

            int num = await Popup.ShowOptionListAsync("Choose Pet", availablePets.Select((FrameworkDataElement o) => o.Description).ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
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
