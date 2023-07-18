using System;

namespace XRL.World.Parts
{
    [Serializable]
    public class Kernelmethod_BetterPetSelector : IPart
    {
        public string _Description = null;

        public string Description
        {
            get
            {
                return "&c" + _Description;
            }
            set
            {
                _Description = value;
            }
        }
    }
}