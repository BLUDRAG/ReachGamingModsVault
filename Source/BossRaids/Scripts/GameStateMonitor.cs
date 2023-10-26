using System;
using System.Reflection;
using BossRaids.Scripts.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BossRaids.Scripts
{
    public class GameStateMonitor : MonoBehaviour
    {
        private static bool  _gameReady;
        private const  float CHANCE_TO_SPAWN_BOSS = 0.05f;

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


            if(_gameReady && Input.GetKeyDown(KeyCode.Period))
            {
                LocalPlayer _localPlayer = Object.FindObjectOfType<LocalPlayer>();
                Transform window = _localPlayer.playerUI.nguiWindowManager.GetWindow(EnumNGUIWindow.MainMenuBackground);

                ForAllTransforms(window, (t) =>
                                         {
                                             foreach(Component component in t.GetComponents<Component>())
                                             {
                                                 Log.Warning($"{component.GetType().Name}");
                                             }

                                             UIPanel p;
                                             Log.Warning("=============");
                                         });
            }
        }

        public static void ForAllTransforms(Transform transform, Action<Transform> action)
        {
            action(transform);
            ForAllChildren(transform, action);
        }

        public static void ForAllChildren(Transform transform, Action<Transform> action)
        {
            foreach(Transform child in transform)
            {
                action(child);
                ForAllTransforms(child, action);
            }
        }
        
        private void ShouldSpawnRaidBoss()
        {
            (int day, int hour, int minute) = DataManagement.GetCurrentSpawnTime();
            int currentDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int currentHour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
            int currentMinute = GameUtils.WorldTimeToMinutes(GameManager.Instance.World.worldTime);
            if(currentDay < day || (currentHour < hour && currentMinute < minute)) return;
            
            if(Random.value <= CHANCE_TO_SPAWN_BOSS) StartCoroutine(GameInteractions.SpawnRandomBoss());
            DataManagement.CalculateNextSpawnTime();
        }
    }
}