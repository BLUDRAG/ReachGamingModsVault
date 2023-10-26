using System.Reflection;
using BossRaids.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BossRaids.Scripts
{
    public class GameStateMonitor : MonoBehaviour
    {
        private static bool _gameReady;

        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(SetGameReady);
            ModEvents.SavePlayerData.RegisterHandler(SetGameNotReady);
            GameObject gameStateMonitor = new GameObject($"{Assembly.GetExecutingAssembly().FullName} - Game State Monitor");
            DontDestroyOnLoad(gameStateMonitor);
            gameStateMonitor.AddComponent<GameStateMonitor>();
        }

        private static void SetGameReady()
        {
            _gameReady = true;
        }

        private static void SetGameNotReady(ClientInfo clientInfo, PlayerDataFile playerDataFile)
        {
            _gameReady = false;
        }
        
        private void Update()
        {
            if(!_gameReady || !GameManager.Instance.gameStateManager.IsGameStarted()) return;
            ShouldSpawnRaidBoss();
        }
        
        private void ShouldSpawnRaidBoss()
        {
            (int day, int hour, int minute) = DataManagement.GetCurrentSpawnTime();
            int currentDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int currentHour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
            int currentMinute = GameUtils.WorldTimeToMinutes(GameManager.Instance.World.worldTime);
            if(currentDay < day || (currentHour < hour && currentMinute < minute)) return;

            if(Random.value <= DataManagement.GetSpawnChance())
            {
                StartCoroutine(GameInteractions.SpawnRandomBoss());
                DataManagement.ResetSpawnChance();
            }
            else
            {
                DataManagement.IncrementSpawnChance();
            }

            DataManagement.CalculateNextSpawnTime();
        }
    }
}