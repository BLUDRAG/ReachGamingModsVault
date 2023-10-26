using System.Threading.Tasks;
using UnityEngine;

namespace ReachKillshotOverlay.Scripts.AssetManagement
{
    public static class AssetBundleLoader
    {
        public static async Task<T> LoadContent<T>(string bundlePath, string assetName)
        {
            AssetBundle bundle = await LoadBundle(bundlePath);
            if(bundle is null) return default;
            T content = await LoadAsset<T>(bundle, assetName);
            bundle.Unload(false);
            return !(content is null) ? content : default;
        }

        public static async Task<AssetBundle> LoadBundle(string bundlePath)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            while(!request.isDone) await Task.Yield();
            AssetBundle bundle = request.assetBundle;

            if(bundle is null)
            {
                Log.Error($"Failed to load : {bundlePath}");
                return null;
            }
            
            return bundle;
        }
        
        public static async Task<T> LoadAsset<T>(AssetBundle bundle, string assetName)
        {
            AssetBundleRequest assetRequest = bundle.LoadAssetAsync<T>(assetName);
            while(!assetRequest.isDone) await Task.Yield();
            
            if(!(assetRequest.asset is T asset))
            {
                Log.Error($"Failed to load asset: {assetName}");
                return default;
            }

            return asset;
        }
    }
}