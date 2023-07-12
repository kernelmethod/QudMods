using XRL.UI;

namespace XRL.World.WorldBuilders
{
    /// <summary>
    /// World builder extension for the Riftwalker mod.
    ///
    /// The purpose of this extension is to figure out what options are enabled at the time of
    /// worldgen, and store those in the game state. This will allow the mod to figure out what
    /// content to enable or disable.
    /// </summary>
    [WorldBuilderExtension]
    public class Kernelmethod_Riftwalker_WorldBuilderExtension : IWorldBuilderExtension
    {
        public override void OnBeforeBuild(string world, object builder)
        {
            // Set the game state based on what options are enabled.
            The.Game.SetBooleanGameState("Kernelmethod_Riftwalker_EnableItems", Kernelmethod_Riftwalker_Options.EnableItems);

            base.OnBeforeBuild(world, builder);
        }
    }
}