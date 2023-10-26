using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalArtworker.Scripts.Blocks;
using PersonalArtworker.Scripts.Entities;
using UnityEngine;

namespace PersonalArtworker.Scripts.Features
{
    public class ArtworkFeatureController
    {
        public static bool IsLegacyInputActive;
        
        private Transform             _mesh;
        private GameObject            _resizeRoot;
        private ArtworkEntity         _entity;
        private ArtworkImageProcessor _imageProcessor;
        private ArtworkBlock          _block;
        
        private static readonly Vector3 ORIGINAL_POSITION = new Vector3(0f, 0f, 0.45f);
        private static readonly Vector3 ORIGINAL_SCALE    = new Vector3(1f, 1f, 0.1f);
        private static string LEGACY_INPUT_KEY => $"{nameof(ArtworkFeatureController)}_{nameof(IsLegacyInputActive)}";

        public static void InitConfig()
        {
            IsLegacyInputActive = PlayerPrefs.GetInt(LEGACY_INPUT_KEY, 1) == 0;
        }
        
        public static void ToggleLegacyInput()
        {
            IsLegacyInputActive = !IsLegacyInputActive;
            PlayerPrefs.SetInt(LEGACY_INPUT_KEY, IsLegacyInputActive ? 1 : 0);
        }
        
        public void Init(ArtworkBlock block, Transform mesh, GameObject resizeRoot, ArtworkEntity entity)
        {
            _block      = block;
            _mesh       = mesh;
            _resizeRoot = resizeRoot;
            _entity     = entity;
        }

        public void PrepareImageProcessor(MeshRenderer meshRenderer)
        {
            _imageProcessor = new ArtworkImageProcessor(meshRenderer);
        }

        public void Reset()
        {
            if(_mesh is null)
            {
                return;
            }

            _mesh.localPosition = ORIGINAL_POSITION;
            _mesh.localScale = ORIGINAL_SCALE;
        }

        public void ToggleResizer()
        {
            _resizeRoot.SetActive(!_resizeRoot.activeSelf);
            RecalculateMultiBlockDim();
        }

        public void DisableResizer()
        {
            _resizeRoot.SetActive(false);
        }

        public (Vector3 position, Vector3 scale) GetSizeData()
        {
            return (_mesh.localPosition, _mesh.localScale);
        }

        public async Task ApplyImage(string imagePath)
        {
            await _imageProcessor.ApplyImage(imagePath);
        }

        public void SaveData()
        {
            if(!GameInteractions.GameInteractions.IsServer())
            {
                RecalculateMultiBlockDim();
            }

            _entity.SetModified();
        }

        public void LoadData(Vector3 position, Vector3 scale, string guid)
        {
            _mesh.localPosition = position;
            _mesh.localScale    = scale;
            _ = LoadImage(guid);
        }

        public async Task LoadImage(string guid)
        {
            await _imageProcessor.LoadImage(guid);
        }

        public bool IsAspectLocked()
        {
            return _entity.IsAspectLocked();
        }

        public void UpdateAspectLockedState()
        {
            if(!IsAspectLocked()) return;

            Material material    = _entity.GetMaterial();
            float    aspectRatio = (float)material.mainTexture.width / material.mainTexture.height;
            Vector3  localScale  = _mesh.localScale;
            
            localScale.x     = localScale.x;
            localScale.y     = localScale.x / aspectRatio;
            _mesh.localScale = localScale;
        }

        /// <summary>
        /// Directly override the MultiBlockDim property based on the original <see cref="Block"/> code.
        /// </summary>
        private void UpdateMultiBlockDim(Vector3 scale)
        {
            Vector3i       vector3i  = new Vector3i(Mathf.Ceil(scale.x), Mathf.Ceil(scale.y), 1);
            List<Vector3i> _pos      = new List<Vector3i>();
            
            int num1 = vector3i.x / 2;
            int num2 = Mathf.RoundToInt((float)(vector3i.x / 2.0 + 0.10000000149011612)) - 1;
            int num3 = vector3i.z / 2;
            int num4 = Mathf.RoundToInt((float)(vector3i.z / 2.0 + 0.10000000149011612)) - 1;

            for(int _x = -num1; _x <= num2; ++_x)
            {
                for(int _y = 0; _y < vector3i.y; ++_y)
                {
                    for(int _z = -num3; _z <= num4; ++_z)
                        _pos.Add(new Vector3i(_x, _y, _z));
                }
            }

            _block.Properties.Values["MultiBlockDim"] = $"{vector3i.x},{vector3i.y},{vector3i.z}";
            _block.multiBlockPos = new Block.MultiBlockArray(vector3i, _pos);
        }
        
        public void RecalculateMultiBlockDim()
        {
            // UpdateMultiBlockDim(_mesh.localScale);
        }

        public void ResetMultiBlockDim()
        {
            // UpdateMultiBlockDim(Vector3.one);
        }
    }
}