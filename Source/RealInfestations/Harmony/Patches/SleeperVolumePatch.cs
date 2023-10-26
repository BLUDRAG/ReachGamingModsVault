using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
using RealInfestations.Scripts.Data;
using UnityEngine;

namespace RealInfestations.Harmony.Patches
{
  [HarmonyPatch(typeof(SleeperVolume))]
  public class SleeperVolumePatch
  {
    private static ConfigData _configData;

    public static void Init(string modPath)
    {
      string configPath = Path.Combine(modPath, "Config", "config.json");

      if(!File.Exists(configPath))
      {
        _configData = new ConfigData();
        Directory.CreateDirectory(Path.Combine(modPath, "Config"));
        File.WriteAllText(configPath, JsonConvert.SerializeObject(_configData, Formatting.Indented));
        return;
      }

      _configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configPath));
    }

    [HarmonyPostfix]
    [HarmonyPatch("Spawn")]
    public static void SpawnPostfix(World _world,
                                    int entityClass,
                                    int spawnIndex,
                                    GameStageGroup group,
                                    BlockSleeper block,
                                    GameStageGroup ___overrideGroup, GameStageGroup ___spawnGroup,
                                    List<int> ___countList, ref int ___numSpawned, int ___gameStage,
                                    List<SleeperVolume.SpawnPoint> ___spawnPointList,
                                    int ___lastClassId, PrefabInstance ___prefabInstance)
    {
      if(___prefabInstance is null || ___prefabInstance.LastQuestClass is null || !___prefabInstance.LastRefreshType.Test_AnySet(QuestEventManager.infestedTag))
      {
        return;
      }

      GameRandom sleeperRandom = (GameRandom)AccessTools.Field(typeof(SleeperVolume), "sleeperRandom").GetValue(null);

      int questScaling = _configData.ScaleWithQuest ? ___prefabInstance.LastQuestClass.QuestStage : 0;
      int extraZombies = _configData.InfestationZombieSpawnMultiplier + questScaling;
      
      for(int i = 0; i < extraZombies; i++)
      {
        Spawn(_world,            ___overrideGroup, ___spawnGroup, ___countList, ___numSpawned, ___gameStage,
              ___spawnPointList, ___lastClassId,   sleeperRandom);
      }
    }

    private static void Spawn(World _world, GameStageGroup ___overrideGroup, GameStageGroup ___spawnGroup, 
                              List<int> ___countList, int ___numSpawned, int ___gameStage,
                              List<SleeperVolume.SpawnPoint> ___spawnPointList, int ___lastClassId,
                              GameRandom sleeperRandom)
    {
      string cultureInvariantString = Time.time.ToCultureInvariantString();

      GameStageGroup         group   = ___overrideGroup ?? ___spawnGroup;
      GameStageGroup.Spawner spawner = new GameStageGroup.Spawner();
      int                    num3    = 0;

      for(int index = 0; index < ___countList.Count; ++index)
      {
        num3 += ___countList[index];

        if(num3 > ___numSpawned)
        {
          spawner = group.spawners[index];

          break;
        }
      }

      if(spawner.stageDefinition != null)
      {
        GameStageDefinition.Stage stage = spawner.stageDefinition.GetStage(___gameStage);

        if(stage != null)
        {
          int spawnIndex = Random.Range(0, ___spawnPointList.Count);

          if(spawnIndex >= 0)
          {
            SleeperVolume.SpawnPoint spawnPoint = ___spawnPointList[spawnIndex];
            BlockSleeper             block      = spawnPoint.GetBlock();

            if(block == null)
            {
              Log.Error("{0} BlockSleeper {1} null, type {2}", cultureInvariantString, spawnPoint.pos,
                        spawnPoint.blockType);
            }
            else
            {
              string _sEntityGroupName = block.spawnGroup;

              if(string.IsNullOrEmpty(_sEntityGroupName))
                _sEntityGroupName = stage.GetSpawnGroup(0).groupName;

              int randomFromGroup = EntityGroups.GetRandomFromGroup(_sEntityGroupName, ref ___lastClassId,
                                                                    sleeperRandom);

              Vector3 vector3 = spawnPoint.pos.ToVector3();
              vector3.x += 0.502f;
              vector3.z += 0.501f;

              if(!EntityClass.list.TryGetValue(randomFromGroup, out EntityClass _entityClass))
              {
                Log.Warning("Spawn class {0} is missing", randomFromGroup);
                randomFromGroup = EntityClass.FromString("zombieArlene");
              }
              else if(block.ExcludesWalkType(EntityAlive.GetSpawnWalkType(_entityClass)))
              {
                Log.Warning("Spawn {0} can't walk on block {1} with walkType {2}",
                            _entityClass.entityClassName, block,
                            EntityAlive.GetSpawnWalkType(_entityClass));

                return;
              }

              EntityAlive entity = (EntityAlive)EntityFactory.CreateEntity(randomFromGroup, vector3,
                                                                           new Vector3(0.0f, spawnPoint.rot, 0.0f));

              if(!entity)
              {
                Log.Error("Spawn class {0} is null", randomFromGroup);

                return;
              }

              entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
              _world.SpawnEntityInWorld(entity);
            }
          }
        }
      }
    }
  }
}