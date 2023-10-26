using System.IO;
using Newtonsoft.Json;

namespace RagsToRiches.Scripts.Data
{
    public static class DataManagement
    {
        private static RagsToRichesData _data;
        private static string           _dataPath;
        private const  string           _dataFile = "RagsToRichesData.json";
        
        private static string _saveName     => GamePrefs.GetString(EnumGamePrefs.GameName);
        private static string _dataFilePath => Path.Combine(_dataPath, $"{_saveName}_{_dataFile}");

        public static void Init(Mod modInstance)
        {
            ModEvents.GameStartDone.RegisterHandler(Load);
            ModEvents.SavePlayerData.RegisterHandler(Save);
            _dataPath = Path.Combine(modInstance.Path, "Data");
            _data     = new RagsToRichesData();
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
            _data = JsonConvert.DeserializeObject<RagsToRichesData>(json);
            if(_data is null) _data = new RagsToRichesData();
        }

        public static bool HasBought(PrefabInstance prefab)
        {
            return _data.BoughtPrefabs.Contains(prefab.name);
        }
        
        public static void Buy(PrefabInstance prefab)
        {
            _data.BoughtPrefabs.Add(prefab.name);
        }
        
        public static void Sell(PrefabInstance prefab)
        {
            _data.BoughtPrefabs.Remove(prefab.name);
        }
        /// <summary>
        /// Determines if the player is currently squatting at this prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <returns></returns>
        public static bool IsSquatting(PrefabInstance prefab, int day, int hour)
        {
            SquattingData squattingData = GetSquattingData(prefab, day);
            return squattingData.Day == day || (squattingData.Day + 1 == day && hour < Constant.TrespassingHours.x);
        }

        /// <summary>
        /// Returns the amount of time spent trespassing on this prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static ulong GetIncrementalTrespassingTime(PrefabInstance prefab)
        {
            TrespassingData trespassingData = GetTrespassingData(prefab);
            trespassingData.TrespassingTime++;
            if(trespassingData.TrespassingTime % Constant.TrespassDataSaveFrequency == 0) Save();
            return trespassingData.TrespassingTime;
        }
        
        /// <summary>
        /// Resets the trespassing time to 0.
        /// </summary>
        /// <param name="prefab"></param>
        public static void ResetTrespassingTime(PrefabInstance prefab)
        {
            TrespassingData trespassingData = GetTrespassingData(prefab);
            trespassingData.TrespassingTime = 0;
            Save();
        }

        /// <summary>
        /// Returns the amount of warnings the player has received for trespassing on this prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int TrespassingWarnings(PrefabInstance prefab)
        {
            TrespassingData trespassingData = GetTrespassingData(prefab);
            trespassingData.Warnings++;
            Save();
            return trespassingData.Warnings;
        }
        
        private static SquattingData GetSquattingData(PrefabInstance prefab, int day)
        {
            SquattingData squattingData = _data.SquattingData.Find(data => data.Prefab == prefab.name);
            if(!(squattingData is null)) return squattingData;

            squattingData = new SquattingData(){ Prefab = prefab.name, Day = day };
            _data.SquattingData.Add(squattingData);
            Save();

            return squattingData;
        }

        private static TrespassingData GetTrespassingData(PrefabInstance prefab)
        {
            TrespassingData trespassingData = _data.TrespassingData.Find(data => data.Prefab == prefab.name);
            if(!(trespassingData is null)) return trespassingData;

            trespassingData = new TrespassingData(){ Prefab = prefab.name, TrespassingTime = 0, Warnings = 0 };
            _data.TrespassingData.Add(trespassingData);
            Save();

            return trespassingData;
        }
    }
}