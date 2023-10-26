using System.Collections.Generic;
using InControl;
using UnityEngine;

namespace ReachBuilderTools.Tools
{
    public class ItemActionReachBuilderCubeTerrainTool : ItemActionRanged
    {
        private static readonly Vector3[] INNER_POINTS = new Vector3[17]
                                                         {
                                                             new Vector3(0.5f, 0.5f, 0.5f),
                                                             new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1f, 0.0f, 0.0f),
                                                             new Vector3(0.0f, 1f, 0.0f), new Vector3(0.0f, 0.0f, 1f),
                                                             new Vector3(1f, 1f, 0.0f), new Vector3(0.0f, 1f, 1f),
                                                             new Vector3(1f, 0.0f, 1f), new Vector3(1f, 1f, 1f),
                                                             new Vector3(0.25f, 0.25f, 0.25f),
                                                             new Vector3(0.75f, 0.25f, 0.25f),
                                                             new Vector3(0.25f, 0.75f, 0.25f),
                                                             new Vector3(0.25f, 0.25f, 0.75f),
                                                             new Vector3(0.75f, 0.75f, 0.25f),
                                                             new Vector3(0.25f, 0.75f, 0.75f),
                                                             new Vector3(0.75f, 0.25f, 0.75f),
                                                             new Vector3(0.75f, 0.75f, 0.75f)
                                                         };

        private readonly List<BlockChangeInfo>          blockChanges = new List<BlockChangeInfo>();
        private          float                          damage;
        private          ItemActionTerrainTool.EnumMode mode;

        public override void ReadFrom(DynamicProperties _props)
        {
            base.ReadFrom(_props);

            if(!_props.Values.ContainsKey("Mode"))
                return;

            mode = EnumUtils.Parse<ItemActionTerrainTool.EnumMode>(_props.Values["Mode"]);
        }

        public override ItemActionData CreateModifierData(
            ItemInventoryData _invData,
            int               _indexInEntityOfAction)
        {
            return new CTATInventoryData(_invData, _indexInEntityOfAction, null);
        }

        public override int GetInitialMeta(ItemValue _itemValue)
        {
            return 0;
        }

        public override void StartHolding(ItemActionData _actionData)
        {
            showSphere(_actionData);
        }

        public override void StopHolding(ItemActionData _actionData)
        {
            ((CTATInventoryData)_actionData).bActivated = false;
            hideSphere(_actionData);
        }

        private void showSphere(ItemActionData _actionData)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            Ray               lookRay         = myInventoryData.invData.holdingEntity.GetLookRay();

            myInventoryData.model.transform.position =
                lookRay.origin + lookRay.direction * myInventoryData.sphereDistance - Origin.position;

            myInventoryData.model.transform.rotation = Quaternion.identity;
            myInventoryData.model.SetActive(true);
            myInventoryData.model.transform.parent = null;

            myInventoryData.model.transform.localScale = new Vector3(myInventoryData.sphereSize * 2f,
                                                                     myInventoryData.sphereSize * 2f,
                                                                     myInventoryData.sphereSize * 2f);

