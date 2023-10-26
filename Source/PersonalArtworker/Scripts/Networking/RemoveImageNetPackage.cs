namespace PersonalArtworker.Scripts.Data
{
    public class RemoveImageNetPackage : NetPackage
    {
        private string _guid;
        private int    _chunkIndex;
        private Vector3i _blockPosition;
        
        public RemoveImageNetPackage Setup(string guid, int chunkIndex, Vector3i blockPosition)
        {
            _guid          = guid;
            _chunkIndex    = chunkIndex;
            _blockPosition = blockPosition;
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            _guid = _reader.ReadString();
            _chunkIndex = _reader.ReadInt32();
            _blockPosition.x = _reader.ReadInt32();
            _blockPosition.y = _reader.ReadInt32();
            _blockPosition.z = _reader.ReadInt32();
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            _writer.Write(_guid);
            _writer.Write(_chunkIndex);
            _writer.Write(_blockPosition.x);
            _writer.Write(_blockPosition.y);
            _writer.Write(_blockPosition.z);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            DataManagement.RemoveImageServerSide(_guid, _chunkIndex, _blockPosition);
        }

        public override int GetLength()
        {
            return 1 + (sizeof(int) * 4) + (_guid.Length * 2);
        }

        public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;
    }
}