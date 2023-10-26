using System.Reflection;
using HighwayRadio.Scripts.GameInteractions;

namespace HighwayRadio.Harmony
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

        private static void InitCustomCode(Mod modInstance)
        {
            Scripts.Features.AudioPlayer.Init(modInstance);
            GameStateMonitor.Init();
        }
    }
}