using HarmonyLib;

namespace ReachSlimTrees.Harmony.Patches
{
    [HarmonyPatch(typeof(BlockModelTree))]
    public class BlockModelTreePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CanPlaceBlockAt")]
        public static void CanPlaceBlockAtPrefix(ref bool __result)
        {
            __result = true;
        }
    }
}