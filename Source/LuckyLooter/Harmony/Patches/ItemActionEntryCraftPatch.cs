using HarmonyLib;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(ItemActionEntryCraft))]
    public class ItemActionEntryCraftPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnActivated")]
        public static bool OnActivatedPrefix()
        {
            return false;
        }
    }
}