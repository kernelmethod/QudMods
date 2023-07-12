using System;
using XRL;
using XRL.Core;
using XRL.Rules;

namespace Kernelmethod.Riftwalker.Utilities
{
    /// <summary>
    /// Custom RNG provider for the mod.
    ///
    /// Wiki reference: https://wiki.cavesofqud.com/wiki/Modding:Random_Functions
    /// </summary>
    [HasGameBasedStaticCache]
    public class Kernelmethod_Riftwalker_Random
    {
        private static string GameStateSeedKey = "Kernelmethod_Riftwalker:Random";

        private static Random _rand;
        public static Random Rand
        {
            get
            {
                if (_rand == null)
                {
                    if (XRLCore.Core?.Game == null)
                        throw new Exception("Riftwalker mod attempted to retrieve Random, but game is not created yet.");

                    if (XRLCore.Core.Game.IntGameState.ContainsKey(GameStateSeedKey))
                    {
                        int seed = XRLCore.Core.Game.GetIntGameState(GameStateSeedKey);
                        _rand = new Random(seed);
                    }
                    else
                    {
                        _rand = Stat.GetSeededRandomGenerator("Kernelmethod_Riftwalker");
                    }

                    XRLCore.Core.Game.SetIntGameState(GameStateSeedKey, _rand.Next());
                }

                return _rand;
            }
        }

        [GameBasedCacheInit]
        public static void ResetRandom()
        {
            _rand = null;
        }

        public static int Next(int minInclusive, int maxInclusive)
        {
            return Rand.Next(minInclusive, maxInclusive + 1);
        }
    }
}