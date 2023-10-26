using System.Threading.Tasks;
using PersonalArtworker.Scripts.Blocks;
using PersonalArtworker.Scripts.Data;
using PersonalArtworker.Scripts.Features;
using UnityEngine;

namespace PersonalArtworker.Scripts.Entities
{
    public class ArtworkEntity : TileEntity
    {
        public const int TILE_ENTITY_TYPE = 128;
        
        public bool     Initialized;
        public Vector3i BlockPosition;
        
        private readonly ArtworkFeatureController _featureController;
        private          MeshRenderer             _meshRenderer;
        private          Vector3                  _loadedPosition;
        private          Vector3                  _loadedScale;
        private          Color                    _loadedColor;
        private          bool                     _aspectLocked;
        private          string                   _guid;
        private          bool                     _dataLoaded;

        private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

        public ArtworkEntity(Chunk _chunk) : base(_chunk)
        {
            _featureController = new ArtworkFeatureController();
            _guid              = DataManagement.DEFAULT_IMAGE_ID;
        }
        
        public string GetImageID()
        {
            return _guid;
        }

        public bool IsAspectLocked()
        {
            return _aspectLocked;
        }
        
        public void ToggleAspectLocked()
        {
            _aspectLocked = !_aspectLocked;
            _featureController.UpdateAspectLockedState();
        }

        public void SetImageID(string guid)
        {
            _guid = guid;
        }

        public override void Reset(FastTags questTags)
        {
            _featureController?.Reset();
        }

        public override TileEntityType GetTileEntityType()
        {
            return (TileEntityType)TILE_ENTITY_TYPE;
        }

        public ArtworkFeatureController Init(ArtworkBlock block, MeshRenderer artworkRenderer, GameObject root)
        {
            _meshRenderer = artworkRenderer;
            _featureController.Init(block, artworkRenderer.transform, root, this);
            _featureController.PrepareImageProcessor(artworkRenderer);
            LoadData();
            Initialized = true;
            return _featureController;
        }

        private void LoadData()
        {
            if(!_dataLoaded) return;
            _featureController.LoadData(_loadedPosition, _loadedScale, _guid);
            SetArtworkColor(_loadedColor);
            _dataLoaded = false;
        }

        public async Task LoadImage()
        {
            await _featureController.LoadImage(_guid);
        }

        public void ToggleResizer()
        {
            _featureController.ToggleResizer();
        }

        public async void ApplyImage(string imagePath)
        {
            await _featureController.ApplyImage(imagePath);
            _guid = await DataManagement.GetImageID((Texture2D)_meshRenderer.material.mainTexture, this);
            _featureController.SaveData();
        }
        
        public Color GetArtworkColor()
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(materialPropertyBlock);
            Color color = materialPropertyBlock.GetVector(COLOR_ID);

            if(color == Color.clear)
            {
                color = Color.white;
            }

            return color;
        }

        public Vector3 GetLoadedScale()
        {
            return _loadedScale;
        }

        public void SetArtworkColor(Color color)
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetVector(COLOR_ID, color);
            _meshRenderer.SetPropertyBlock(materialPropertyBlock);
            _featureController.SaveData();
        }
        
        public Material GetMaterial()
        {
            return _meshRenderer.material;
        }

        public override void OnUnload(World world)
        {
            base.OnUnload(world);
            bool isServer = GameInteractions.GameInteractions.IsServer();

            if(isServer)
            {
                return;
            }

            _featureController.DisableResizer();
        }

        public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
        {
            base.write(_bw, _eStreamMode);
            bool isServer = GameInteractions.GameInteractions.IsServer();
            
            (Vector3 position, Vector3 scale) = isServer ? (_loadedPosition, _loadedScale) : _featureController.GetSizeData();
            Color color = isServer ? _loadedColor : GetArtworkColor();
            
            _bw.Write(position.x);
            _bw.Write(position.y);
            _bw.Write(position.z);
            
            _bw.Write(scale.x);
            _bw.Write(scale.y);
            _bw.Write(scale.z);

            _bw.Write(color.r);
            _bw.Write(color.g);
            _bw.Write(color.b);
            _bw.Write(color.a);
            
            _bw.Write(_aspectLocked);
            _bw.Write(_guid);
        }

        public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
        {
            base.read(_br, _eStreamMode);
            _loadedPosition = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
            _loadedScale    = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
            _loadedColor    = new Color(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
            _aspectLocked   = _br.ReadBoolean();
            _guid           = _br.ReadString();
            _dataLoaded     = true;
        }
    }
}