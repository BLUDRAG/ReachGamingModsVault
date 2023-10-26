using HarmonyLib;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(ItemClass))]
    public class ItemClassPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void InitPostfix(ItemClass __instance)
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

        [HarmonyPostfix]
        [HarmonyPatch("AutoCalcEcoVal")]
        public static void AutoCalcEcoValPostfix(ItemClass __instance)
        {
            if(__instance.EconomicValue <= 0f)
            {
                __instance.EconomicValue = 1f;
            }
        }
    }
}