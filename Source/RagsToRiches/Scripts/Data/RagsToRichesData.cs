using System;
using System.Collections.Generic;

namespace RagsToRiches.Scripts.Data
{
    [Serializable]
    public sealed class RagsToRichesData
    {
        public List<string>          BoughtPrefabs   = new List<string>();
        public List<SquattingData>   SquattingData   = new List<SquattingData>();
        public List<TrespassingData> TrespassingData = new List<TrespassingData>();
    }
}