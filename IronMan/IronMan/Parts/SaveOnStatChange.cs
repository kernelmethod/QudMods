using XRL.World;

namespace Kernelmethod.IronMan {
    /// <summary>
    /// Custom part that triggers a save when one of the player's stats decreases.
    /// </summary>
    public class SaveOnStatChange : AbstractSavePart {
        public override bool AllowStaticRegistration() => true;

        public override void Register(GameObject obj) {
            obj.RegisterPartEvent(this, "StatChange_Strength");
            obj.RegisterPartEvent(this, "StatChange_Agility");
            obj.RegisterPartEvent(this, "statChange_Toughness");
            obj.RegisterPartEvent(this, "StatChange_Willpower");
            obj.RegisterPartEvent(this, "StatChange_Intelligence");
            obj.RegisterPartEvent(this, "StatChange_Ego");
            obj.RegisterPartEvent(this, "StatChange_Hitpoints");
            base.Register(obj);
        }

        public override bool FireEvent(Event E) {
            if (!E.ID.StartsWith("StatChange_"))
                goto Exit;

            // We only trigger a save on certain stat changes
            switch (E.ID) {
                case "StatChange_Strength":
                case "StatChange_Agility":
                case "StatChange_Toughness":
                case "StatChange_Willpower":
                case "StatChange_Intelligence":
                case "StatChange_Ego":
                case "StatChange_Hitpoints":
                    // Only save if the base value decreased
                    int oldBaseValue = E.GetIntParameter("OldBaseValue");
                    int newBaseValue = E.GetIntParameter("NewBaseValue");

                    if (newBaseValue >= oldBaseValue)
                        break;

                    TriggerSave();
                    break;
                default:
                    // Do nothing
                    break;
            }

            Exit:
            return base.FireEvent(E);
        }
    }
}