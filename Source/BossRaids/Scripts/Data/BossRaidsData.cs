using System;

namespace BossRaids.Scripts.Data
{
    [Serializable]
    public class BossRaidsData
    {
        public int   NextSpawnDay;
        public int   NextSpawnHour;
        public int   NextSpawnMinute;
        public float ForceSpawnChance;
        public float ForceSpawnIncrementRate = 0.05f; // Increase spawn chance by 5% every failed attempt. Reset to 0% on successful attempt.
    }
}