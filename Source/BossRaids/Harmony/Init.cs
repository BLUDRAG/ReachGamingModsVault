using System.Reflection;
using BossRaids.Scripts;
using BossRaids.Scripts.Data;

namespace BossRaids.Harmony
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
            DataManagement.Init(modInstance);
            GameInteractions.Init();
            GameStateMonitor.Init();
            CommandInterpreter.Init();
        }
    }
}