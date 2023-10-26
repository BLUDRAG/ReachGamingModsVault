using HarmonyLib;
using RagsToRiches.Script.Data;
using RagsToRiches.Scripts.GameInteractions;

namespace RagsToRiches.Harmony
{
    [HarmonyPatch(typeof(BlockSleepingBag))]
    [HarmonyPatch("PlaceBlock")]
    public class BlockSleepingBagPatch
    {
        public static bool Prefix()
        {
            if(GameInteractions.HasBuff(BuffStates.AT_HOME)) return true;
            return false;
        }
    }
}