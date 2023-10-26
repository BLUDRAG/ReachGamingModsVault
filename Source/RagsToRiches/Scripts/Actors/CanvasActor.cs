using System;
using System.IO;
using System.Threading.Tasks;
using RagsToRiches.Script.Data;
using RagsToRiches.Scripts.Data;
using RagsToRiches.Scripts.AssetManagement;
using RagsToRiches.Scripts.GameInteractions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RagsToRiches.Scripts.Actors
{
    public static class CanvasActor
    {
        private static GameObject _canvas;
        private static Text       _priceText;
        private static Button     _transactionButton;
        private static Text       _transactionButtonText;
        private static RawImage   _previewImage;
        private static Button     _creditsButton;
        private static bool       _canvasOpen;
        private const  string     _canvasPath  = "Assets/RagsToRichesCanvas.unity3d";
        private const  string     _canvasName  = "Rags To Riches Canvas";
        private const  string     _creditsLink = "https://www.youtube.com/watch?v=vemY5dXzHlQ&list=PLYL9eL_H8WdEdp34SeDoV7ocGNkQDFytU";

        public static async void Init(Mod modInstance)
        {
            await LoadCanvas(modInstance);
            RegisterCreditsButtonListener();
        }

        private static async Task LoadCanvas(Mod modInstance)
        {
            GameObject asset = await AssetBundleLoader.LoadContent(Path.Combine(modInstance.Path, _canvasPath), _canvasName);

            _canvas                = Object.Instantiate(asset);
            _transactionButton     = _canvas.transform.FindInChildren("Transaction Button").GetComponent<Button>();
            _transactionButtonText = _transactionButton.GetComponentInChildren<Text>();
            _previewImage          = _canvas.GetComponentInChildren<RawImage>();
            _creditsButton         = _canvas.transform.FindInChildren("Credits Button").GetComponent<Button>();
            _priceText             = _canvas.transform.FindInChildren("Header Text").GetComponent<Text>();
            _canvas.SetActive(false);
        }

        private static void SetPriceText(int price)
        {
            _priceText.text = $"${price:N0}";
        }

        private static void UpdateCanvasState(PrefabInstance prefab, TransactionStates state)
        {
            UpdatePreviewImage(prefab);
            UpdateTransactionButton(state);
        }

        private static async void UpdatePreviewImage(PrefabInstance prefab)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{prefab.location.FullPathNoExtension}.jpg");

            www.SendWebRequest();
            while(!www.isDone) await Task.Yield();

            if(www.result == UnityWebRequest.Result.ConnectionError ||
               www.result == UnityWebRequest.Result.ProtocolError)
            {
                Log.Error(www.error);
            }
            else
            {
                _previewImage.texture = DownloadHandlerTexture.GetContent(www);
            }
        }
        
        private static void UpdateTransactionButton(TransactionStates state)
        {
            switch(state)
            {
                case TransactionStates.NONE:
                    _transactionButton.gameObject.SetActive(false);

                    break;
                case TransactionStates.BUY:
                    MakeBuyingButton();

                    break;
                case TransactionStates.SELL:
                    MakeSellingButton();

                    break;
            }
        }

        /// <summary>
        /// Makes the transaction button a buy button.
        /// </summary>
        private static void MakeBuyingButton()
        {
            _transactionButtonText.text = "BUY";
            SetTransactionButtonListener(GameInteractions.GameInteractions.BuyCurrentPrefab);
            _transactionButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Makes the transaction button a sell button.
        /// </summary>
        private static void MakeSellingButton()
        {
            _transactionButtonText.text = "SELL";
            SetTransactionButtonListener(GameInteractions.GameInteractions.SellCurrentPrefab);
            _transactionButton.gameObject.SetActive(true);
        }

        public static void Open(PrefabInstance prefab)
        {
            if(_canvasOpen) return;

            _canvas.SetActive(true);
            _canvasOpen = true;
            UpdateCanvasState(prefab);
        }

        public static void Close()
        {
            if(!_canvasOpen) return;

            _canvas.SetActive(false);
            _canvasOpen = false;
        }

        /// <summary>
        /// Sets the transaction button callback action.
        /// </summary>
        /// <param name="action"></param>
        private static void SetTransactionButtonListener(Action action)
        {
            _transactionButton.onClick.RemoveAllListeners();
            _transactionButton.onClick.AddListener(new UnityAction(action));
        }

        /// <summary>
        /// Updates the canvas state to match the current prefab.
        /// </summary>
        public static void UpdateCanvasState(PrefabInstance currentPrefab)
        {
            if(currentPrefab is null)
            {
                UpdateCanvasState(null, TransactionStates.NONE);
                return;
            }

            int price = FinanceManagement.GetPrice(currentPrefab);

            if(DataManagement.HasBought(currentPrefab))
            {
                SetPriceText(price / 2);
                UpdateCanvasState(currentPrefab, TransactionStates.SELL);
                return;
            }

            SetPriceText(price);
            UpdateCanvasState(currentPrefab, TransactionStates.BUY);
        }
        
        private static void RegisterCreditsButtonListener() 
        { 
            _creditsButton.onClick.RemoveAllListeners(); 
            _creditsButton.onClick.AddListener(OpenCreditsLink);
        }

        private static void OpenCreditsLink()
        {
            Application.OpenURL(_creditsLink);
        }
    }
}