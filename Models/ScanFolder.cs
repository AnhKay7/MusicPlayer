using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer.Models
{
    public class ScanFolder
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public DateTime AddedAt { get; set; }
        
        public ScanFolder(long id, string path, DateTime addedAt)
        {
            Id = id;
            Path = path;
            AddedAt = addedAt;
        }
    }
}
