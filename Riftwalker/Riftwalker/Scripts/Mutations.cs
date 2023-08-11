using Kernelmethod.Riftwalker.Utilities;

using System;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Kernelmethod_Riftwalker_EscapeRift : BaseMutation
    {
        public GameObjectReference escapeRift = null;

        public Kernelmethod_Riftwalker_EscapeRift()
        {
            DisplayName = "Escape Rift";
            base.Type = "Mental";
        }

        public override string GetDescription()
        {
            return "You tear the spacetime fabric at your location, sending you to places unknown.";
        }

        public override string GetLevelText(int Level)
        {
            string text = "Summon a vortex at your current location.\n";
            int cooldown = Math.Max(150 - 10 * Level, 20);
            text += "Cooldown: {{rules|" + cooldown + "}} rounds\n";
            text += "The vortex pulls you through to a random location in Qud.\n";
            text += "+100 reputation with {{w|highly entropic beings}}";
            return text;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
            {
                return ID == GetItemElementsEvent.ID;
            }
            return true;
        }

        public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
        {
            if (E.Distance <= 5 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
                E.Add("CommandEscapeRift");

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetItemElementsEvent E)
        {
            E.Add("chance", 2);
            E.Add("time", 1);
            E.Add("travel", 1);
            return base.HandleEvent(E);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "CommandEscapeRift");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandEscapeRift")
            {
                Zone currentZone = ParentObject.CurrentZone;
                if (currentZone == null)
                {
                    return false;
                }
                if (currentZone.IsWorldMap())
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("You may not use this mutation on the world map.");
                    }
                    return false;
                }
                Cell cell = The.PlayerCell;
                if (cell == null)
                {
                    return false;
                }

                Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
                if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
                {
                    return false;
                }
                int turns = Math.Max(160 - 15 * base.Level, 10);
                CooldownMyActivatedAbility(ActivatedAbilityID, turns);
                UseEnergy(0, "Mental Mutation EscapeRift");
                Vortex(cell);
            }
            return base.FireEvent(E);
        }

        public void Vortex(Cell C)
        {
            if (C != null)
            {
                GameObject gameObject = GameObject.create("Space-Time Vortex");
                Temporary temporary = gameObject.GetPart("Temporary") as Temporary;
                temporary.Duration = Kernelmethod_Riftwalker_Random.Next(6, 8);
                C.AddObject(gameObject);
                escapeRift = gameObject.takeReference();
            }
        }

        public void CloseVortex()
        {
            if (escapeRift != null)
            {
                GameObject vortex = escapeRift.go();
                if (vortex != null)
                {
                    vortex.Obliterate();
                }

                escapeRift.free();
                escapeRift = null;
            }
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilityID = AddMyActivatedAbility("Escape Rift", "CommandEscapeRift", "Mental Mutation", null, "\u0015", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.Unmutate(GO);
        }
    }
}
