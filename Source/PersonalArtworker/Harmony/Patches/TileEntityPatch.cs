using HarmonyLib;
using PersonalArtworker.Scripts.Entities;

namespace PersonalArtworker.Harmony.Patches
{
    [HarmonyPatch(typeof(TileEntity))]
    public class TileEntityPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Instantiate")]
        public static bool Prefix(TileEntityType type, Chunk _chunk, ref TileEntity __result)
        {
            if((int)type == ArtworkEntity.TILE_ENTITY_TYPE)
            {
                __result = new ArtworkEntity(_chunk);
                return false;
            }

            return true;
        }
    }
}