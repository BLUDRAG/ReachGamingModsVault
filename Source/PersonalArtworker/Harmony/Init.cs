using System.Reflection;
using PersonalArtworker.Scripts.Data;
using PersonalArtworker.Scripts.Features;
using PersonalArtworker.Scripts.GameInteractions;
using PersonalArtworker.Scripts.Plugins;

namespace PersonalArtworker.Harmony
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
            GameInteractions.Init();
            UnityFileBrowser.Init(modInstance);
            UnityColorPicker.Init(modInstance);
            DataManagement.Init(modInstance);
            ArtworkFeatureController.InitConfig();
        }
    }
}