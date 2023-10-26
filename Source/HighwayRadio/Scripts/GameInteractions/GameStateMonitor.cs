using UnityEngine;

namespace HighwayRadio.Scripts.GameInteractions;

public sealed class GameStateMonitor : MonoBehaviour
{
    public static bool OccupyingVehicle = false;
    
    public static void Init()
    {
        GameObject gameStateMonitor = new GameObject("Game State Monitor");
        DontDestroyOnLoad(gameStateMonitor);
        gameStateMonitor.AddComponent<GameStateMonitor>();
    }

    public void Update()
    {
        if(!OccupyingVehicle || !GameManager.Instance.gameStateManager.IsGameStarted())
        {
            return;
        }
        
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if(Input.GetKeyDown(KeyCode.Home))
        {
            Features.AudioPlayer.TogglePause();
        }
        else if(Input.GetKeyDown(KeyCode.PageUp))
        {
            Features.AudioPlayer.IncreaseVolume();
        }
        else if(Input.GetKeyDown(KeyCode.PageDown))
        {
            Features.AudioPlayer.DecreaseVolume();
        }
        else if(Input.GetKeyDown(KeyCode.End))
        {
            _ = Features.AudioPlayer.Play();
        }
    }
}