using System;

namespace XRL.World.Parts {
    /// <summary>
    /// Small part that gets added to creatures whose tile needs to point to the right
    /// rather than the left.
    /// </summary>
    [Serializable]
    [Obsolete("will be removed after the next breaking update")]
    public class Kernelmethod_ChooseYourFighter_FlipTile : IPart {
        public override bool Render(RenderEvent E) {
            if (The.Player.Body == ParentObject.Body)
                E.HFlip = true;

            return base.Render(E);
        }
    }
}
