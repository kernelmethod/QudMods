using System;
using XRL;
using XRL.Core;
using XRL.World;

namespace Kernelmethod.IronMan
{
    [Serializable]
    public abstract class AbstractSavePart : IPart
    {
        public virtual long MinTurnsBetweenSaves => 0;
        public long LastSaveTurn = -1;

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == AfterPlayerBodyChangeEvent.ID;
        }

        public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
        {
            Type bodyType = E.NewBody.GetType();
            var baseMethod = bodyType.GetMethod("RequirePart", new Type[] {false.GetType()});
            var typedForThis = baseMethod.MakeGenericMethod(this.GetType());
            typedForThis.Invoke(E.NewBody, new object[] {false});

            return base.HandleEvent(E);
        }
        public void TriggerSave() {
            if (MinTurnsBetweenSaves > 0 && XRLCore.CurrentTurn - MinTurnsBetweenSaves < LastSaveTurn && LastSaveTurn > 0)
                return;

            LastSaveTurn = XRLCore.CurrentTurn;
            The.Game.QuickSave();
        }
    }
}