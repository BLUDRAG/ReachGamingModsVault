using RagsToRiches.Scripts.Data;
using Object = UnityEngine.Object;

namespace RagsToRiches.Scripts.GameInteractions
{
    public static class FinanceManagement
    {
        private static LocalPlayer _localPlayer;
        private const  int         _basePrefabPrice       = 10000;
        private const  float       _prefabPriceMultiplier = 5f;

        public static void Init()
        {
            ModEvents.GameStartDone.RegisterHandler(GetGameInstances);
        }

        private static void GetGameInstances()
        {
            _localPlayer = Object.FindObjectOfType<LocalPlayer>();
        }

        /// <summary>
        /// Returns true if the player can afford the given prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static bool PlayerCanAfford(PrefabInstance prefab)
        {
            int price = GetPrice(prefab);
            XUiM_PlayerInventory playerInventory = _localPlayer.playerUI.xui.PlayerInventory;
            return playerInventory.CurrencyAmount >= price;
        }

        /// <summary>
        /// Returns the price of the given prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int GetPrice(PrefabInstance prefab)
        {
            int   totalBlocks      = GameInteractions.GetTotalChunkBlocks(prefab);
            float volumeMultiplier = totalBlocks / Constant.ChunkBlocksAdjustment;
            float biomeModifier    = GameInteractions.GetBiomeLootModifier(prefab);
            return (int)((_basePrefabPrice + (int)(_basePrefabPrice * prefab.prefab.DifficultyTier * _prefabPriceMultiplier)) * volumeMultiplier * biomeModifier);
        }

        /// <summary>
        /// Buys the given prefab if the player can afford it.
        /// </summary>
        /// <param name="prefab"></param>
        public static void Buy(PrefabInstance prefab)
        {
            int                  price           = GetPrice(prefab);
            XUiM_PlayerInventory playerInventory = _localPlayer.playerUI.xui.PlayerInventory;
            ItemStack            _itemStack      = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem), price);
            playerInventory.RemoveItem(_itemStack);
            DataManagement.Buy(prefab);
            DataManagement.Save();
        }
        
        /// <summary>
        /// Sells the given prefab if the player owns it.
        /// </summary>
        /// <param name="prefab"></param>
        public static void Sell(PrefabInstance prefab)
        {
            int                  price           = GetPrice(prefab);
            XUiM_PlayerInventory playerInventory = _localPlayer.playerUI.xui.PlayerInventory;
            ItemStack            _itemStack      = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem), (int)(price * Constant.SellingPriceMarkdownRate));
            playerInventory.AddItem(_itemStack);
            DataManagement.Sell(prefab);
            DataManagement.Save();
        }

        /// <summary>
        /// Returns true if the player owns the current prefab.
        /// </summary>
        /// <returns></returns>
        public static bool OwnsCurrentPrefab()
        {
            PrefabInstance prefab = GameInteractions.GetPrefabNearPlayer();
            return !(prefab is null) && DataManagement.HasBought(prefab);
        }
    }
}