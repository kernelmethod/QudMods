using System;
using XRL;
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

            The.Game.QuickSave();

            Exit:
            return base.HandleEvent(E);
        }
    }
}