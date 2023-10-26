using System.Reflection;
using ReachKillshotOverlay.Scripts.Features;
using ReachKillshotOverlay.Scripts.GameInteractions;

namespace ReachKillshotOverlay.Harmony
{
    public class Init : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            InitCustomCode(modInstance);
        }

        private void InitCustomCode(Mod modInstance)
        {
            KillDisplay.Init(modInstance);
            GameInteractions.Init(modInstance);
        }
    }
}