using System;
using XRL.World;

namespace Kernelmethod.IronMan
{
    [Serializable]
    public abstract class AbstractSavePart : IPart
    {
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
    }
}