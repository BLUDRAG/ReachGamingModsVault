using System.Threading.Tasks;
using PersonalArtworker.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace PersonalArtworker.Scripts.Features
{
    public class ArtworkImageProcessor
    {
        private readonly MeshRenderer _meshRenderer;

        public ArtworkImageProcessor(MeshRenderer meshRenderer)
        {
            _meshRenderer = meshRenderer;
        }

        public async Task ApplyImage(string imagePath)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture($"file://{imagePath}");

            www.SendWebRequest();
            while(!www.isDone) await Task.Yield();

            if(www.result == UnityWebRequest.Result.ConnectionError ||
               www.result == UnityWebRequest.Result.ProtocolError)
            {
                Log.Error(www.error);
            }
            else
            {
                _meshRenderer.material.mainTexture = DownloadHandlerTexture.GetContent(www);
            }
        }

        public async Task LoadImage(string guid)
        {
            if(guid is null || guid == DataManagement.DEFAULT_IMAGE_ID) return;

            Task<byte[]> task        = Task.Run(() => DataManagement.LoadImage(guid));
            byte[]       textureData = await task;
            if(textureData is null) return;
            
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(textureData);
            _meshRenderer.material.mainTexture = texture;
        }
    }
}