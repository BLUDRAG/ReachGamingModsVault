using HarmonyLib;
using RagsToRiches.Script.Data;
using RagsToRiches.Scripts.GameInteractions;

namespace RagsToRiches.Harmony
{
    [HarmonyPatch(typeof(BlockLandClaim))]
    [HarmonyPatch("PlaceBlock")]
    public class BlockLandClaimPatch
    {
        public static bool Prefix()
        {
            if(GameInteractions.HasBuff(BuffStates.AT_HOME)) return true;
            return false;
        }
    }
}