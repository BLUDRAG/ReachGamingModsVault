using System;

namespace RagsToRiches.Scripts.Data
{
    [Serializable]
    public sealed class TrespassingData
    {
        public string Prefab;
        public ulong  TrespassingTime;
        public int    Warnings;
    }
}