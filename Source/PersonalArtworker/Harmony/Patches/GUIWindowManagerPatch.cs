using HarmonyLib;
using PersonalArtworker.Scripts.GameInteractions;

namespace PersonalArtworker.Harmony.Patches
{
    [HarmonyPatch(typeof(GUIWindowManager))]
    public class GUIWindowManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("IsModalWindowOpen")]
        public static bool Prefix(ref bool __result)
        {
            if(GameInteractions.IsUIInteracting)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}