using System.Reflection;

namespace LuckyLooter.Harmony
{
    public class Init : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}