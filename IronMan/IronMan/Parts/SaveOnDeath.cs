using System;
using XRL.World;

namespace Kernelmethod.IronMan {
    [Serializable]
    public class SaveOnDeath : AbstractSavePart
    {
        public SaveOnDeath() {}

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == AfterDieEvent.ID;
        }

        public override bool HandleEvent(AfterDieEvent E)
        {
            if (!ParentObject.IsPlayer())
                goto Exit;

            TriggerSave();

            Exit:
            return base.HandleEvent(E);
        }
    }
}