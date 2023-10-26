using System.IO;
using PersonalArtworker.Scripts.AssetManagement;
using PersonalArtworker.Scripts.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;

namespace PersonalArtworker.Scripts.Plugins
{
    public static class UnityColorPicker
    {
        private static GameObject         _canvas;
        private static ColorPickerControl _colorPicker;

        private const  string FILE_BROWSER_ASSET_PATH = "Assets/ColorPickerCanvas.unity3d";
        private const  string FILE_BROWSER_ASSET_NAME = "ColorPickerCanvas";
        private static int    COLOR_ID                = Shader.PropertyToID("_Color");

        public static async void Init(Mod modInstance)
        {
            GameObject asset = await AssetBundleLoader.LoadContent(Path.Combine(modInstance.Path,
                                                                                    FILE_BROWSER_ASSET_PATH), 
                                                                   FILE_BROWSER_ASSET_NAME);

            _canvas = Object.Instantiate(asset);
            Object.DontDestroyOnLoad(_canvas);
            _canvas.SetActive(false);
            
            _colorPicker = _canvas.GetComponentInChildren<ColorPickerControl>();
            Button closeButton = _colorPicker.transform.Find("Close Button").GetComponent<Button>();
            Button closeButtonBackground = _colorPicker.transform.Find("Close Button Background").GetComponent<Button>();
            closeButton.onClick.AddListener(() => GameInteractions.GameInteractions.ToggleUIInteractiveState(false));
            closeButtonBackground.onClick.AddListener(() => GameInteractions.GameInteractions.ToggleUIInteractiveState(false));
        }
        
        public static void Show(ArtworkEntity entity)
        {
            _canvas.SetActive(true);
            _colorPicker.onValueChanged.RemoveAllListeners();
            _colorPicker.CurrentColor = entity.GetArtworkColor();
            _colorPicker.onValueChanged.AddListener(entity.SetArtworkColor);
        }
    }
}