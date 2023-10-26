using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalArtworker.Scripts.Entities;
using UnityEngine;

namespace PersonalArtworker.Scripts.Data
{
    public static class DataManagement
    {
        public const string DEFAULT_IMAGE_ID = "612402e89fd3fc1403f8365c2541143d954f725b03801f2ce41bc074afbcf33c";

        private static ImageRecordContainer _imageRecords = new ImageRecordContainer();
        private static string _fullImagePath => Path.Combine(_dataPath, GameInteractions.GameInteractions.IsServer() ? SERVER_PATH : CLIENT_PATH);
        private static string _dataPath;
        private const  string SERVER_PATH = "Server";
        private const  string CLIENT_PATH = "Client";
        private const  string IMAGE_RECORDS_FILE = "ImageRecords.json";

        public static void Init(Mod modInstance)
        {
            _dataPath = Path.Combine(modInstance.Path, "Data");
            LoadImageRecords();
        }

        private static void LoadImageRecords()
        {
            if(File.Exists(Path.Combine(_dataPath, IMAGE_RECORDS_FILE)))
            {
                _imageRecords = JsonConvert.DeserializeObject<ImageRecordContainer>(File.ReadAllText(Path.Combine(_dataPath, IMAGE_RECORDS_FILE)));
            }
        }

        private static void SaveImageRecords()
        {
            File.WriteAllText(Path.Combine(_dataPath, IMAGE_RECORDS_FILE), JsonConvert.SerializeObject(_imageRecords));
        }

        public static async Task ProcessImage(byte[] imageData, string guid)
        {
            string path = Path.Combine(_dataPath, _fullImagePath, guid);

            if(!File.Exists(path))
            {
                Task task = Task.Run(() => File.WriteAllBytes(path, imageData));
                await task;
            }
        }
        
        public static void RemoveImage(string guid)
        {
            string path = Path.Combine(_dataPath, _fullImagePath, guid);

            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void ProcessImageServerSide(byte[] imageData, string guid, int chunkIndex, Vector3i blockPosition)
        {
            _ = ProcessImage(imageData, guid);
            _imageRecords.Register(guid, chunkIndex, blockPosition);
            SaveImageRecords();
        }

        public static void RemoveImageServerSide(string guid, int chunkIndex, Vector3i blockPosition)
        {
            int remainingReferences = _imageRecords.Unregister(guid, chunkIndex, blockPosition);
            SaveImageRecords();

            if(remainingReferences <= 0)
            {
                RemoveImage(guid);
            }
        }

        public static async Task<byte[]> LoadImage(string guid)
        {
            string path = Path.Combine(_fullImagePath, guid);
            Task<byte[]> task = Task.Run(() => File.Exists(path) ? File.ReadAllBytes(path) : null);
            return await task;
        }

        public static async Task<string> GetImageID(Texture2D texture, ArtworkEntity entity)
        {
            byte[] textureData = GetReadableTexture(texture).EncodeToJPG(75);

            Task<string> task = Task.Run(() =>
                                         {
                                             string str       = Encoding.ASCII.GetString(textureData);
                                             SHA256 sha256    = SHA256.Create();
                                             byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
                                             sha256.Dispose();
                                             string id = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                             _ = ProcessImage(textureData, id);

                                             if(ConnectionManager.Instance.IsClient)
                                             {
                                                 SendImagePackage(textureData, id, entity.GetChunk().ClrIdx,
                                                                  entity.BlockPosition);
                                             }

                                             return id;
                                         });

            return await task;
        }

        private static void SendImagePackage(byte[] textureData, string guid, int chunkIndex, Vector3i blockPosition)
        {
            NetPackage package = NetPackageManager
                                .GetPackage<SendImageNetPackage>()
                                .Setup(textureData, guid, chunkIndex, blockPosition);

            ConnectionManager.Instance.SendToServer(package);
        }

        private static Texture2D GetReadableTexture(Texture2D texture)
        {
            Texture2D readableTexture = texture;

            if(!texture.isReadable)
            {
                RenderTexture temporaryRenderTexture = new RenderTexture(texture.width, texture.height,
                                                                         0, RenderTextureFormat.ARGB32,
                                                                         RenderTextureReadWrite.Linear);

                RenderTexture previousActiveRenderTexture = RenderTexture.active;
                RenderTexture.active = temporaryRenderTexture;
                Graphics.Blit(texture, temporaryRenderTexture);

                readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, 1, true);

                readableTexture.ReadPixels(new Rect(0, 0, temporaryRenderTexture.width, temporaryRenderTexture.height),
                                           0, 0, true);

                readableTexture.Apply(true);

                RenderTexture.active = previousActiveRenderTexture;
                temporaryRenderTexture.Release();
            }

            return readableTexture;
        }
    }
}