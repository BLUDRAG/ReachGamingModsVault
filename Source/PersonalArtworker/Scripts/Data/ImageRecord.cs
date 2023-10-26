using System;
using System.Collections.Generic;

namespace PersonalArtworker.Scripts.Data
{
    [Serializable]
    public class ImageRecord
    {
        public string              ID;
        public List<ImageLocation> Locations = new List<ImageLocation>();

        public void Register(int chunkIndex, Vector3i position)
        {
            if(!Locations.Exists(l => l.ChunkIndex == chunkIndex && l.Position == position))
            {
                Locations.Add(new ImageLocation {ChunkIndex = chunkIndex, Position = position});
            }
        }
        
        public int Unregister(int chunkIndex, Vector3i position)
        {
            Locations.RemoveAll(l => l.ChunkIndex == chunkIndex && l.Position == position);
            return Locations.Count;
        }
    }
}