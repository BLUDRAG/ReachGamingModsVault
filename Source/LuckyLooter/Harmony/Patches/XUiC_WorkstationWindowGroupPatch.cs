using Audio;
using HarmonyLib;
using UnityEngine;

namespace LuckyLooter.Harmony.Patches
{
    [HarmonyPatch(typeof(XUiC_WorkstationWindowGroup))]
    public class XUiC_WorkstationWindowGroupPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnOpen")]
        public static bool OnOpenPrefix()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnClose")]
        public static bool OnClosePrefix(XUiC_WorkstationWindowGroup __instance, string ___workstation)
        {
            __instance.WorkstationData.SetUserAccessing(false);
            WorkstationData workstationData = CraftingManager.GetWorkstationData(___workstation);

            GameManager.Instance.TEUnlockServer(__instance.WorkstationData.TileEntity.GetClrIdx(),
                                                __instance.WorkstationData.TileEntity.ToWorldPos(),
                                                __instance.WorkstationData.TileEntity.entityId);
            
            if(workstationData != null)
                Manager.BroadcastPlayByLocalPlayer(__instance.WorkstationData.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f,
                                                   workstationData.CloseSound);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(float _dt)
        {
            return false;
        }
    }
}