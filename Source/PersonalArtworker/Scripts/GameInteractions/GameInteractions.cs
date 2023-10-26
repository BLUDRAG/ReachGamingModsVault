using UnityEngine;

namespace PersonalArtworker.Scripts.GameInteractions
{
    public static class GameInteractions
    {
        public static bool IsUIInteracting;
        
        private static LocalPlayer _localPlayer;

        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(GetGameInstances);
        }

        private static void GetGameInstances()
        {
            if(_localPlayer is null)
            {
                _localPlayer = Object.FindObjectOfType<LocalPlayer>();
            }
        }

        public static int GetPlayerID()
        {
            return _localPlayer.entityPlayerLocal.entityId;
        }

        public static void ToggleUIInteractiveState(bool active)
        {
            GetGameInstances();
            IsUIInteracting = active;
            GameManager.Instance.SetCursorEnabledOverride(active, active);

            if(active)
            {
                _localPlayer.playerUI.ActionSetManager.Push(_localPlayer.playerUI.playerInput.GUIActions);

            }
            else
            {
                _localPlayer.playerUI.ActionSetManager.Pop();
            }
        }

        public static bool IsServer()
        {
            return !(ConnectionManager.Instance.LocalServerInfo is null) && ConnectionManager.Instance.LocalServerInfo.IsDedicated;
        }

        public static void TogglePrimaryAction(bool enabled)
        {
            _localPlayer.entityPlayerLocal.playerInput.Primary.Enabled = enabled;
        }
    }
}