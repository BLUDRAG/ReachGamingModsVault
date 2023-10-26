using System.Collections.Generic;
using HarmonyLib;
using Platform;
using UniLinq;
using Enumerable = System.Linq.Enumerable;

namespace FlushTerrainBlocks.Harmony.Patches
{
    [HarmonyPatch(typeof(Block))]
    public class BlockPatch
    {
        private static readonly List<string> _densityBlocks = new List<string>
                                                              {
                                                                  "cube", "cube_glass", "cube_frame", "farm_plot",
                                                              };
        
        private static readonly List<Vector3i> _nonTerrain = new List<Vector3i>
                                                             {
                                                                 Vector3i.forward + Vector3i.left,
                                                                 Vector3i.forward + Vector3i.right,
                                                                 Vector3i.back, Vector3i.left,
                                                                 Vector3i.back, Vector3i.right,
                                                             };
        
        private static readonly List<Vector3i> _terrain = new List<Vector3i>
                                                             {
                                                                 Vector3i.forward, Vector3i.right, 
                                                                 Vector3i.back, Vector3i.left,
                                                             };

        [HarmonyPrefix]
        [HarmonyPatch("PlaceBlock")]
        public static bool Prefix(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea,
                                  string ___blockName, sbyte ___Density)
        {
            Block    block    = _result.blockValue.Block;
            int      clrIdx   = _result.clrIdx;
            Vector3i blockPos = _result.blockPos;

            if(ShouldUpdateDensity(block))
            {
                UpdateBlockDensities(_world, _result, clrIdx, blockPos);
                return false;
            }
            
            if(block.shape.IsTerrain())
            {
                _world.SetBlockRPC(clrIdx, blockPos, _result.blockValue, ___Density);
            }
            else if(!block.IsTerrainDecoration)
            {
                _world.SetBlockRPC(clrIdx, blockPos, _result.blockValue, MarchingCubes.DensityAir);
            }
            else
            {
                _world.SetBlockRPC(clrIdx, blockPos, _result.blockValue);
            }

            if(!___blockName.Equals("keystoneBlock") || !(_ea is EntityPlayerLocal))
            {
                return false;
            }

            PlatformManager.NativePlatform.AchievementManager ?.SetAchievementStat(EnumAchievementDataStat.LandClaimPlaced, 1);

            return false;
        }

        private static void UpdateBlockDensities(WorldBase _world, BlockPlacement.Result _result, int clrIdx, Vector3i blockPos)
        {
            List<BlockChangeInfo> blockUpdates = new List<BlockChangeInfo>();
            sbyte                 density      = sbyte.MinValue;
            bool                  hasTerrain   = Enumerable.Any(_terrain, terrain => IsTerrainBlock(_world, blockPos + terrain));

            if(!hasTerrain)
            {
                if(Enumerable.Any(_nonTerrain, nonTerrain => !IsTerrainBlock(_world, blockPos + nonTerrain)))
                {
                    density = _world.GetDensity(clrIdx, blockPos);
                }
            }

            blockUpdates.Add(new BlockChangeInfo(clrIdx, blockPos, _result.blockValue, density));
            _world.SetBlocksRPC(blockUpdates);
        }

        private static bool IsTerrainBlock(WorldBase world, Vector3i position)
        {
            BlockValue block    = world.GetBlock(position);
            string     meshType = block.Block.Properties.GetString("Mesh");
            return meshType.EqualsCaseInsensitive("terrain");
        }

        private static bool ShouldUpdateDensity(Block block)
        {
            BlockShape shape = block.shape;
            string     name  = shape.GetName();

            return shape.IsSolidCube || _densityBlocks.Any(name.EqualsCaseInsensitive);
        }
    }
}