            myInventoryData.model.layer    = 0;
            myInventoryData.material.color = new Color(0.0f, 0.0f, 1f, 0.1f);
        }

        private void hideSphere(ItemActionData _actionData)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            myInventoryData.model.SetActive(false);
            myInventoryData.model.transform.parent = myInventoryData.invData.model;
        }

        public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;

            if(_bReleased)
            {
                myInventoryData.bActivated = false;

                GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId,
                                                             myInventoryData.invData.slotIdx,
                                                             myInventoryData.indexInEntityOfAction, 0, Vector3.zero,
                                                             Vector3.zero);
            }
            else
            {
                myInventoryData.bActivated   = true;
                myInventoryData.activateTime = Time.time;

                GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId,
                                                             myInventoryData.invData.slotIdx,
                                                             myInventoryData.indexInEntityOfAction, 1, Vector3.zero,
                                                             Vector3.zero);
            }
        }

        public override void ItemActionEffects(
            GameManager    _gameManager,
            ItemActionData _actionData,
            int            _firingState,
            Vector3        _startPos,
            Vector3        _direction,
            int            _userData = 0)
        {
            ItemActionFiringState actionFiringState = (ItemActionFiringState)_firingState;
            CTATInventoryData     myInventoryData   = (CTATInventoryData)_actionData;

            switch(actionFiringState)
            {
                case ItemActionFiringState.Off:
                    myInventoryData.material.color = new Color(0.0f, 0.0f, 1f, 0.1f);

                    break;
                case ItemActionFiringState.Start:
                    myInventoryData.material.color = new Color(1f, 0.0f, 0.0f, 0.1f);

                    break;
            }
        }

        public override bool IsActionRunning(ItemActionData _actionData)
        {
            return ((CTATInventoryData)_actionData).bActivated;
        }

        public override void OnHoldingUpdate(ItemActionData _actionData)
        {
            if(!(_actionData.invData.holdingEntity is EntityPlayerLocal))
                return;

            EntityPlayerLocal holdingEntity = _actionData.invData.holdingEntity as EntityPlayerLocal;

            if(!holdingEntity.HitInfo.bHitValid)
                return;

            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            Vector3           pos             = holdingEntity.HitInfo.hit.pos;

            myInventoryData.model.transform.position =
                Vector3.Lerp(myInventoryData.model.transform.position, pos - Origin.position, 0.5f);

            if(!myInventoryData.bActivated || Time.time - (double)myInventoryData.lastHitTime <= 0.5)
                return;

            myInventoryData.lastHitTime = Time.time;

            if(mode == ItemActionTerrainTool.EnumMode.Grow)
                setTerrain(_actionData, pos, damage, damageMultiplier);
            else
                removeTerrain3D(_actionData, pos, damage, damageMultiplier);
        }

        public override float GetRange(ItemActionData _actionData)
        {
            return 20f;
        }

        private int addTerrain2D(
            ItemActionData   _actionData,
            Vector3          _worldPos,
            float            _damage           = 0.0f,
            DamageMultiplier _damageMultiplier = null,
            bool             _bChangeBlocks    = true)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            int               num1 = Utils.Fastfloor((float)(_worldPos.x - (double)myInventoryData.sphereSize - 1.0));
            int               num2 = Utils.Fastfloor((float)(_worldPos.x + (double)myInventoryData.sphereSize + 1.0));
            int               num3 = Utils.Fastfloor((float)(_worldPos.z - (double)myInventoryData.sphereSize - 1.0));
            int               num4 = Utils.Fastfloor((float)(_worldPos.z + (double)myInventoryData.sphereSize + 1.0));
            Vector3           vector3 = _worldPos;
            vector3.y = 0.0f;
            blockChanges.Clear();

            for(int index1 = num1; index1 <= num2; ++index1)
            {
                for(int index2 = num3; index2 <= num4; ++index2)
                {
                    int        height = myInventoryData.invData.world.GetHeight(index1, index2);
                    BlockValue block  = myInventoryData.invData.world.GetBlock(index1, height, index2);

                    if(!block.Block.shape.IsTerrain())
                    {
                        --height;
                        block = myInventoryData.invData.world.GetBlock(index1, height - 1, index2);
                    }

                    float num5 = height +
                                 Mathf.Lerp(0.0f, 1f,
                                            myInventoryData.invData.world.GetDensity(0, index1, height, index2) /
                                            (float)MarchingCubes.DensityTerrain);

                    float magnitude = (vector3 - new Vector3(index1 + 0.5f, 0.0f, index2 + 0.5f))
                       .magnitude;

                    float num6;

                    if(magnitude <= (double)myInventoryData.sphereSize)
                    {
                        num6 = num5 + 0.5f;
                    }
                    else if(magnitude - (double)myInventoryData.sphereSize < 1.0)
                    {
                        float num7 = (float)(1.0 - (magnitude - (double)myInventoryData.sphereSize) / 1.0);
                        num6 = num5 + 0.5f * num7;
                    }
                    else
                    {
                        continue;
                    }

                    for(int _y = height - 1; _y <= num6 + 1.0; ++_y)
                    {
                        BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
                        blockChangeInfo.pos               = new Vector3i(index1, _y, index2);
                        blockChangeInfo.bChangeDensity    = true;
                        blockChangeInfo.bChangeBlockValue = true;

                        if(_y < (int)num6)
                        {
                            blockChangeInfo.density         = MarchingCubes.DensityTerrain;
                            blockChangeInfo.blockValue.type = block.type;
                        }
                        else
                        {
                            float num7 = num6 - (int)num6;

                            if((int)num6 == _y)
                            {
                                blockChangeInfo.density         = (sbyte)(MarchingCubes.DensityTerrain * (double)num7);
                                blockChangeInfo.blockValue.type = block.type;
                            }
                            else
                            {
                                blockChangeInfo.density =
                                    (sbyte)(MarchingCubes.DensityAir * (1.0 - num7));

                                blockChangeInfo.blockValue.type = 0;
                            }
                        }

                        blockChanges.Add(blockChangeInfo);
                    }
                }
            }

            myInventoryData.invData.world.SetBlocksRPC(blockChanges);

            return 0;
        }

        private int setTerrain(
            ItemActionData   _actionData,
            Vector3          _worldPos,
            float            _damage           = 0.0f,
            DamageMultiplier _damageMultiplier = null,
            bool             _bChangeBlocks    = true)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            int               num1            = Utils.Fastfloor(_worldPos.x - myInventoryData.sphereSize);
            int               num2            = Utils.Fastfloor(_worldPos.x + myInventoryData.sphereSize);
            int               num3            = Utils.Fastfloor(_worldPos.y - myInventoryData.sphereSize);
            int               num4            = Utils.Fastfloor(_worldPos.y + myInventoryData.sphereSize);
            int               num5            = Utils.Fastfloor(_worldPos.z - myInventoryData.sphereSize);
            int               num6            = Utils.Fastfloor(_worldPos.z + myInventoryData.sphereSize);
            blockChanges.Clear();

            for(int _x = num1; _x <= num2; ++_x)
            {
                for(int _y = num3; _y <= num4; ++_y)
                {
                    for(int _z = num5; _z <= num6; ++_z)
                    {
                        int num7 = INNER_POINTS.Length;

                        if(num7 != 0)
                        {
                            Vector3i   vector3i   = new Vector3i(_x, _y, _z);
                            BlockValue block      = myInventoryData.invData.world.GetBlock(vector3i);
                            BlockValue blockValue = block;
                            sbyte      density    = myInventoryData.invData.world.GetDensity(0, vector3i);
                            sbyte      num8       = density;

                            if(num7 > INNER_POINTS.Length / 2 || block.Block.shape.IsTerrain())
                            {
                                blockValue.type = 1;

                                num8 = (sbyte)(MarchingCubes.DensityTerrain *
                                               (double)(num7 - INNER_POINTS.Length / 2 - 1) /
                                               (INNER_POINTS.Length / 2));
                            }
                            else if(block.isair)
                            {
                                num8 = (sbyte)(MarchingCubes.DensityAir *
                                               (double)(INNER_POINTS.Length / 2 - num7) /
                                               (INNER_POINTS.Length / 2));

                                if(num8 >= 0)
                                    num8 = -1;
                            }

                            if(blockValue.type != block.type || num8 < density)
                            {
                                BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
                                blockChangeInfo.pos            = vector3i;
                                blockChangeInfo.bChangeDensity = true;
                                blockChangeInfo.density        = num8;

                                if(blockValue.type != block.type)
                                {
                                    blockChangeInfo.bChangeBlockValue = true;
                                    blockChangeInfo.blockValue        = blockValue;
                                }

                                blockChanges.Add(blockChangeInfo);
                            }
                        }
                    }
                }
            }

            myInventoryData.invData.world.SetBlocksRPC(blockChanges);

            return 0;
        }

        private int removeTerrain3D(
            ItemActionData   _actionData,
            Vector3          _worldPos,
            float            _damage           = 0.0f,
            DamageMultiplier _damageMultiplier = null,
            bool             _bChangeBlocks    = true)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;
            int               num1            = Utils.Fastfloor(_worldPos.x - myInventoryData.sphereSize);
            int               num2            = Utils.Fastfloor(_worldPos.x + myInventoryData.sphereSize);
            int               num3            = Utils.Fastfloor(_worldPos.y - myInventoryData.sphereSize);
            int               num4            = Utils.Fastfloor(_worldPos.y + myInventoryData.sphereSize);
            int               num5            = Utils.Fastfloor(_worldPos.z - myInventoryData.sphereSize);
            int               num6            = Utils.Fastfloor(_worldPos.z + myInventoryData.sphereSize);
            blockChanges.Clear();

            for(int _x = num1; _x <= num2; ++_x)
            {
                for(int _y = num3; _y <= num4; ++_y)
                {
                    for(int _z = num5; _z <= num6; ++_z)
                    {
                        int num7 = INNER_POINTS.Length;

                        if(num7 != 0)
                        {
                            Vector3i   vector3i = new Vector3i(_x, _y, _z);
                            BlockValue block    = myInventoryData.invData.world.GetBlock(vector3i);

                            if(block.Block.shape.IsTerrain())
                            {
                                BlockValue blockValue = block;
                                sbyte      density    = myInventoryData.invData.world.GetDensity(0, vector3i);
                                sbyte      num8       = density;

                                if(num7 > INNER_POINTS.Length / 2)
                                {
                                    blockValue = BlockValue.Air;

                                    num8 = (sbyte)(MarchingCubes.DensityAir *
                                                   (double)(num7 - INNER_POINTS.Length / 2 - 1) /
                                                   (INNER_POINTS.Length / 2));

                                    if(num8 <= 0)
                                        num8 = 1;
                                }
                                else if(!block.isair)
                                {
                                    num8 = (sbyte)(MarchingCubes.DensityTerrain *
                                                   (double)(INNER_POINTS.Length / 2 - num7) /
                                                   (INNER_POINTS.Length / 2));

                                    if(num8 >= 0)
                                        num8 = -1;
                                }

                                if(blockValue.type != block.type || num8 > density)
                                {
                                    BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
                                    blockChangeInfo.pos            = vector3i;
                                    blockChangeInfo.bChangeDensity = true;
                                    blockChangeInfo.density        = num8;

                                    if(blockValue.type != block.type)
                                    {
                                        blockChangeInfo.bChangeBlockValue = true;
                                        blockChangeInfo.blockValue        = blockValue;
                                    }

                                    blockChanges.Add(blockChangeInfo);
                                }
                            }
                        }
                    }
                }
            }

            myInventoryData.invData.world.SetBlocksRPC(blockChanges);

            return 0;
        }

        public override bool ConsumeScrollWheel(
            ItemActionData     _actionData,
            float              _scrollWheelInput,
            PlayerActionsLocal _playerInput)
        {
            CTATInventoryData myInventoryData = (CTATInventoryData)_actionData;

            if(!(bool)(OneAxisInputControl)_playerInput.Run)
                return false;

            myInventoryData.sphereSize = Utils.FastClamp(myInventoryData.sphereSize + _scrollWheelInput * 5f, 1f, 10f);

            myInventoryData.model.transform.localScale = new Vector3(myInventoryData.sphereSize * 2f,
                                                                     myInventoryData.sphereSize * 2f,
                                                                     myInventoryData.sphereSize * 2f);

            return true;
        }

        protected class CTATInventoryData : ItemActionDataRanged
        {
            public float      activateTime;
            public bool       bActivated;
            public float      lastHitTime;
            public Material   material;
            public GameObject model;
            public float      sphereDistance = 5f;
            public float      sphereSize     = 3f;

            public CTATInventoryData(
                ItemInventoryData _invData,
                int               _indexInEntityOfAction,
                string            _particleTransform)
                : base(_invData, _indexInEntityOfAction)
            {
                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(model.transform.GetComponent<Collider>());
                model.transform.parent = null;
                model.layer            = 0;
                model.SetActive(false);
                material                                = Resources.Load<Material>("Materials/TerrainSmoothing");
                model.GetComponent<Renderer>().material = material;
            }
        }
    }
}