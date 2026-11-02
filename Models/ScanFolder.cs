using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.Models
{
    public class ScanFolder
    {
        public long Id { get; private set; }
        public string Path { get; private set; }
        public DateTime AddedAt { get; private set; }
        
        public ScanFolder(long id, string path, DateTime addedAt)
        {
            Id = id;
            Path = path;
            AddedAt = addedAt;
        }
    }
}
