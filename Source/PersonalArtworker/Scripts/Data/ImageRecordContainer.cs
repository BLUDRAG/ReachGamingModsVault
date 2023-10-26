using System;
using System.Collections.Generic;

namespace PersonalArtworker.Scripts.Data
{
    [Serializable]
    public class ImageRecordContainer
    {
        public List<ImageRecord> Records = new List<ImageRecord>();

        public void Register(string id, int chunkIndex, Vector3i position)
        {
            ImageRecord record = Records.Find(r => r.ID == id);

            if(record is null)
            {
                record = new ImageRecord {ID = id};
                Records.Add(record);
            }

            RemoveRecordAt(chunkIndex, position);
            record.Register(chunkIndex, position);
        }
        
        public int Unregister(string id, int chunkIndex, Vector3i position)
        {
            ImageRecord record = Records.Find(r => r.ID == id);

            if(record is null)
            {
                return 0;
            }
            
            return record.Unregister(chunkIndex, position);
        }
        
        private void RemoveRecordAt(int chunkIndex, Vector3i position)
        {
            foreach(ImageRecord record in Records)
            {
                record.Unregister(chunkIndex, position);
            }
        }
    }
}