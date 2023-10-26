using PersonalArtworker.Scripts.Entities;

namespace PersonalArtworker.Scripts.Data
{
    public class ReceiveImageNetPackage : NetPackage
    {
        private byte[]   _imageData;
        private string   _guid;
        private int      _chunkIndex;
        private Vector3i _blockPosition;
        private int      _entityID;

        public ReceiveImageNetPackage Setup(byte[] imageData, string guid, int chunkIndex, Vector3i blockPosition, int entityID)
        {
            _imageData     = imageData;
            _guid          = guid;
            _chunkIndex    = chunkIndex;
            _blockPosition = blockPosition;
            _entityID      = entityID;
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            bool isServer = GameInteractions.GameInteractions.IsServer();
            
            _guid            = _reader.ReadString();
            _chunkIndex      = _reader.ReadInt32();
            _blockPosition.x = _reader.ReadInt32();
            _blockPosition.y = _reader.ReadInt32();
            _blockPosition.z = _reader.ReadInt32();
            _entityID        = _reader.ReadInt32();

            if(isServer)
            {
                return;
            }

            int length = _reader.ReadInt32();
            _imageData = _reader.ReadBytes(length);
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            bool isServer = GameInteractions.GameInteractions.IsServer();
            
            _writer.Write(_guid);
            _writer.Write(_chunkIndex);
            _writer.Write(_blockPosition.x);
            _writer.Write(_blockPosition.y);
            _writer.Write(_blockPosition.z);
            _writer.Write(_entityID);
            
            if(!isServer)
            {
                return;
            }
            
            _writer.Write(_imageData.Length);
            _writer.Write(_imageData);
        }

        public override async void ProcessPackage(World _world, GameManager _callbacks)
        {
            bool isServer = GameInteractions.GameInteractions.IsServer();

            if(isServer)
            {
                byte[] textureData = await DataManagement.LoadImage(_guid);

                if(textureData is null)
                {
                    return;
                }

                NetPackage package = NetPackageManager
                                    .GetPackage<ReceiveImageNetPackage>()
                                    .Setup(textureData, _guid, _chunkIndex, _blockPosition, _entityID);

                ConnectionManager.Instance.SendPackage(package, _attachedToEntityId:_entityID);
            }
            else
            {
                LoadImage(_world);
            }
        }

        private async void LoadImage(World world)
        {
            await DataManagement.ProcessImage(_imageData, _guid);

            if(world.GetTileEntity(_chunkIndex, _blockPosition) is ArtworkEntity artworkEntity)
            {
                artworkEntity.SetImageID(_guid);
                await artworkEntity.LoadImage();
            }
        }

        public override int GetLength()
        {
            return (sizeof(int) * 5) + (_guid.Length * 2) + _imageData.Length;
        }

        public override NetPackageDirection PackageDirection => NetPackageDirection.Both;
    }
}