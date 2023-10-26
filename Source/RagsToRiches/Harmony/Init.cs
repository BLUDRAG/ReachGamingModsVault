using System.Reflection;
using RagsToRiches.Scripts.Actors;
using RagsToRiches.Scripts.Data;
using RagsToRiches.Scripts.GameInteractions;

namespace RagsToRiches.Harmony
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
            CanvasActor.Init(modInstance);
            GameInteractions.Init();
            GameStateMonitor.Init();
            FinanceManagement.Init();
        }
    }
}