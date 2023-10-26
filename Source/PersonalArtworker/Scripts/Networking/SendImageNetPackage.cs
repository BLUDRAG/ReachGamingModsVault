namespace PersonalArtworker.Scripts.Data
{
    public class SendImageNetPackage : NetPackage
    {
        private byte[] _imageData;
        private string _guid;
        private int    _chunkIndex;
        private Vector3i _blockPosition;
        
        public SendImageNetPackage Setup(byte[] imageData, string guid, int chunkIndex, Vector3i blockPosition)
        {
            _imageData     = imageData;
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
            int length = _reader.ReadInt32();
            _imageData = _reader.ReadBytes(length);
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            _writer.Write(_guid);
            _writer.Write(_chunkIndex);
            _writer.Write(_blockPosition.x);
            _writer.Write(_blockPosition.y);
            _writer.Write(_blockPosition.z);
            _writer.Write(_imageData.Length);
            _writer.Write(_imageData);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            DataManagement.ProcessImageServerSide(_imageData, _guid, _chunkIndex, _blockPosition);

            NetPackage package = NetPackageManager
                                .GetPackage<NotifyImageNetPackage>()
                                .Setup(_guid, _chunkIndex, _blockPosition, true);

            ConnectionManager.Instance.SendPackage(package);
        }

        public override int GetLength()
        {
            return 1 + (sizeof(int) * 5) + (_guid.Length * 2) + _imageData.Length;
        }

        public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;
    }
}