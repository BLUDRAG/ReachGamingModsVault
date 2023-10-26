using HarmonyLib;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(XUiM_Trader))]
    public class XUiM_TraderPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetSellPrice")]
        public static void GetSellPricePostfix(ref int __result)
        {
            if(__result <= 0)
            {
                __result = 1;
            }
        }
    }
}