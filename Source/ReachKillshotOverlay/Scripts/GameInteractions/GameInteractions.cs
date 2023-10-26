using ReachKillshotOverlay.Scripts.Features;

namespace ReachKillshotOverlay.Scripts.GameInteractions
{
    public static class GameInteractions
    {
        public static void Init(Mod modInstance)
        {
            ModEvents.EntityKilled.RegisterHandler(LogKill);
        }

        public static void LogKill(Entity killedEntity, Entity killerEntity)
        {
            if(killedEntity is null || killedEntity.EntityClass is null)
            {
                return;
            }
            
            if(killerEntity is null || !(killerEntity is EntityPlayerLocal))
            {
                return;
            }

            bool headshot = ((EntityAlive)killedEntity).bodyDamage.bodyPartHit == EnumBodyPartHit.Head;
            KillDisplay.QueueKill(Localization.Get(killedEntity.EntityClass.entityClassName), headshot);
        }
    }
}