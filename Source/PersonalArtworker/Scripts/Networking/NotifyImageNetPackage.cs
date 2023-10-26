using PersonalArtworker.Scripts.Entities;

namespace PersonalArtworker.Scripts.Data
{
    public class NotifyImageNetPackage : NetPackage
    {
        private string   _guid;
        private int      _chunkIndex;
        private Vector3i _blockPosition;
        private bool     _adding;

        public NotifyImageNetPackage Setup(string guid, int chunkIndex, Vector3i blockPosition, bool adding)
        {
            _guid          = guid;
            _chunkIndex    = chunkIndex;
            _blockPosition = blockPosition;
            _adding        = adding;

            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            _guid            = _reader.ReadString();
            _chunkIndex      = _reader.ReadInt32();
            _blockPosition.x = _reader.ReadInt32();
            _blockPosition.y = _reader.ReadInt32();
            _blockPosition.z = _reader.ReadInt32();
            _adding          = _reader.ReadBoolean();
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            _writer.Write(_guid);
            _writer.Write(_chunkIndex);
            _writer.Write(_blockPosition.x);
            _writer.Write(_blockPosition.y);
            _writer.Write(_blockPosition.z);
            _writer.Write(_adding);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            World world = GameManager.Instance.World;

            if(world.IsChunkAreaLoaded(_blockPosition))
            {
                if(_world.GetTileEntity(_chunkIndex, _blockPosition) is ArtworkEntity artworkEntity && artworkEntity.GetImageID() != _guid)
                {
                    NetPackage package = NetPackageManager
                                        .GetPackage<ReceiveImageNetPackage>()
                                        .Setup(null, _guid, _chunkIndex, _blockPosition, GameInteractions.GameInteractions.GetPlayerID());

                    ConnectionManager.Instance.SendToServer(package);
                }
            }
        }

        public override int GetLength()
        {
            return (sizeof(int) * 4) + (_guid.Length * 2);
        }

        public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;
    }
}