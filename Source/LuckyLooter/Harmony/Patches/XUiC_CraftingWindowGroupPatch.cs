using HarmonyLib;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(XUiC_CraftingWindowGroup))]
    public class XUiC_CraftingWindowGroupPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnOpen")]
        public static bool OnOpenPrefix()
        {
            return false;
        }
    }
}