using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BossRaids.Harmony.Patches;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BossRaids.Scripts
{
    public class GameInteractions
    {
        private static LocalPlayer                   _localPlayer;
        private static AIDirectorChunkEventComponent _aiDirector;
        private static Type                          _aiDirectorType;
        private static MethodInfo                    _findScoutStartPos;
        private static FieldInfo                     _scoutSpawnListField;
        private static FieldInfo                     _entityIdSpawnedFieldInfo;
        
        private const int    BASE_HEALTH        = 5000;
        private const int    HEALTH_MULTIPLIER  = 30;
        private const string BOSS_SCREAMER_NAME = "ScreamerRaidBoss";

        private static string[] BOSS_NAMES =
        {
            BOSS_SCREAMER_NAME
        };

        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(GetGameInstances);
            ModEvents.GameStartDone.RegisterHandler(PrepareReflectedBindings);
        }

        private static void GetGameInstances()
        {
            _localPlayer = Object.FindObjectOfType<LocalPlayer>();
            _aiDirector  = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
        }

        private static void PrepareReflectedBindings()
        {
            _entityIdSpawnedFieldInfo = typeof(EntitySpawner).GetField("entityIdSpawned", BindingFlags.Instance | BindingFlags.NonPublic);
            _aiDirectorType = typeof(AIDirectorChunkEventComponent);
            _findScoutStartPos = _aiDirectorType.GetMethod("FindScoutStartPos", BindingFlags.Instance | BindingFlags.NonPublic);
            _scoutSpawnListField = _aiDirectorType.GetField("scoutSpawnList", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static EntitySpawner CreateRandomBossSpawner()
        {
            List<AIScoutHordeSpawner> scoutSpawnList = (List<AIScoutHordeSpawner>)_scoutSpawnListField.GetValue(_aiDirector);

            EntitySpawner spawner = CreateBossSpawner(BOSS_NAMES[Random.Range(0, BOSS_NAMES.Length)], _aiDirector,
                                                  ref scoutSpawnList,                             _findScoutStartPos,
                                                  _localPlayer.entityPlayerLocal.position);
            _scoutSpawnListField.SetValue(_aiDirector, scoutSpawnList);
            return spawner;
        }

        private static EntitySpawner CreateBossSpawner(string boss, AIDirectorChunkEventComponent aiDirector, ref List<AIScoutHordeSpawner> scoutSpawnList, 
                                                       MethodInfo findScoutStartPos, Vector3 targetPos)
        {
            object[] parameters = new object[] { _localPlayer.entityPlayerLocal.position, null };
            object result = findScoutStartPos.Invoke(aiDirector, parameters);
            
            if((bool)result)
            {
                EntitySpawner scoutSpawner = new EntitySpawner(boss, Vector3i.zero, Vector3i.zero, 0);
                scoutSpawnList.Add(new AIScoutHordeSpawner(scoutSpawner, (Vector3)parameters[1], targetPos, true));
                return scoutSpawner;
            }

            return null;
        }

        public static IEnumerator SpawnRandomBoss()
        {
            EntitySpawner eSpawner  = CreateRandomBossSpawner();
            PList<int>    entityIds = (PList<int>)_entityIdSpawnedFieldInfo.GetValue(eSpawner);

            while(entityIds is null || entityIds.Count == 0) yield return null;
            EntityAlive bossScreamer = GameManager.Instance.World.Entities.dict[entityIds[0]].GetComponent<EntityAlive>();

            float health = BASE_HEALTH + (_localPlayer.entityPlayerLocal.gameStage * HEALTH_MULTIPLIER);
            bossScreamer.Stats.Health.BaseMax     = health;
            bossScreamer.Stats.Health.OriginalMax = health;
            bossScreamer.Stats.Health.MaxModifier = health;
            bossScreamer.Stats.Health.Value       = health;
            bossScreamer.Stats.Health.Changed     = true;

            NetPackage package = NetPackageManager
                                .GetPackage<NetPackageEntityStatChanged>()
                                .Setup(bossScreamer,
                                       _localPlayer.entityPlayerLocal.entityId,
                                       NetPackageEntityStatChanged.EnumStat.Health);

            GameManager.Instance.World.entityDistributer
                       .SendPacketToTrackedPlayersAndTrackedEntity(bossScreamer.entityId, -1, package);

            XUiC_TargetBarPatch.SetBossGroup(_localPlayer.entityPlayerLocal, bossScreamer);
        }
    }
}