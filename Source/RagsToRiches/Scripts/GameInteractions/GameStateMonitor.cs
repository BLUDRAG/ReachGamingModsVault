using RagsToRiches.Script.Data;
using RagsToRiches.Scripts.Data;
using RagsToRiches.Scripts.Actors;
using UnityEngine;

namespace RagsToRiches.Scripts.GameInteractions
{
    public sealed class GameStateMonitor : MonoBehaviour
    {
        private static ulong _lastWorldTime;
        private static bool  _gameReady;

        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(SetGameReady);
            ModEvents.SavePlayerData.RegisterHandler(SetGameNotReady);
            GameObject gameStateMonitor = new GameObject("Game State Monitor");
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

        public void Update()
        {
            if(!_gameReady || !GameManager.Instance.gameStateManager.IsGameStarted() || !GameInteractions.Ready) return;
            
            PrefabInstance prefab      = GameInteractions.GetPrefabNearPlayer();
            bool           foundPrefab = !(prefab is null);
            bool           ownsPrefab  = foundPrefab && DataManagement.HasBought(prefab);

            MonitorUIState(foundPrefab, prefab);
            MonitorOwnershipState(prefab, foundPrefab, ownsPrefab);
        }

        private static void MonitorUIState(bool foundPrefab, PrefabInstance prefab)
        {
            bool UIOpen = GameInteractions.UIOpen();
            if(foundPrefab && UIOpen) CanvasActor.Open(prefab);
            else CanvasActor.Close();
        }

        private static void MonitorOwnershipState(PrefabInstance prefab, bool foundPrefab, bool ownsPrefab)
        {
            if(!foundPrefab)
            {
                GameInteractions.UpdateBuffState(BuffStates.NONE);
                return;
            }

            if(!ownsPrefab) MonitorTrespassingState(prefab);
            else GameInteractions.UpdateBuffState(BuffStates.AT_HOME);
        }

        /// <summary>
        /// Monitors the player's trespassing state and summons screamers if they've been trespassing for too long.
        /// </summary>
        /// <param name="prefab"></param>
        private static void MonitorTrespassingState(PrefabInstance prefab)
        {
            ulong worldTime = GameManager.Instance.World.worldTime;
            int   hour      = GameUtils.WorldTimeToHours(worldTime);

            // Ignore daytime
            if(hour > Constant.TrespassingHours.x && hour < Constant.TrespassingHours.y)
            {
                GameInteractions.UpdateBuffState(BuffStates.NONE);
                return;
            }

            if(_lastWorldTime == worldTime) return;

            // Ignore squatting
            int day = GameUtils.WorldTimeToDays(worldTime);

            if(DataManagement.IsSquatting(prefab, day, hour))
            {
                GameInteractions.UpdateBuffState(BuffStates.SQUATTING);
                return;
            }

            // Summon an escalating set of screamers every hour of trespassing.
            GameInteractions.UpdateBuffState(BuffStates.TRESPASSING);
            ulong trespassTime = DataManagement.GetIncrementalTrespassingTime(prefab);
            _lastWorldTime = worldTime;
            if(trespassTime < Constant.TrespassingLimit) return;

            GameInteractions.SummonScreamer(prefab);
            DataManagement.ResetTrespassingTime(prefab);
        }
    }
}