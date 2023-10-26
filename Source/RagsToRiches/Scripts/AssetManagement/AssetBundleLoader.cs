using System.Threading.Tasks;
using UnityEngine;

namespace RagsToRiches.Scripts.AssetManagement
{
    public static class AssetBundleLoader
    {
        public static async Task<GameObject> LoadContent(string bundlePath, string assetName)
        {
            AssetBundle bundle = await LoadBundle(bundlePath);
            if(bundle is null) return null;
            GameObject content = await LoadAsset(bundle, assetName);
            bundle.Unload(false);
            return content ? content : null;
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
        
        public static async Task<GameObject> LoadAsset(AssetBundle bundle, string assetName)
        {
            AssetBundleRequest assetRequest = bundle.LoadAssetAsync<GameObject>(assetName);
            while(!assetRequest.isDone) await Task.Yield();
            
            if(!(assetRequest.asset is GameObject asset))
            {
                Log.Error($"Failed to load asset: {assetName}");
                return null;
            }

            return asset;
        }
    }
}