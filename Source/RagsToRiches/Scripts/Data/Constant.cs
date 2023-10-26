using System.Collections.Generic;
using RagsToRiches.Script.Data;

namespace RagsToRiches.Scripts.Data
{
    public static class Constant
    {
        public static readonly Vector2i TrespassingHours          = new Vector2i(7, 21);
        public const           ulong    TrespassingLimit          = 1000;
        public const           ulong    TrespassDataSaveFrequency = 100;
        public const           float    SellingPriceMarkdownRate  = 0.5f;
        public const           float    ChunkBlocksAdjustment    = 250000f;
        
        public static readonly Dictionary<BuffStates, string> BuffToKeys = new Dictionary<BuffStates, string>
                                                                           {
                                                                               {BuffStates.AT_HOME, "At Home"},
                                                                               {BuffStates.SQUATTING, "Squatting"},
                                                                               {BuffStates.TRESPASSING, "Trespassing"},
                                                                           };
        
        public static string AtHomeBuff      => BuffToKeys[BuffStates.AT_HOME];
        public static string SquattingBuff   => BuffToKeys[BuffStates.SQUATTING];
        public static string TrespassingBuff => BuffToKeys[BuffStates.TRESPASSING];
    }
}