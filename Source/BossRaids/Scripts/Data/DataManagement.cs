using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace BossRaids.Scripts.Data
{
    public static class DataManagement
    {
        private static BossRaidsData _data;
        private static string        _dataPath;
        private static string        _dataFile => $"{nameof(BossRaids)}Data.json";

        private static string _saveName     => GamePrefs.GetString(EnumGamePrefs.GameName);
        private static string _dataFilePath => Path.Combine(_dataPath, $"{_saveName}_{_dataFile}");

        public static void Init(Mod modInstance)
        {
            ModEvents.GameStartDone.RegisterHandler(Load);
            ModEvents.SavePlayerData.RegisterHandler(Save);
            _dataPath = Path.Combine(modInstance.Path, "Data");
            _data     = new BossRaidsData();
        }

        private static void Save(ClientInfo clientInfo, PlayerDataFile playerDataFile)
        {
            Save();
        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(_data);
            File.WriteAllText(_dataFilePath, json);
        }

        public static void Load()
        {
            if(!File.Exists(_dataFilePath)) return;

            string json = File.ReadAllText(_dataFilePath);
            _data = JsonConvert.DeserializeObject<BossRaidsData>(json);
            if(_data is null) _data = new BossRaidsData();
        }
        
        public static (int day, int hour, int minute) GetCurrentSpawnTime()
        {
            if(_data.NextSpawnDay == 0 || _data.NextSpawnDay < GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime)) CalculateNextSpawnTime();
            return (_data.NextSpawnDay, _data.NextSpawnHour, _data.NextSpawnMinute);
        }
        
        public static float GetSpawnChance()
        {
            return _data.ForceSpawnChance;
        }
        
        public static void IncrementSpawnChance()
        {
            _data.ForceSpawnChance += _data.ForceSpawnIncrementRate;
            Save();
        }
        
        public static void ResetSpawnChance()
        {
            _data.ForceSpawnChance = 0f;
            Save();
        }
        
        public static void CalculateNextSpawnTime()
        {
            ulong worldTime = GameManager.Instance.World.worldTime;
            _data.NextSpawnDay    = GameUtils.WorldTimeToDays(worldTime) + 1;
            _data.NextSpawnHour   = Random.Range(0, 24);
            _data.NextSpawnMinute = Random.Range(0, 60);
            Save();
        }
    }
}