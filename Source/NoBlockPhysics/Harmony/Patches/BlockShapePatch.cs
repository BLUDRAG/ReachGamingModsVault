using HarmonyLib;

namespace NoBlockPhysics.Harmony.Patches
{
    [HarmonyPatch(typeof(BlockShape))]
    public class BlockShapePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BlockShape), "Init")]
        public static void InitPostfix(Block _block)
        {
            _block.StabilityIgnore = true;
        }
    }
}