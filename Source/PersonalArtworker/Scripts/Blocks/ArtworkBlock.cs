using PersonalArtworker.Scripts.Data;
using PersonalArtworker.Scripts.Entities;
using PersonalArtworker.Scripts.Features;
using PersonalArtworker.Scripts.Plugins;
using SimpleFileBrowser;
using UnityEngine;

namespace PersonalArtworker.Scripts.Blocks
{
    public class ArtworkBlock : Block
    {
        private BlockActivationCommand[] commands = new BlockActivationCommand[]
                                                    {
                                                        new BlockActivationCommand("take", "hand", true),
                                                        new BlockActivationCommand("ResizeArtwork", "frames", true),
                                                        new BlockActivationCommand("ImportArtwork", "search", true),
                                                        new BlockActivationCommand("ArtworkColor", "paint_brush", true),
                                                        new BlockActivationCommand("ArtworkLockAspect", "lock", true),
                                                        new BlockActivationCommand("ArtworkLegacyInputOn", "wrench", true),
                                                    };

        public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
        {
            base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);

            if(!(_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is ArtworkEntity))
            {
                ArtworkEntity artworkEntity = new ArtworkEntity(_chunk);
                artworkEntity.localChunkPos = World.toBlock(_blockPos);
                artworkEntity.BlockPosition = _blockPos;
                _chunk.AddTileEntity(artworkEntity);
            }
        }

        public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
        {
            base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);

            if(!GameInteractions.GameInteractions.IsServer() && world.GetTileEntity(_chunk.ClrIdx, _blockPos) is ArtworkEntity artworkEntity)
            {
                artworkEntity.Reset(FastTags.none);

                if(ConnectionManager.Instance.IsClient && !GameInteractions.GameInteractions.IsServer())
                {
                    NetPackage package = NetPackageManager
                                        .GetPackage<RemoveImageNetPackage>()
                                        .Setup(artworkEntity.GetImageID(), _chunk.ClrIdx, _blockPos);

                    ConnectionManager.Instance.SendToServer(package);
                }
            }
            
            _chunk.RemoveTileEntityAt<ArtworkEntity>((World)world, World.toBlock(_blockPos));
        }

        public override void OnBlockEntityTransformAfterActivated(WorldBase       _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue,
                                                                  BlockEntityData _ebcd)
        {
            base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);

            if(GameInteractions.GameInteractions.IsServer())
            {
                return;
            }

            if(_world.GetTileEntity(_cIdx, _blockPos) is ArtworkEntity artworkEntity)
            {
                InjectFeatures(this, artworkEntity, _ebcd.transform);
            }
        }

        public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world,    BlockValue  _blockValue, int _clrIdx,
                                                                            Vector3i  _blockPos, EntityAlive _entityFocusing)
        {
            bool isLegacyInputActive = ArtworkFeatureController.IsLegacyInputActive;

            commands[5] = isLegacyInputActive
                              ? new BlockActivationCommand("ArtworkLegacyInputOn",  "wrench", true)
                              : new BlockActivationCommand("ArtworkLegacyInputOff", "wrench", true);

            return commands;
        }

        public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos,
                                              BlockValue _blockValue, EntityAlive _player)
        {
            if(_commandName == commands[0].text)
            {
                return base.OnBlockActivated(_commandName, _world, _cIdx, _blockPos, _blockValue,
                                             _player);
            }
            if(_commandName == commands[1].text)
            {
                if(_world.GetTileEntity(_cIdx, _blockPos) is ArtworkEntity artworkEntity)
                {
                    artworkEntity.ToggleResizer();
                }
            }
            else if(_commandName == commands[2].text)
            {
                GameInteractions.GameInteractions.ToggleUIInteractiveState(true);

                FileBrowser.ShowLoadDialog(success =>
                                           {
                                               GameInteractions.GameInteractions.ToggleUIInteractiveState(false);

                                               if(_world.GetTileEntity(_cIdx, _blockPos) is ArtworkEntity artworkEntity)
                                               {
                                                   artworkEntity.ApplyImage(success[0]);
                                               }
                                           },
                                           () =>
                                           {
                                               GameInteractions.GameInteractions.ToggleUIInteractiveState(false);
                                           }, FileBrowser.PickMode.Files, false, null, null,
                                           "Import Artwork", "Import");
            }
            else if(_commandName == commands[3].text)
            {
                if(_world.GetTileEntity(_cIdx, _blockPos) is ArtworkEntity entity)
                {
                    GameInteractions.GameInteractions.ToggleUIInteractiveState(true);
                    UnityColorPicker.Show(entity);
                }
            }
            else if(_commandName == commands[4].text)
            {
                if(_world.GetTileEntity(_cIdx, _blockPos) is ArtworkEntity entity)
                {
                    entity.ToggleAspectLocked();
                    bool isAspectLocked = entity.IsAspectLocked();

                    commands[4] = new BlockActivationCommand(isAspectLocked ? "ArtworkUnlockAspect" : "ArtworkLockAspect",
                                                             isAspectLocked ? "unlock" : "lock", true);
                }
            }
            else if(_commandName == commands[5].text)
            {
                ArtworkFeatureController.ToggleLegacyInput();
            }

            return true;
        }

        private void InjectFeatures(ArtworkBlock block, ArtworkEntity artworkEntity, Transform root)
        {
            ArtworkFeatureInjector.InjectFeatures(block, artworkEntity, root);
        }
    }
}