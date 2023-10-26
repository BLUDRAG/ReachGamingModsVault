using System.IO;
using PersonalArtworker.Scripts.AssetManagement;
using SimpleFileBrowser;
using UnityEngine;

namespace PersonalArtworker.Scripts.Plugins
{
    public static class UnityFileBrowser
    {
        private const string FILE_BROWSER_ASSET_PATH = "Assets/SimpleFileBrowserCanvas.unity3d";
        private const string FILE_BROWSER_ASSET_NAME = "SimpleFileBrowserCanvas";
        
        public static async void Init(Mod modInstance)
        {
            GameObject asset = await AssetBundleLoader .LoadContent(Path.Combine(modInstance.Path,
                                                                        FILE_BROWSER_ASSET_PATH),
                                                                    FILE_BROWSER_ASSET_NAME);

            GameObject canvas = Object.Instantiate(asset);
            Object.DontDestroyOnLoad(canvas);
            canvas.SetActive(false);
            FileBrowser.Instance = canvas.GetComponent<FileBrowser>();
        }
    }
}