using System;
using System.Collections.Generic;
using System.Reflection;
using Audio;
using RagsToRiches.Script.Data;
using RagsToRiches.Scripts.Actors;
using RagsToRiches.Scripts.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RagsToRiches.Scripts.GameInteractions
{
    public static class GameInteractions
    {
        public static bool Ready => _localPlayer;
        
        private static LocalPlayer            _localPlayer;
        private static DynamicPrefabDecorator _decorator;
        private static FieldInfo              _blockLayersField;
        private static FieldInfo              _blockRefCountField;
        
        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(GetGameInstances);
            Type chunkType = typeof(Chunk);
            Type chunkBlockLayerType = typeof(ChunkBlockLayer);
            _blockLayersField = chunkType.GetField("m_BlockLayers", BindingFlags.Instance | BindingFlags.NonPublic);
            _blockRefCountField = chunkBlockLayerType.GetField("blockRefCount", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void GetGameInstances()
        {
            _localPlayer = Object.FindObjectOfType<LocalPlayer>();
            _decorator   = GameManager.Instance.GetDynamicPrefabDecorator();
        }

        /// <summary>
        /// Spawns a screamer for each trespassing warning.
        /// </summary>
        /// <param name="prefab"></param>
        public static void SummonScreamer(PrefabInstance prefab)
        {
            int trespassingWarnings = DataManagement.TrespassingWarnings(prefab);
            AIDirectorChunkEventComponent aiDirector = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
            Type type = typeof(AIDirectorChunkEventComponent);
            
            MethodInfo findScoutStartPos = type.GetMethod("FindScoutStartPos", BindingFlags.Instance | BindingFlags.NonPublic);

            FieldInfo scoutSpawnListField = type.GetField("scoutSpawnList", BindingFlags.Instance | BindingFlags.NonPublic);
            List<AIScoutHordeSpawner> scoutSpawnList = (List<AIScoutHordeSpawner>)scoutSpawnListField.GetValue(aiDirector);

            for(int i = 0; i < trespassingWarnings; i++)
            {
                SpawnScout(aiDirector, ref scoutSpawnList, findScoutStartPos, _localPlayer.entityPlayerLocal.position);
            }

            scoutSpawnListField.SetValue(aiDirector, scoutSpawnList);
        }

        private static void SpawnScout(AIDirectorChunkEventComponent aiDirector, ref List<AIScoutHordeSpawner> scoutSpawnList, 
                                       MethodInfo findScoutStartPos, Vector3 targetPos)
        {
            object[] parameters = new object[] { _localPlayer.entityPlayerLocal.position, null };
            object result = findScoutStartPos.Invoke(aiDirector, parameters);
            
            if((bool)result)
            {
                EntitySpawner scoutSpawner = new EntitySpawner("Scouts", Vector3i.zero, Vector3i.zero, 0);
                scoutSpawnList.Add(new AIScoutHordeSpawner(scoutSpawner, (Vector3)parameters[1], targetPos, true));
            }
        }

        public static PrefabInstance GetPrefabNearPlayer()
        {
            PrefabInstance prefab = _decorator.GetPrefabAtPosition(_localPlayer.entityPlayerLocal.position);
            return !PrefabValid(prefab) ? null : prefab;
        }

        private static bool PrefabValid(PrefabInstance prefab)
        {
            if(prefab is null) return false;
            if(prefab.prefab.HasQuestTag()) return true;
            
            List<string> tags = prefab.prefab.Tags.GetTagNames();
            if(tags.Count == 0) return false;
            if(tags.Contains("part")) return false;
            if(tags.Contains("biomeonly")) return false;
            if(tags.Contains("navonly")) return false;
            if(tags.Contains("trader")) return false;

            return true;
        }

        public static bool UIOpen()
        {
            bool UIOpen = _localPlayer.playerUI.windowManager.IsModalWindowOpen();
            return UIOpen && !_localPlayer.playerUI.windowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID);
        }

        /// <summary>
        /// Buys the current prefab if the player can afford it.
        /// </summary>
        public static void BuyCurrentPrefab()
        {
            Manager.PlayButtonClick();
            PrefabInstance currentPrefab = GetPrefabNearPlayer();
            if(currentPrefab == null) return;
            if(!FinanceManagement.PlayerCanAfford(currentPrefab)) return;
            FinanceManagement.Buy(currentPrefab);
            CanvasActor.UpdateCanvasState(currentPrefab);
        }
        
        /// <summary>
        /// Sells the current prefab if the player owns it.
        /// </summary>
        public static void SellCurrentPrefab()
        {
            Manager.PlayButtonClick();
            PrefabInstance currentPrefab = GetPrefabNearPlayer();
            if(currentPrefab == null) return;
            FinanceManagement.Sell(currentPrefab);
            CanvasActor.UpdateCanvasState(currentPrefab);
        }

        public static void UpdateBuffState(BuffStates state)
        {
            switch (state)
            {
                case BuffStates.NONE:
                    RemoveBuff(Constant.AtHomeBuff);
                    RemoveBuff(Constant.SquattingBuff);
                    RemoveBuff(Constant.TrespassingBuff);
                    break;
                case BuffStates.AT_HOME:
                    AddBuff(Constant.AtHomeBuff);
                    RemoveBuff(Constant.SquattingBuff);
                    RemoveBuff(Constant.TrespassingBuff);
                    break;
                case BuffStates.SQUATTING:
                    RemoveBuff(Constant.AtHomeBuff);
                    AddBuff(Constant.SquattingBuff);
                    RemoveBuff(Constant.TrespassingBuff);
                    break;
                case BuffStates.TRESPASSING:
                    RemoveBuff(Constant.AtHomeBuff);
                    RemoveBuff(Constant.SquattingBuff);
                    AddBuff(Constant.TrespassingBuff);
                    break;
            }
        }
        
        private static void AddBuff(string buffName)
        {
            EntityBuffs playerBuffs = _localPlayer.entityPlayerLocal.Buffs;
            if(!playerBuffs.HasBuff(buffName)) playerBuffs.AddBuff(buffName);
        }
        
        private static void RemoveBuff(string buffName)
        {
            EntityBuffs playerBuffs = _localPlayer.entityPlayerLocal.Buffs;
            if(playerBuffs.HasBuff(buffName)) playerBuffs.RemoveBuff(buffName);
        }

        public static bool HasBuff(BuffStates state)
        {
            EntityBuffs playerBuffs = _localPlayer.entityPlayerLocal.Buffs;
            return playerBuffs.HasBuff(Constant.BuffToKeys[state]);
        }

        public static int GetTotalChunkBlocks(PrefabInstance prefab)
        {
            int totalBlocks = 0;

            foreach(long occupiedChunk in prefab.GetOccupiedChunks())
            {
                Chunk chunk = GameManager.Instance.World.GetChunkSync(occupiedChunk) as Chunk;
                if(chunk is null) continue;
                
                ChunkBlockLayer[] m_BlockLayers = (ChunkBlockLayer[])_blockLayersField.GetValue(chunk);
                if(m_BlockLayers is null) continue;

                foreach(ChunkBlockLayer blockLayer in m_BlockLayers)
                {
                    if(blockLayer is null) continue;
                    object blockCount = _blockRefCountField.GetValue(blockLayer);
                    if(blockCount is null) continue;
                    totalBlocks += (int)blockCount;
                }
            }

            return totalBlocks;
        }

        public static float GetBiomeLootModifier(PrefabInstance prefab)
        {
            Vector3i        position = prefab.boundingBoxPosition;
            BiomeDefinition biome    = GameManager.Instance.World.GetBiome(position.x, position.z);
            return 1f + (biome?.LootStageMod ?? 0);
        }
    }
}