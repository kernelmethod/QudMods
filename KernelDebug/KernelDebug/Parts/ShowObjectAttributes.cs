using Kernelmethod.KernelDebug;
using System;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Parts {
    [Serializable]
    public class Kernelmethod_KernelDebug_ShowObjectAttributes : IPart {
        public override bool WantEvent(int ID, int cascade) {
            return base.WantEvent(ID, cascade)
                || ID == GetShortDescriptionEvent.ID;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E) {
            if (Options.ShowTemperature && ParentObject.TryGetPart<Physics>(out var part)) {
                E.Postfix
                    .Append("\n{{c|Temperature: ")
                    .Append(part.Temperature.ToString())
                    .Append("ø}}");
            }

            return base.HandleEvent(E);
        }
    }
}
