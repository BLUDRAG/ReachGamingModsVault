using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace BossRaids.Harmony.Patches
{
    [HarmonyPatch(typeof(XUiC_TargetBar))]
    public class XUiC_TargetBarPatch
    {
        public static XUiC_TargetBar Instance;
        public static BossGroup      BossGroup;
        public static Type           Type;
        public static FieldInfo      DeltaTime;
        
        [HarmonyPrefix]
        [HarmonyPatch("Init")]
        public static void InitPrefix(XUiC_TargetBar __instance)
        {
            Instance  = __instance;
            BossGroup = null;
            Type      = Instance.GetType();
            DeltaTime = Type.GetField("deltaTime", BindingFlags.Instance | BindingFlags.NonPublic);

            FieldInfo fieldInfo = Type.GetField("gameEventManager", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(Instance, GameEventManager.Current);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(float _dt)
        {
            if (Instance.ViewComponent != null && Instance.WindowGroup != null && Instance.WindowGroup.isShowing && Instance.ViewComponent.IsVisible)
                Instance.ViewComponent.Update(_dt);
            for (int index = 0; index < Instance.Children.Count; ++index)
            {
              XUiController child = Instance.Children[index];
              if (!child.IsDormant)
                child.Update(_dt);
            }

            DeltaTime.SetValue(Instance, _dt);
            
            Instance.ViewComponent.IsVisible = !(BossGroup is null) && !(BossGroup.BossEntity is null) && BossGroup.BossEntity.IsAlive();
            if(!Instance.ViewComponent.IsVisible) return false;
            
            Instance.Target = BossGroup.BossEntity;
            Instance.RefreshBindings(Instance.IsDirty);
            Instance.IsDirty = false;
            
            return false;
        }

        public static void SetBossGroup(EntityPlayer localPlayer, EntityAlive boss)
        {
            BossGroup = new BossGroup(localPlayer, boss,
                                      new List<EntityAlive>(), BossGroup.BossGroupTypes.Standard,
                                      "ui_game_symbol_twitch_boss_bar_default");

            FieldInfo fieldInfo = Type.GetField("CurrentBossGroup", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(Instance, BossGroup);
            
            fieldInfo = Type.GetField("noTargetFadeTime", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(Instance, 0.0f);

            Instance.Target = BossGroup.BossEntity;
        }
    }
}