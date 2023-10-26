using HarmonyLib;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(Block))]
    public class BlockPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void InitPostfix(Block __instance)
        {
            if(__instance.EconomicValue <= 0f)
            {
                __instance.EconomicValue = 1f;
            }

            if(__instance.EconomicBundleSize > 1)
            {
                __instance.EconomicBundleSize = 1;
            }

            __instance.SellableToTrader = true;
        }
    }
}