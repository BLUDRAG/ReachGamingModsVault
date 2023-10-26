using HarmonyLib;
using HighwayRadio.Scripts.GameInteractions;

namespace HighwayRadio.Harmony.Patches;

[HarmonyPatch(typeof(EntityVehicle))]
public class EntityVehiclePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("EnterVehicle")]
    public static bool EnterVehiclePrefix()
    {
        GameStateMonitor.OccupyingVehicle = true;
        Scripts.Features.AudioPlayer.Show();
        _ = Scripts.Features.AudioPlayer.Play();
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("DriverRemoved")]
    public static bool DriverRemovedPrefix()
    {
        GameStateMonitor.OccupyingVehicle = false;
        Scripts.Features.AudioPlayer.Hide();
        return true;
    }
}